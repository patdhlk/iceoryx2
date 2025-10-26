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
using Iceoryx2.SafeHandles;

namespace Iceoryx2;

/// <summary>
/// WaitSet provides event multiplexing for iceoryx2 ports (Listeners, Subscribers, etc.).
/// Similar to epoll/select/kqueue, it allows waiting on multiple event sources with a single blocking call.
/// Works cross-platform on Windows (via custom implementation), macOS (kqueue), and Linux (epoll).
/// </summary>
public sealed class WaitSet : IDisposable
{
    private SafeWaitSetHandle _handle;
    private bool _disposed;

    // Keep callback delegate alive to prevent GC collection
    private Native.Iox2NativeMethods.iox2_waitset_run_callback? _nativeCallback;

    internal WaitSet(SafeWaitSetHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    /// <summary>
    /// Returns true if the WaitSet has no attachments.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            ThrowIfDisposed();
            var handle = _handle.DangerousGetHandle();
            return Native.Iox2NativeMethods.iox2_waitset_is_empty(ref handle);
        }
    }

    /// <summary>
    /// Returns the number of current attachments.
    /// </summary>
    public ulong Length
    {
        get
        {
            ThrowIfDisposed();
            var handle = _handle.DangerousGetHandle();
            return (ulong)Native.Iox2NativeMethods.iox2_waitset_len(ref handle);
        }
    }

    /// <summary>
    /// Returns the maximum number of attachments this WaitSet can hold.
    /// </summary>
    public ulong Capacity
    {
        get
        {
            ThrowIfDisposed();
            var handle = _handle.DangerousGetHandle();
            return (ulong)Native.Iox2NativeMethods.iox2_waitset_capacity(ref handle);
        }
    }

    /// <summary>
    /// Returns the signal handling mode configured for this WaitSet.
    /// </summary>
    public SignalHandlingMode SignalHandlingMode
    {
        get
        {
            ThrowIfDisposed();
            var handle = _handle.DangerousGetHandle();
            var mode = Native.Iox2NativeMethods.iox2_waitset_signal_handling_mode(ref handle);
            return (SignalHandlingMode)mode;
        }
    }

    /// <summary>
    /// Attaches a Listener for event notifications.
    /// The WaitSet will wake up when the Listener receives an event.
    /// </summary>
    /// <param name="listener">The listener to attach.</param>
    /// <returns>Result containing the guard on success, or an error.</returns>
    public Result<WaitSetGuard, Iox2Error> AttachNotification(Listener listener)
    {
        ThrowIfDisposed();
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        var waitsetHandle = _handle.DangerousGetHandle();
        var listenerHandle = listener.GetHandle();
        
        // Get file descriptor from listener
        var fd = Native.Iox2NativeMethods.iox2_listener_get_file_descriptor(ref listenerHandle);
        if (fd == IntPtr.Zero)
            return Result<WaitSetGuard, Iox2Error>.Err(Iox2Error.WaitSetAttachmentFailed);

        var result = Native.Iox2NativeMethods.iox2_waitset_attach_notification(
            ref waitsetHandle,
            fd,
            IntPtr.Zero,
            out var guardHandle);

        if (result != Native.Iox2NativeMethods.IOX2_OK)
        {
            return Result<WaitSetGuard, Iox2Error>.Err(Iox2Error.WaitSetAttachmentFailed);
        }

        return Result<WaitSetGuard, Iox2Error>.Ok(new WaitSetGuard(new SafeWaitSetGuardHandle(guardHandle)));
    }

    /// <summary>
    /// Attaches a Listener with a deadline.
    /// The WaitSet will wake up when the Listener receives an event OR when the deadline expires.
    /// </summary>
    /// <param name="listener">The listener to attach.</param>
    /// <param name="deadline">The deadline duration.</param>
    /// <returns>Result containing the guard on success, or an error.</returns>
    public Result<WaitSetGuard, Iox2Error> AttachDeadline(Listener listener, TimeSpan deadline)
    {
        ThrowIfDisposed();
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        var waitsetHandle = _handle.DangerousGetHandle();
        var listenerHandle = listener.GetHandle();
        
        // Get file descriptor from listener
        var fd = Native.Iox2NativeMethods.iox2_listener_get_file_descriptor(ref listenerHandle);
        if (fd == IntPtr.Zero)
            return Result<WaitSetGuard, Iox2Error>.Err(Iox2Error.WaitSetAttachmentFailed);

        var seconds = (ulong)deadline.TotalSeconds;
        var nanoseconds = (uint)((deadline.TotalSeconds - seconds) * 1_000_000_000);

        var result = Native.Iox2NativeMethods.iox2_waitset_attach_deadline(
            ref waitsetHandle,
            fd,
            seconds,
            nanoseconds,
            IntPtr.Zero,
            out var guardHandle);

        if (result != Native.Iox2NativeMethods.IOX2_OK)
        {
            return Result<WaitSetGuard, Iox2Error>.Err(Iox2Error.WaitSetAttachmentFailed);
        }

        return Result<WaitSetGuard, Iox2Error>.Ok(new WaitSetGuard(new SafeWaitSetGuardHandle(guardHandle)));
    }

    /// <summary>
    /// Attaches a periodic interval timer.
    /// The WaitSet will wake up at regular intervals specified by the interval duration.
    /// </summary>
    /// <param name="interval">The interval duration.</param>
    /// <returns>Result containing the guard on success, or an error.</returns>
    public Result<WaitSetGuard, Iox2Error> AttachInterval(TimeSpan interval)
    {
        ThrowIfDisposed();

        var waitsetHandle = _handle.DangerousGetHandle();
        var seconds = (ulong)interval.TotalSeconds;
        var nanoseconds = (uint)((interval.TotalSeconds - seconds) * 1_000_000_000);

        var result = Native.Iox2NativeMethods.iox2_waitset_attach_interval(
            ref waitsetHandle,
            seconds,
            nanoseconds,
            IntPtr.Zero,
            out var guardHandle);

        if (result != Native.Iox2NativeMethods.IOX2_OK)
        {
            return Result<WaitSetGuard, Iox2Error>.Err(Iox2Error.WaitSetAttachmentFailed);
        }

        return Result<WaitSetGuard, Iox2Error>.Ok(new WaitSetGuard(new SafeWaitSetGuardHandle(guardHandle)));
    }

    /// <summary>
    /// Waits for events and processes them in an infinite loop.
    /// The callback is invoked for each event, providing the attachment ID.
    /// Returns when:
    /// - A signal (SIGINT/SIGTERM) is received (if signal handling is enabled)
    /// - The callback returns CallbackProgression.Stop
    /// </summary>
    /// <param name="callback">
    /// Callback invoked for each event. Receives the WaitSetAttachmentId which must be disposed.
    /// Return CallbackProgression.Continue to keep processing, or CallbackProgression.Stop to exit.
    /// </param>
    /// <returns>Result indicating why the wait loop ended, or an error.</returns>
    public Result<WaitSetRunResult, Iox2Error> WaitAndProcess(Func<WaitSetAttachmentId, CallbackProgression> callback)
    {
        ThrowIfDisposed();
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var waitsetHandle = _handle.DangerousGetHandle();

        // Create native callback wrapper
        _nativeCallback = (attachmentIdHandle, contextPtr) =>
        {
            try
            {
                using var attachmentId = new WaitSetAttachmentId(new SafeWaitSetAttachmentIdHandle(attachmentIdHandle));
                var progression = callback(attachmentId);
                return (Native.Iox2NativeMethods.iox2_callback_progression_e)progression;
            }
            catch
            {
                // On exception, stop processing
                return Native.Iox2NativeMethods.iox2_callback_progression_e.STOP;
            }
        };

        var result = Native.Iox2NativeMethods.iox2_waitset_wait_and_process(
            ref waitsetHandle,
            _nativeCallback,
            IntPtr.Zero,
            out var runResult);

        if (result != Native.Iox2NativeMethods.IOX2_OK)
        {
            return Result<WaitSetRunResult, Iox2Error>.Err(Iox2Error.WaitSetRunFailed);
        }

        return Result<WaitSetRunResult, Iox2Error>.Ok((WaitSetRunResult)runResult);
    }

    /// <summary>
    /// Waits for ONE event and processes it, then returns.
    /// Useful for event loops where you want explicit control over each iteration.
    /// </summary>
    /// <param name="callback">
    /// Callback invoked for the event. Receives the WaitSetAttachmentId which must be disposed.
    /// Return CallbackProgression.Continue to process the event, or CallbackProgression.Stop to skip it.
    /// </param>
    /// <returns>Result indicating the outcome, or an error.</returns>
    public Result<WaitSetRunResult, Iox2Error> WaitAndProcessOnce(Func<WaitSetAttachmentId, CallbackProgression> callback)
    {
        ThrowIfDisposed();
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var waitsetHandle = _handle.DangerousGetHandle();

        // Create native callback wrapper
        _nativeCallback = (attachmentIdHandle, contextPtr) =>
        {
            try
            {
                using var attachmentId = new WaitSetAttachmentId(new SafeWaitSetAttachmentIdHandle(attachmentIdHandle));
                var progression = callback(attachmentId);
                return (Native.Iox2NativeMethods.iox2_callback_progression_e)progression;
            }
            catch
            {
                return Native.Iox2NativeMethods.iox2_callback_progression_e.STOP;
            }
        };

        var result = Native.Iox2NativeMethods.iox2_waitset_wait_and_process_once(
            ref waitsetHandle,
            _nativeCallback,
            IntPtr.Zero,
            out var runResult);

        if (result != Native.Iox2NativeMethods.IOX2_OK)
        {
            return Result<WaitSetRunResult, Iox2Error>.Err(Iox2Error.WaitSetRunFailed);
        }

        return Result<WaitSetRunResult, Iox2Error>.Ok((WaitSetRunResult)runResult);
    }

    /// <summary>
    /// Waits for ONE event with a timeout and processes it, then returns.
    /// Returns immediately if an event is available, or after the timeout if no event arrives.
    /// </summary>
    /// <param name="callback">
    /// Callback invoked for the event if one arrives. Receives the WaitSetAttachmentId which must be disposed.
    /// </param>
    /// <param name="timeout">Maximum time to wait for an event.</param>
    /// <returns>Result indicating the outcome, or an error.</returns>
    public Result<WaitSetRunResult, Iox2Error> WaitAndProcessOnce(
        Func<WaitSetAttachmentId, CallbackProgression> callback,
        TimeSpan timeout)
    {
        ThrowIfDisposed();
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var waitsetHandle = _handle.DangerousGetHandle();
        var seconds = (ulong)timeout.TotalSeconds;
        var nanoseconds = (uint)((timeout.TotalSeconds - seconds) * 1_000_000_000);

        // Create native callback wrapper
        _nativeCallback = (attachmentIdHandle, contextPtr) =>
        {
            try
            {
                using var attachmentId = new WaitSetAttachmentId(new SafeWaitSetAttachmentIdHandle(attachmentIdHandle));
                var progression = callback(attachmentId);
                return (Native.Iox2NativeMethods.iox2_callback_progression_e)progression;
            }
            catch
            {
                return Native.Iox2NativeMethods.iox2_callback_progression_e.STOP;
            }
        };

        var result = Native.Iox2NativeMethods.iox2_waitset_wait_and_process_once_with_timeout(
            ref waitsetHandle,
            _nativeCallback,
            IntPtr.Zero,
            seconds,
            nanoseconds,
            out var runResult);

        if (result != Native.Iox2NativeMethods.IOX2_OK)
        {
            return Result<WaitSetRunResult, Iox2Error>.Err(Iox2Error.WaitSetRunFailed);
        }

        return Result<WaitSetRunResult, Iox2Error>.Ok((WaitSetRunResult)runResult);
    }

    /// <summary>
    /// Stops the WaitSet, causing WaitAndProcess() to return with WaitSetRunResult.StopRequest.
    /// Can be called from another thread or from within a callback.
    /// </summary>
    public void Stop()
    {
        ThrowIfDisposed();
        var handle = _handle.DangerousGetHandle();
        Native.Iox2NativeMethods.iox2_waitset_stop(ref handle);
    }

    /// <summary>
    /// Disposes the WaitSet and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _handle?.Dispose();
            _nativeCallback = null; // Allow GC to collect callback
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WaitSet));
    }
}
