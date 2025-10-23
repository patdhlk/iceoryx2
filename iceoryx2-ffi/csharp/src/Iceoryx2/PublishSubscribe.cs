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

            Console.WriteLine($"[DEBUG] Loan returned: result={result}, sampleHandle={sampleHandle}");

            var handle = new SafeSampleHandle(sampleHandle, isMutable: true);
            var sample = new Sample<T>(handle);
            
            return Result<Sample<T>, Iox2Error>.Ok(sample);
        }
        catch (Exception)
        {
            return Result<Sample<T>, Iox2Error>.Err(Iox2Error.SampleLoanFailed);
        }
    }

    /// <summary>
    /// Send a copy of the provided managed struct via the native send-copy path.
    /// This is a fallback that avoids the loan/send lifecycle and is useful for complex types.
    /// </summary>
    public Result<Unit, Iox2Error> SendCopy<T>(T value) where T : unmanaged
    {
        ThrowIfDisposed();

        try
        {
            var publisherHandle = _handle.DangerousGetHandle();
            var size = (ulong)Marshal.SizeOf<T>();
            var tmp = Marshal.AllocHGlobal((int)size);
            try
            {
                Marshal.StructureToPtr(value, tmp, false);
                var result = Native.Iox2NativeMethods.iox2_publisher_send_copy(
                    ref publisherHandle,
                    tmp,
                    (UIntPtr)size,
                    IntPtr.Zero);

                if (result != Native.Iox2NativeMethods.IOX2_OK)
                    return Result<Unit, Iox2Error>.Err(Iox2Error.SendFailed);

                return Result<Unit, Iox2Error>.Ok(Unit.Value);
            }
            finally
            {
                Marshal.FreeHGlobal(tmp);
            }
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

            Console.WriteLine($"[DEBUG] Payload.get: sampleHandle={sampleHandle}, payloadPtr={payloadPtr}, payloadLen={payloadLen}");

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

            Console.WriteLine($"[DEBUG] Payload.set: sampleHandle={sampleHandle}, payloadPtr={payloadPtr}, payloadLen={payloadLen}");

            if (payloadPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get sample payload");

            // Ensure we don't overwrite memory unexpectedly. Marshal the structure into a temporary
            // unmanaged buffer and then copy the bytes into the payload pointer returned by native.
            var structSize = Marshal.SizeOf<T>();
            // payloadLen is the number of elements available; convert to available bytes
            var availableElements = payloadLen.ToUInt64();
            var availableBytes = availableElements * (ulong)structSize;
            if ((ulong)structSize > availableBytes)
                throw new InvalidOperationException($"Payload buffer too small: needed={structSize}, availableBytes={availableBytes} (elements={availableElements})");

            var tmp = Marshal.AllocHGlobal(structSize);
            try
            {
                Marshal.StructureToPtr(value, tmp, false);
                unsafe
                {
                    Buffer.MemoryCopy(tmp.ToPointer(), payloadPtr.ToPointer(), (long)availableBytes, (long)structSize);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tmp);
            }
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
            var sampleHandle = _handle.DangerousGetHandle();
            Console.WriteLine($"[DEBUG] About to send sample. handle={sampleHandle}");
            // Re-query payload pointer to ensure sample is still valid
            Native.Iox2NativeMethods.iox2_sample_mut_payload_mut(ref sampleHandle, out var ptrBeforeSend, out var lenBeforeSend);
            Console.WriteLine($"[DEBUG] BeforeSend payload ptr={ptrBeforeSend}, len={lenBeforeSend}");

            var result = Native.Iox2NativeMethods.iox2_sample_mut_send(
                sampleHandle,
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
