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

    /// <summary>
    /// Disposes of the resources used by the Publisher instance.
    /// </summary>
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