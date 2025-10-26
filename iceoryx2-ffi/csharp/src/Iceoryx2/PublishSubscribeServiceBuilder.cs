using Iceoryx2.SafeHandles;
using System;

namespace Iceoryx2;

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