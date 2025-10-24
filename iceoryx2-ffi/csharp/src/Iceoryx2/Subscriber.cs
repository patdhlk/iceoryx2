using System;

namespace Iceoryx2;

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