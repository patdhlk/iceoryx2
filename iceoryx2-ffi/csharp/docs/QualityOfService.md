# Quality of Service (QoS) Support in iceoryx2 C# Bindings

This document describes the Quality of Service (QoS) settings available in the iceoryx2 C# bindings and how to use them.

## Overview

Quality of Service (QoS) settings allow you to configure various aspects of service behavior, including buffer sizes, connection limits, history depth, and overflow handling. These settings help you optimize performance and reliability for your specific use case.

## Service-Level QoS Settings

These settings are configured when creating or opening a service and affect all publishers and subscribers connected to that service.

### MaxSubscribers

**Default:** `8`

Sets the maximum number of subscribers that can connect to the service simultaneously.

```csharp
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .MaxSubscribers(10)  // Allow up to 10 subscribers
    .Open("my_service")
    .Expect("Failed to open service");
```

### MaxPublishers

**Default:** `2`

Sets the maximum number of publishers that can connect to the service simultaneously.

```csharp
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .MaxPublishers(5)  // Allow up to 5 publishers
    .Open("my_service")
    .Expect("Failed to open service");
```

### SubscriberMaxBufferSize

**Default:** `2`

Sets the maximum number of samples each subscriber can buffer internally. This defines how many samples a subscriber can hold before they are consumed.

```csharp
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .SubscriberMaxBufferSize(20)  // Each subscriber can buffer 20 samples
    .Open("my_service")
    .Expect("Failed to open service");
```

**Important:** When `EnableSafeOverflow` is disabled, this value must be greater than or equal to `HistorySize`.

### SubscriberMaxBorrowedSamples

**Default:** `2`

Sets the maximum number of samples a subscriber can borrow (hold a reference to) simultaneously.

```csharp
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .SubscriberMaxBorrowedSamples(5)  // Can hold 5 samples at once
    .Open("my_service")
    .Expect("Failed to open service");
```

### HistorySize

**Default:** `0`

Sets the number of historical samples that will be delivered to late-joining subscribers. When a subscriber connects after samples have been published, it will receive up to this many recent samples.

```csharp
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .HistorySize(10)  // Deliver last 10 samples to late-joiners
    .Open("my_service")
    .Expect("Failed to open service");

var publisher = service.PublisherBuilder().Create().Expect("...");

// Publish some samples
for (ulong i = 1; i <= 10; i++) {
    publisher.SendCopy(i);
}

// Late-joining subscriber
var subscriber = service.SubscriberBuilder()
    .BufferSize(10)  // Must be >= HistorySize to receive all history
    .Create()
    .Expect("...");

// CRITICAL: Publisher must update connections to deliver history
publisher.UpdateConnections().Expect("Failed to update connections");

// Now subscriber receives the 10 historical samples
while (var sample = subscriber.Receive<ulong>()) {
    Console.WriteLine($"History: {sample.Payload}");
}
```

**Use case:** Useful for subscribers that need to catch up on missed data.

**Important:** 
- The subscriber's buffer size must be >= HistorySize to receive all historical samples
- The publisher **must call `UpdateConnections()`** after a subscriber connects to deliver history
- History is delivered only to subscribers that connect after samples have been published

### EnableSafeOverflow

**Default:** `true`

Controls the behavior when a subscriber's buffer is full:

- **`true` (Safe Overflow):** The publisher will overwrite the oldest sample in the subscriber's buffer with the newest one. This ensures publishers never block, but subscribers may miss samples.
  
- **`false` (No Safe Overflow):** The publisher will apply the "unable to deliver" strategy (typically blocking) when the subscriber's buffer is full.

```csharp
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .EnableSafeOverflow(true)  // Overwrite oldest samples when buffer is full
    .Open("my_service")
    .Expect("Failed to open service");
```

**When to use:**
- **`true`**: Real-time systems where latest data is more important than older data
- **`false`**: Systems where no data loss is acceptable

## Publisher-Level QoS Settings

These settings are configured when creating a publisher and affect only that specific publisher instance.

### MaxLoanedSamples

**Default:** `2`

Sets the maximum number of samples a publisher can loan (allocate) simultaneously. A sample is "loaned" when you call `publisher.Loan()` and becomes available again when `Send()` is called or the sample is dropped.

```csharp
var publisher = service.PublisherBuilder()
    .MaxLoanedSamples(10)  // Can loan up to 10 samples at once
    .Create()
    .Expect("Failed to create publisher");
```

