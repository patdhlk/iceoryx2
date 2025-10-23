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
using Microsoft.Win32.SafeHandles;

namespace Iceoryx2;

/// <summary>
/// Safe handle for native iceoryx2 resources.
/// Ensures proper cleanup of native resources even if Dispose is not called.
/// </summary>
internal abstract class SafeIox2Handle : SafeHandleZeroOrMinusOneIsInvalid
{
    protected SafeIox2Handle() : base(true)
    {
    }

    protected SafeIox2Handle(IntPtr handle) : base(true)
    {
        SetHandle(handle);
    }
}

/// <summary>
/// Safe handle for Node resources.
/// </summary>
internal sealed class SafeNodeHandle : SafeIox2Handle
{
    public SafeNodeHandle() : base()
    {
    }

    public SafeNodeHandle(IntPtr handle) : base(handle)
    {
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid && handle != IntPtr.Zero)
        {
            Native.Iox2NativeMethods.iox2_node_drop(handle);
            return true;
        }
        return false;
    }
}

/// <summary>
/// Safe handle for Service resources (Port Factory).
/// </summary>
internal sealed class SafeServiceHandle : SafeIox2Handle
{
    public SafeServiceHandle() : base()
    {
    }

    public SafeServiceHandle(IntPtr handle) : base(handle)
    {
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid && handle != IntPtr.Zero)
        {
            Native.Iox2NativeMethods.iox2_port_factory_pub_sub_drop(handle);
            return true;
        }
        return false;
    }
}

/// <summary>
/// Safe handle for Publisher resources.
/// </summary>
internal sealed class SafePublisherHandle : SafeIox2Handle
{
    public SafePublisherHandle() : base()
    {
    }

    public SafePublisherHandle(IntPtr handle) : base(handle)
    {
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid && handle != IntPtr.Zero)
        {
            Native.Iox2NativeMethods.iox2_publisher_drop(handle);
            return true;
        }
        return false;
    }
}

/// <summary>
/// Safe handle for Subscriber resources.
/// </summary>
internal sealed class SafeSubscriberHandle : SafeIox2Handle
{
    public SafeSubscriberHandle() : base()
    {
    }

    public SafeSubscriberHandle(IntPtr handle) : base(handle)
    {
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid && handle != IntPtr.Zero)
        {
            Native.Iox2NativeMethods.iox2_subscriber_drop(handle);
            return true;
        }
        return false;
    }
}

/// <summary>
/// Safe handle for Sample resources.
/// </summary>
internal sealed class SafeSampleHandle : SafeIox2Handle
{
    private readonly bool _isMutable;

    public SafeSampleHandle(bool isMutable = false) : base()
    {
        _isMutable = isMutable;
    }

    public SafeSampleHandle(IntPtr handle, bool isMutable = false) : base(handle)
    {
        _isMutable = isMutable;
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid && handle != IntPtr.Zero)
        {
            if (_isMutable)
                Native.Iox2NativeMethods.iox2_sample_mut_drop(handle);
            else
                Native.Iox2NativeMethods.iox2_sample_drop(handle);
            return true;
        }
        return false;
    }
}
