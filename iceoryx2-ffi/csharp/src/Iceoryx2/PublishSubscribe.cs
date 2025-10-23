// Copyright (c) 2025 Contributors to the Eclipse Foundation
//
// See the NOTICE file(s) distributed with this work for additional
// information regarding copyright ownership.
//
// This program and the accompanying materials are made available under the
// terms of the Apache Software License 2.0 which is available at
// https://www.apache.org/licenses/LICENSE-2.0, or the MIT license
// which is available at https://opensource.org/licenses/MIT.
//
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Runtime.InteropServices;

namespace Iceoryx2;

/// <summary>
/// A publisher that can send data samples to subscribers.
/// </summary>
public sealed class Publisher : IDisposable
{
    private SafePublisherHandle _handle;
    private bool _disposed;

    internal Publisher(SafePublisherHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    /// <summary>
    /// Loans a sample for sending data.
    /// </summary>
    public Result<Sample<T>, Iox2Error> Loan<T>() where T : unmanaged
    {
        ThrowIfDisposed();
        
        try
        {
            // Loan sample - pass by reference for publisher handle
            var publisherHandle = _handle.DangerousGetHandle();
            var result = Native.Iox2NativeMethods.iox2_publisher_loan_slice_uninit(
                ref publisherHandle,  // Pass by reference - C expects pointer to handle
                IntPtr.Zero,  // NULL - let C allocate the struct
                out var sampleHandle,
                (UIntPtr)1);  // size_t in C = UIntPtr in C#
            
            if (result != Native.Iox2NativeMethods.IOX2_OK || sampleHandle == IntPtr.Zero)
                return Result<Sample<T>, Iox2Error>.Err(Iox2Error.SampleLoanFailed);
            
            var handle = new SafeSampleHandle(sampleHandle, isMutable: true);
            var sample = new Sample<T>(handle);
            
            return Result<Sample<T>, Iox2Error>.Ok(sample);
        }
        catch (Exception)
        {
            return Result<Sample<T>, Iox2Error>.Err(Iox2Error.SampleLoanFailed);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _handle?.Dispose();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Publisher));
    }
}

/// <summary>
/// A subscriber that can receive data samples from publishers.
/// </summary>
public sealed class Subscriber : IDisposable
{
    private SafeSubscriberHandle _handle;
    private bool _disposed;

    internal Subscriber(SafeSubscriberHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    /// <summary>
    /// Receives a sample if one is available.
    /// </summary>
    public Result<Sample<T>?, Iox2Error> Receive<T>() where T : unmanaged
    {
        ThrowIfDisposed();
        
        try
        {
            // Receive sample - pass by reference for subscriber handle
            var subscriberHandle = _handle.DangerousGetHandle();
            
            // Debug: Log the handle value
            Console.WriteLine($"[DEBUG] Calling receive with handle: {subscriberHandle}");
            
            var result = Native.Iox2NativeMethods.iox2_subscriber_receive(
                ref subscriberHandle,  // Pass by reference - C expects pointer to handle
                IntPtr.Zero,  // NULL - let C allocate the struct
                out var sampleHandle);
            
            Console.WriteLine($"[DEBUG] Receive returned: result={result}, sampleHandle={sampleHandle}");
            
            // No sample available is not an error
            if (result != Native.Iox2NativeMethods.IOX2_OK)
            {
                if (sampleHandle == IntPtr.Zero)
                    return Result<Sample<T>?, Iox2Error>.Ok(null);
                return Result<Sample<T>?, Iox2Error>.Err(Iox2Error.ReceiveFailed);
            }
            
            if (sampleHandle == IntPtr.Zero)
                return Result<Sample<T>?, Iox2Error>.Ok(null);
            
            var handle = new SafeSampleHandle(sampleHandle, isMutable: false);
            var sample = new Sample<T>(handle);
            
            return Result<Sample<T>?, Iox2Error>.Ok(sample);
        }
        catch (Exception)
        {
            return Result<Sample<T>?, Iox2Error>.Err(Iox2Error.ReceiveFailed);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _handle?.Dispose();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Subscriber));
    }
}

/// <summary>
/// Represents a data sample that can be sent or received.
/// </summary>
public sealed class Sample<T> : IDisposable where T : unmanaged
{
    private SafeSampleHandle _handle;
    private bool _disposed;

    internal Sample(SafeSampleHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    /// <summary>
    /// Gets or sets the payload data.
    /// </summary>
    public unsafe T Payload
    {
        get
        {
            ThrowIfDisposed();
            var sampleHandle = _handle.DangerousGetHandle();
            Native.Iox2NativeMethods.iox2_sample_payload(
                ref sampleHandle,  // Pass by reference - C expects pointer to handle
                out var payloadPtr,
                out var payloadLen);
            
            if (payloadPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get sample payload");
            
            return Marshal.PtrToStructure<T>(payloadPtr);
        }
        set
        {
            ThrowIfDisposed();
            var sampleHandle = _handle.DangerousGetHandle();
            Native.Iox2NativeMethods.iox2_sample_mut_payload_mut(
                ref sampleHandle,  // Pass by reference - C expects pointer to handle
                out var payloadPtr,
                out var payloadLen);
            
            if (payloadPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get sample payload");
            
            Marshal.StructureToPtr(value, payloadPtr, false);
        }
    }

    /// <summary>
    /// Sends the sample to all subscribers.
    /// </summary>
    public Result<Unit, Iox2Error> Send()
    {
        ThrowIfDisposed();
        
        try
        {
            var result = Native.Iox2NativeMethods.iox2_sample_mut_send(
                _handle.DangerousGetHandle(),
                IntPtr.Zero);
            
            if (result != Native.Iox2NativeMethods.IOX2_OK)
                return Result<Unit, Iox2Error>.Err(Iox2Error.SendFailed);

            // The handle is consumed by send
            _handle.SetHandleAsInvalid();
            _disposed = true;
            
            return Result<Unit, Iox2Error>.Ok(Unit.Value);
        }
        catch (Exception)
        {
            return Result<Unit, Iox2Error>.Err(Iox2Error.SendFailed);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _handle?.Dispose();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Sample<T>));
    }
}

/// <summary>
/// Represents a unit type (similar to Rust's () or void but as a value).
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}
