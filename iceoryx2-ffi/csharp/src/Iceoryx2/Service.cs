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
/// Represents a service in the iceoryx2 system.
/// Services are created with a specific messaging pattern (e.g., publish-subscribe).
/// </summary>
public sealed class Service : IDisposable
{
    private SafeServiceHandle _handle;
    private bool _disposed;

    internal Service(SafeServiceHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    /// <summary>
    /// Creates a publisher for this service.
    /// </summary>
    public Result<Publisher, Iox2Error> CreatePublisher()
    {
        ThrowIfDisposed();
        
        try
        {
            // Create publisher builder - pass by reference for handle
            var portFactoryHandle = _handle.DangerousGetHandle();
            var publisherBuilderHandle = Native.Iox2NativeMethods.iox2_port_factory_pub_sub_publisher_builder(
                ref portFactoryHandle,  // Pass by reference - C expects pointer to handle
                IntPtr.Zero);  // NULL - let C allocate the struct
            
            if (publisherBuilderHandle == IntPtr.Zero)
                return Result<Publisher, Iox2Error>.Err(Iox2Error.PublisherCreationFailed);

            // Create publisher - pass NULL to let C allocate on heap
            var result = Native.Iox2NativeMethods.iox2_port_factory_publisher_builder_create(
                publisherBuilderHandle,
                IntPtr.Zero,  // NULL - let C allocate the struct
                out var publisherHandle);
            
            if (result != Native.Iox2NativeMethods.IOX2_OK || publisherHandle == IntPtr.Zero)
                return Result<Publisher, Iox2Error>.Err(Iox2Error.PublisherCreationFailed);
            
            var handle = new SafePublisherHandle(publisherHandle);
            var publisher = new Publisher(handle);
            
            return Result<Publisher, Iox2Error>.Ok(publisher);
        }
        catch (Exception)
        {
            return Result<Publisher, Iox2Error>.Err(Iox2Error.PublisherCreationFailed);
        }
    }

    /// <summary>
    /// Creates a subscriber for this service.
    /// </summary>
    public Result<Subscriber, Iox2Error> CreateSubscriber()
    {
        ThrowIfDisposed();
        
        try
        {
            // Create subscriber builder - pass by reference for handle
            var portFactoryHandle = _handle.DangerousGetHandle();
            var subscriberBuilderHandle = Native.Iox2NativeMethods.iox2_port_factory_pub_sub_subscriber_builder(
                ref portFactoryHandle,  // Pass by reference - C expects pointer to handle
                IntPtr.Zero);  // NULL - let C allocate the struct
            
            if (subscriberBuilderHandle == IntPtr.Zero)
                return Result<Subscriber, Iox2Error>.Err(Iox2Error.SubscriberCreationFailed);

            // Create subscriber - pass NULL to let C allocate on heap
            var result = Native.Iox2NativeMethods.iox2_port_factory_subscriber_builder_create(
                subscriberBuilderHandle,
                IntPtr.Zero,  // NULL - let C allocate the struct
                out var subscriberHandle);
            
            if (result != Native.Iox2NativeMethods.IOX2_OK || subscriberHandle == IntPtr.Zero)
                return Result<Subscriber, Iox2Error>.Err(Iox2Error.SubscriberCreationFailed);
            
            var handle = new SafeSubscriberHandle(subscriberHandle);
            var subscriber = new Subscriber(handle);
            
            return Result<Subscriber, Iox2Error>.Ok(subscriber);
        }
        catch (Exception)
        {
            return Result<Subscriber, Iox2Error>.Err(Iox2Error.SubscriberCreationFailed);
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
            throw new ObjectDisposedException(nameof(Service));
    }
}

/// <summary>
/// Builder for creating or opening a service.
/// </summary>
public sealed class ServiceBuilder
{
    private readonly Node _node;

    internal ServiceBuilder(Node node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>
    /// Creates a publish-subscribe service builder.
    /// </summary>
    public PublishSubscribeServiceBuilder<T> PublishSubscribe<T>() where T : unmanaged
    {
        return new PublishSubscribeServiceBuilder<T>(_node);
    }

    /// <summary>
    /// Gets a Rust-compatible type name for cross-language interoperability.
    /// Maps .NET types to their Rust equivalents for iceoryx2 type matching.
    /// </summary>
    internal static string GetRustCompatibleTypeName<T>() where T : unmanaged
    {
        var type = typeof(T);

        // Map .NET primitive types to Rust type names (unchanged)
        if (type == typeof(byte)) return "u8";
        if (type == typeof(sbyte)) return "i8";
        if (type == typeof(short)) return "i16";
        if (type == typeof(ushort)) return "u16";
        if (type == typeof(int)) return "i32";
        if (type == typeof(uint)) return "u32";
        if (type == typeof(long)) return "i64";
        if (type == typeof(ulong)) return "u64";
        if (type == typeof(float)) return "f32";
        if (type == typeof(double)) return "f64";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(char)) return "char";

        // For custom structs, check for a custom Iox2TypeAttribute and then
        // return the C-style length-prefixed name (e.g. "16TransmissionData").
        var typeAttr = type.GetCustomAttributes(typeof(Iox2TypeAttribute), false);
        string baseName;
        if (typeAttr.Length > 0 && typeAttr[0] is Iox2TypeAttribute iox2Attr)
        {
            baseName = iox2Attr.TypeName;
        }
        else
        {
            baseName = type.Name;
        }

        // If the user already provided a length-prefixed name (starts with digits),
        // assume it's already in the correct format and return as-is. Otherwise
        // prefix with the UTF-8 byte length of the base name.
        if (!string.IsNullOrEmpty(baseName) && char.IsDigit(baseName[0]))
        {
            return baseName;
        }

        // Use UTF-8 byte count for the base name length (matches C strlen behavior for ASCII/UTF-8)
        var byteCount = System.Text.Encoding.UTF8.GetByteCount(baseName);
        return $"{byteCount}{baseName}";
    }
}

/// <summary>
/// Builder for publish-subscribe services.
/// </summary>
public sealed class PublishSubscribeServiceBuilder<T> where T : unmanaged
{
    private readonly Node _node;
    private string? _serviceName;

    internal PublishSubscribeServiceBuilder(Node node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>
    /// Opens an existing service or creates a new one with the specified name.
    /// </summary>
    public Result<Service, Iox2Error> Open(string serviceName)
    {
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        
        try
        {
            // Create service name
            var serviceNameBytes = System.Text.Encoding.UTF8.GetByteCount(_serviceName);
            
            var result = Native.Iox2NativeMethods.iox2_service_name_new(
                IntPtr.Zero,  // pass IntPtr.Zero to use default storage allocation
                _serviceName,
                serviceNameBytes,
                out var serviceNameHandle);
            
            if (result != Native.Iox2NativeMethods.IOX2_OK)
                return Result<Service, Iox2Error>.Err(Iox2Error.ServiceCreationFailed);

            // Get service name ptr for builder
            var serviceNamePtr = Native.Iox2NativeMethods.iox2_cast_service_name_ptr(serviceNameHandle);

            // Create service builder - pass NULL to let C allocate on heap
            var nodeHandle = _node._handle.DangerousGetHandle();
            var serviceBuilderHandle = Native.Iox2NativeMethods.iox2_node_service_builder(
                ref nodeHandle,  // Pass by reference - C expects pointer to handle
                IntPtr.Zero,  // NULL - let C allocate the struct
                serviceNamePtr);

            // Clean up service name
            Native.Iox2NativeMethods.iox2_service_name_drop(serviceNameHandle);

            if (serviceBuilderHandle == IntPtr.Zero)
                return Result<Service, Iox2Error>.Err(Iox2Error.ServiceCreationFailed);

            // Get pub/sub builder
            var pubSubBuilderHandle = Native.Iox2NativeMethods.iox2_service_builder_pub_sub(serviceBuilderHandle);

            // Set payload type details
            // Use Rust-compatible type names for cross-language interoperability
            var typeName = ServiceBuilder.GetRustCompatibleTypeName<T>();
            Console.WriteLine($"[DEBUG] Opening service '{_serviceName}' with type name: '{typeName}'");
            unsafe
            {
                var typeSize = (ulong)sizeof(T);
                // Calculate proper alignment - for primitive types it's the size, for structs we use Marshal.StructLayout or default to pointer size
                ulong typeAlignment;
                if (typeof(T).IsPrimitive)
                {
                    typeAlignment = typeSize;
                }
                else
                {
                    // For structs, check if there's a StructLayout attribute specifying Pack
                    var layoutAttr = typeof(T).StructLayoutAttribute;
                    if (layoutAttr != null && layoutAttr.Pack > 0)
                    {
                        typeAlignment = (ulong)layoutAttr.Pack;
                    }
                    else
                    {
                        // Default to pointer size for alignment
                        typeAlignment = (ulong)IntPtr.Size;
                    }
                }
                
                Console.WriteLine($"[DEBUG] Setting payload type details: name='{typeName}', name_bytes={System.Text.Encoding.UTF8.GetByteCount(typeName)}, size={typeSize}, alignment={typeAlignment}");
                var typeResult = Native.Iox2NativeMethods.iox2_service_builder_pub_sub_set_payload_type_details(
                    ref pubSubBuilderHandle,  // Pass by reference - C expects pointer to handle
                    Native.Iox2NativeMethods.iox2_type_variant_e.FIXED_SIZE,
                    typeName,
                    System.Text.Encoding.UTF8.GetByteCount(typeName),
                    typeSize,
                    typeAlignment);

                if (typeResult != Native.Iox2NativeMethods.IOX2_OK)
                    return Result<Service, Iox2Error>.Err(Iox2Error.ServiceCreationFailed);
            }

            // Open or create the service - pass NULL to let C allocate on heap
            Console.WriteLine($"[DEBUG] Calling open_or_create with builder handle: {pubSubBuilderHandle}");
            Console.WriteLine($"[DEBUG] Builder handle as hex: 0x{pubSubBuilderHandle:X}");
            var openResult = Native.Iox2NativeMethods.iox2_service_builder_pub_sub_open_or_create(
                pubSubBuilderHandle,
                IntPtr.Zero,  // NULL - let C allocate the struct
                out var portFactoryHandle);

            Console.WriteLine($"[DEBUG] open_or_create result: {openResult} (0x{openResult:X}), port factory handle: {portFactoryHandle} (0x{portFactoryHandle:X})");
            if (openResult != Native.Iox2NativeMethods.IOX2_OK)
            {
                Console.WriteLine($"[ERROR] Service creation failed with error code: {openResult}");
                return Result<Service, Iox2Error>.Err(Iox2Error.ServiceCreationFailed);
            }
            if (portFactoryHandle == IntPtr.Zero)
            {
                Console.WriteLine($"[ERROR] Port factory handle is null despite OK result!");
                return Result<Service, Iox2Error>.Err(Iox2Error.ServiceCreationFailed);
            }

            var handle = new SafeServiceHandle(portFactoryHandle);
            var service = new Service(handle);
            
            return Result<Service, Iox2Error>.Ok(service);
        }
        catch (Exception)
        {
            return Result<Service, Iox2Error>.Err(Iox2Error.ServiceCreationFailed);
        }
    }
}
