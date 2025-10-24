using System;
using System.Threading;
using System.Threading.Tasks;
using Iceoryx2.SafeHandles;

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

    /// <summary>
    /// Asynchronously waits for a sample with a timeout by polling.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for a sample.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the wait operation.</param>
    /// <returns>A Task containing a Result with the sample if received within timeout (null if timeout), or an error.</returns>
    public async Task<Result<Sample<T>?, Iox2Error>> ReceiveAsync<T>(TimeSpan timeout, CancellationToken cancellationToken = default) where T : unmanaged
    {
        ThrowIfDisposed();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = Receive<T>();
            if (!result.IsOk)
            {
                return result;
            }

            var sample = result.Unwrap();
            if (sample != null)
            {
                return Result<Sample<T>?, Iox2Error>.Ok(sample);
            }

            // Yield to thread pool instead of blocking
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        }

        return Result<Sample<T>?, Iox2Error>.Ok(null);
    }

    /// <summary>
    /// Asynchronously waits for a sample indefinitely by polling.
    /// Note: This polls every 10ms since the native API doesn't have a blocking receive.
    /// The polling is efficient as it yields to the thread pool between checks.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token to cancel the wait operation.</param>
    /// <returns>A Task containing a Result with the sample or an error.</returns>
    public async Task<Result<Sample<T>, Iox2Error>> ReceiveAsync<T>(CancellationToken cancellationToken = default) where T : unmanaged
    {
        ThrowIfDisposed();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = Receive<T>();
            if (!result.IsOk)
            {
                return Result<Sample<T>, Iox2Error>.Err(Iox2Error.ReceiveFailed);
            }

            var sample = result.Unwrap();
            if (sample != null)
            {
                return Result<Sample<T>, Iox2Error>.Ok(sample);
            }

            // Yield to thread pool instead of blocking
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// Ensures proper cleanup by disposing of the associated resources when the object is no longer needed.
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
            throw new ObjectDisposedException(nameof(Subscriber));
    }
}