**Use case:** Increase this if you need to prepare multiple samples in parallel before sending them.

## Publisher Connection Management

### UpdateConnections()

**CRITICAL for History Delivery:** When using `HistorySize`, the publisher must explicitly call `UpdateConnections()` after new subscribers connect to deliver historical samples.

```csharp
// Publish samples
for (ulong i = 1; i <= 5; i++) {
    publisher.SendCopy(i).Expect("...");
}

// Late-joining subscriber
var subscriber = service.SubscriberBuilder()
    .BufferSize(10)
    .Create()
    .Expect("...");

// REQUIRED: Update connections to deliver history
publisher.UpdateConnections().Expect("Failed to update connections");

// Now subscriber can receive the 5 historical samples
```

**When to call:**
- After creating a new subscriber when history is configured
- Explicitly when you detect new subscriber connections
- **Not required** if you're only using current samples (no history)

**Auto-Update:** `UpdateConnections()` is called **implicitly** whenever:
- `publisher.Send()` is called
- `publisher.SendCopy()` is called  
- `sample.Send()` is called

**Manual vs Automatic:**
- **Manual call required:** When subscriber connects and no samples are being sent
- **Automatic:** When samples are actively being published (connections updated on each send)

## Complete Example

Here's a complete example demonstrating all QoS settings:

```csharp
using Iceoryx2;

var node = NodeBuilder.New()
    .Name("qos_example")
    .Create()
    .Expect("Failed to create node");

// Configure service with comprehensive QoS settings
var service = node.ServiceBuilder()
    .PublishSubscribe<ulong>()
    .MaxSubscribers(5)              // Max 5 subscribers
    .MaxPublishers(2)               // Max 2 publishers
    .SubscriberMaxBufferSize(10)    // 10 samples per subscriber buffer
    .SubscriberMaxBorrowedSamples(3) // Hold 3 samples at once
    .HistorySize(5)                 // Keep 5 samples for late-joiners
    .EnableSafeOverflow(true)       // Overwrite oldest when full
    .Open("my_service")
    .Expect("Failed to open service");

// Create publisher with custom QoS
var publisher = service.PublisherBuilder()
    .MaxLoanedSamples(5)            // Loan up to 5 samples
    .Create()
    .Expect("Failed to create publisher");

// Create subscriber (uses service-level QoS)
var subscriber = service.CreateSubscriber()
    .Expect("Failed to create subscriber");
```

## QoS Settings Summary Table

| Setting                      | Level     | Default | Description                                    |
|------------------------------|-----------|---------|------------------------------------------------|
| MaxSubscribers               | Service   | 8       | Maximum concurrent subscribers                 |
| MaxPublishers                | Service   | 2       | Maximum concurrent publishers                  |
| SubscriberMaxBufferSize      | Service   | 2       | Samples per subscriber buffer                  |
| SubscriberMaxBorrowedSamples | Service   | 2       | Concurrent borrows allowed per subscriber      |
| HistorySize                  | Service   | 0       | Samples delivered to late-joiners              |
| EnableSafeOverflow           | Service   | true    | Overwrite vs block when buffer is full         |
| MaxLoanedSamples             | Publisher | 2       | Concurrent loans allowed per publisher         |

## Best Practices

1. **Buffer Sizing**: Set `SubscriberMaxBufferSize` based on your subscriber's processing rate relative to the publishing rate. If subscribers are slower than publishers, increase the buffer size to avoid data loss (with safe overflow) or backpressure (without safe overflow).

2. **History for Late-Joiners**: Use `HistorySize` when subscribers need to catch up on recent data upon connection. This is particularly useful for state updates or slowly changing values.

3. **Safe Overflow**: Enable safe overflow (`true`) for real-time systems where the latest data is more important. Disable it (`false`) for systems where every sample must be processed.

4. **Connection Limits**: Set `MaxSubscribers` and `MaxPublishers` based on your system's requirements. Higher values consume more shared memory.

5. **Loaned Samples**: Increase `MaxLoanedSamples` only if you need to prepare multiple samples in parallel. For most use cases, the default is sufficient.

## See Also

- [QualityOfService Example](/examples/QualityOfService/Program.cs) - Complete working example demonstrating all QoS features
- [iceoryx2 Documentation](https://iceoryx.io/v2.0.0/) - Official iceoryx2 documentation
