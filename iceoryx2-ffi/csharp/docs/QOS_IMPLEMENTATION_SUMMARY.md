# Quality of Service (QoS) Feature Implementation Summary

## Overview

Comprehensive Quality of Service (QoS) support has been added to the iceoryx2 C# bindings, providing fine-grained control over service behavior, buffering, and resource limits.

## Changes Made

### 1. Native P/Invoke Declarations (`Iox2NativeMethods.cs`)

Added P/Invoke declarations for all QoS-related C FFI functions:

**Service Builder QoS:**
- `iox2_service_builder_pub_sub_set_max_subscribers`
- `iox2_service_builder_pub_sub_set_max_publishers`
- `iox2_service_builder_pub_sub_set_subscriber_max_buffer_size`
- `iox2_service_builder_pub_sub_set_subscriber_max_borrowed_samples`
- `iox2_service_builder_pub_sub_set_history_size`
- `iox2_service_builder_pub_sub_set_enable_safe_overflow`

**Publisher Builder QoS:**
- `iox2_port_factory_publisher_builder_set_max_loaned_samples`

### 2. PublishSubscribeServiceBuilder Enhancements (`PublishSubscribeServiceBuilder.cs`)

Added fluent API methods for service-level QoS configuration:

```csharp
public PublishSubscribeServiceBuilder<T> MaxSubscribers(ulong value)
public PublishSubscribeServiceBuilder<T> MaxPublishers(ulong value)
public PublishSubscribeServiceBuilder<T> SubscriberMaxBufferSize(ulong value)
public PublishSubscribeServiceBuilder<T> SubscriberMaxBorrowedSamples(ulong value)
public PublishSubscribeServiceBuilder<T> HistorySize(ulong value)
public PublishSubscribeServiceBuilder<T> EnableSafeOverflow(bool value)
```

All methods support method chaining for clean, readable configuration.

### 3. PublisherBuilder Class (New)

Created a new `PublisherBuilder` class (`PublisherBuilder.cs`) to support publisher-level QoS:

```csharp
public class PublisherBuilder
{
    public PublisherBuilder MaxLoanedSamples(ulong value)
    public Result<Publisher, Iox2Error> Create()
}
```

### 4. Service Class Updates (`Service.cs`)

Added:
- `PublisherBuilder()` method to get a builder with QoS configuration
- `GetHandle()` internal method for builder access
- Updated `CreatePublisher()` documentation to mention builder alternative

### 5. Documentation

Created comprehensive documentation:
- `docs/QualityOfService.md` - Complete QoS reference guide
- Includes descriptions, defaults, use cases, best practices
- Complete code examples for all settings
- Summary table of all QoS parameters

### 6. Example Application

Created `examples/QualityOfService/Program.cs` demonstrating:
- All service-level QoS settings
- Publisher-level QoS settings  
- Late-joiner history behavior
- Safe overflow demonstration
- Practical usage patterns

## QoS Settings Supported

### Service-Level Settings

| Setting                      | Default | Description                                |
|------------------------------|---------|--------------------------------------------|
| MaxSubscribers               | 8       | Maximum concurrent subscribers             |
| MaxPublishers                | 2       | Maximum concurrent publishers              |
| SubscriberMaxBufferSize      | 2       | Samples per subscriber buffer              |
| SubscriberMaxBorrowedSamples | 2       | Concurrent borrows per subscriber          |
| HistorySize                  | 0       | Samples for late-joiners                   |
| EnableSafeOverflow           | true    | Overwrite oldest vs block when full        |

### Publisher-Level Settings

| Setting          | Default | Description                        |
|------------------|---------|------------------------------------|
| MaxLoanedSamples | 2       | Concurrent loans per publisher     |

## Usage Example

```csharp
using Iceoryx2;

var node = NodeBuilder.New()
    .Name("qos_demo")
    .Create()
    .Expect("Failed to create node");

// Service with QoS
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .MaxSubscribers(5)
    .MaxPublishers(2)
    .SubscriberMaxBufferSize(10)
    .SubscriberMaxBorrowedSamples(3)
    .HistorySize(5)
    .EnableSafeOverflow(true)
    .Open("my_service")
    .Expect("Failed to open service");

// Publisher with QoS
var publisher = service.PublisherBuilder()
    .MaxLoanedSamples(5)
    .Create()
    .Expect("Failed to create publisher");

// Subscriber (inherits service QoS)
var subscriber = service.CreateSubscriber()
    .Expect("Failed to create subscriber");
```

## Build Status

✅ All code compiles successfully with 0 warnings, 0 errors
✅ Example application builds and is ready to test
✅ Compatible with .NET 8.0 and .NET 9.0

## Testing Recommendations

To test the QoS features:

1. **Build the example:**
   ```bash
   cd iceoryx2-ffi/csharp/examples/QualityOfService
   dotnet build
   ```

2. **Run the example:**
   ```bash
   dotnet run
   ```

3. **Verify behavior:**
   - Check that historical samples are delivered to late-joiners
   - Verify safe overflow behavior (oldest samples overwritten)
   - Test buffer size limits
   - Test connection limits (max publishers/subscribers)

## Integration with Existing Code

The QoS features are fully backward compatible:
- Existing code continues to work with default QoS settings
- New builder methods are optional
- Default `CreatePublisher()` still available for simple use cases

## Next Steps

Potential enhancements:
1. Add QoS query methods to inspect service configuration
2. Add subscriber-level buffer size configuration (if supported by C FFI)
3. Add runtime QoS monitoring/metrics
4. Add QoS validation (e.g., warn if history > buffer size with safe overflow disabled)

## Files Modified/Created

**Modified:**
- `src/Iceoryx2/Native/Iox2NativeMethods.cs` - Added P/Invoke declarations
- `src/Iceoryx2/PublishSubscribeServiceBuilder.cs` - Added QoS configuration methods
- `src/Iceoryx2/Service.cs` - Added PublisherBuilder() method

**Created:**
- `src/Iceoryx2/PublisherBuilder.cs` - New builder class for publisher QoS
- `examples/QualityOfService/Program.cs` - Comprehensive QoS example
- `examples/QualityOfService/QualityOfService.csproj` - Example project file
- `docs/QualityOfService.md` - Complete QoS documentation

## References

- [iceoryx2 Rust API Documentation](https://github.com/eclipse-iceoryx/iceoryx2) - Original QoS implementation
- [iceoryx2 C++ Bindings](https://github.com/eclipse-iceoryx/iceoryx2/tree/main/iceoryx2-cxx) - Reference implementation
- [iceoryx2 Python Bindings](https://github.com/eclipse-iceoryx/iceoryx2/tree/main/iceoryx2-ffi/python) - Similar QoS API
