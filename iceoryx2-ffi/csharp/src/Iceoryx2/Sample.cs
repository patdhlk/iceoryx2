using Iceoryx2.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Iceoryx2;

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

    private unsafe void DumpSampleMemory(string context)
    {
        var sampleHandle = _handle.DangerousGetHandle();
        Console.WriteLine($"[DUMP] {context}: sampleHandle=0x{sampleHandle.ToInt64():X}");

        // Read first 128 bytes of the sample struct to see the service_type, storage bytes, and deleter
        byte[] buffer = new byte[128];
        Marshal.Copy(sampleHandle, buffer, 0, 128);

        Console.Write($"[DUMP] First 128 bytes: ");
        for (int i = 0; i < Math.Min(128, buffer.Length); i++)
        {
            Console.Write($"{buffer[i]:X2} ");
            if ((i + 1) % 16 == 0) Console.Write(" ");
        }
        Console.WriteLine();
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
                ref sampleHandle,  // _ref type needs ref to pass pointer-to-pointer
                out var payloadPtr,
                out var payloadLen);

            if (payloadPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get sample payload");

            return Marshal.PtrToStructure<T>(payloadPtr);
        }
        set
        {
            ThrowIfDisposed();

            DumpSampleMemory("Before payload write");

            var sampleHandle = _handle.DangerousGetHandle();
            IntPtr payloadPtr;
            unsafe
            {
                // WORKAROUND: Pass NULL for number_of_elements because native code has a bug
                // where it accesses .local union variant even when service_type is IPC
                Native.Iox2NativeMethods.iox2_sample_mut_payload_mut_ptr(
                    ref sampleHandle,  // _ref type needs ref to pass pointer-to-pointer
                    out payloadPtr,
                    IntPtr.Zero);  // NULL - don't query element count due to native bug
            }

            if (payloadPtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get sample payload");

            // Ensure we don't overwrite memory unexpectedly. Marshal the structure into a temporary
            // unmanaged buffer and then copy the bytes into the payload pointer returned by native.
            var structSize = Marshal.SizeOf<T>();
            // We loaned exactly 1 element, so available bytes = structSize
            var availableBytes = (ulong)structSize;

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

            DumpSampleMemory("After payload write");
        }
    }

    /// <summary>
    /// Sends the sample to all subscribers.
    /// </summary>
    public Result<Unit, Iox2Error> Send()
    {
        ThrowIfDisposed();

        DumpSampleMemory("Before send");

        try
        {
            var sampleHandle = _handle.DangerousGetHandle();

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

    /// <summary>
    /// Releases the resources associated with the current instance of the Sample class.
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
            throw new ObjectDisposedException(nameof(Sample<T>));
    }
}