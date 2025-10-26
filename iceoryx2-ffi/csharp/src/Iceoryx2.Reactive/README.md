# Iceoryx2.Reactive

Reactive Extensions (Rx) support for iceoryx2 - provides `IObservable<T>` pattern for declarative, composable pub/sub communication.

## Overview

`Iceoryx2.Reactive` transforms iceoryx2's imperative polling-based subscriber into a declarative, Rx-style data stream. This enables powerful LINQ-style operators and clean async/await patterns.

### Before: Imperative Polling

```csharp
while (true)
{
    var result = subscriber.Receive<MyData>();
    if (result.IsOk)
    {
        var sample = result.Unwrap();
        if (sample.HasValue)
        {
            using var s = sample.Value;
            // Process data
            Console.WriteLine($"Received: {s.Payload}");
        }
    }
    Thread.Sleep(10);
}
```

### After: Declarative Rx Stream

```csharp
using var subscription = subscriber.AsObservable<MyData>()
    .Where(data => data.IsValid)
    .Subscribe(data => Console.WriteLine($"Received valid data: {data}"));

Console.ReadKey(); // Keep the app alive
```

## Features

- ✅ **IObservable<T>** - Full Rx integration with System.Reactive
- ✅ **LINQ Operators** - Use Where, Select, Buffer, Throttle, etc.
- ✅ **Async Streams** - IAsyncEnumerable<T> support for `await foreach`
- ✅ **Composable** - Chain and combine multiple streams
- ✅ **Cancellation** - Proper CancellationToken support
- ✅ **Resource Management** - RAII disposal patterns

## Installation

Add package reference to your project:

```xml
<ItemGroup>
  <ProjectReference Include="../path/to/Iceoryx2.Reactive/Iceoryx2.Reactive.csproj" />
</ItemGroup>
```

Or via NuGet (when published):

```bash
dotnet add package Iceoryx2.Reactive
```

## Usage

### Basic Observable

```csharp
using Iceoryx2;
using Iceoryx2.Reactive;
using System.Reactive.Linq;

var node = NodeBuilder.New().Create().Expect("Failed to create node");
var service = node.ServiceBuilder()
    .PublishSubscribe<MyData>()
    .Open("my_service")
    .Expect("Failed to open service");

var subscriber = service.CreateSubscriber()
    .Expect("Failed to create subscriber");

// Convert to observable and subscribe
using var subscription = subscriber.AsObservable<MyData>()
    .Subscribe(data => Console.WriteLine($"Received: {data}"));

Console.ReadKey();
```

### LINQ Operators

```csharp
// Filter, transform, and process
using var subscription = subscriber.AsObservable<SensorData>()
    .Where(data => data.Temperature > 25.0)
    .Select(data => new { data.Temperature, IsCritical = data.Temperature > 50.0 })
    .Subscribe(result => 
        Console.WriteLine($"Temp: {result.Temperature}°C, Critical: {result.IsCritical}"));
```

### Buffering and Throttling

```csharp
// Process in batches every 100ms
using var subscription = subscriber.AsObservable<LogEntry>()
    .Buffer(TimeSpan.FromMilliseconds(100))
    .Subscribe(batch => 
        Console.WriteLine($"Received batch of {batch.Count} log entries"));

// Throttle to max 10 items/second
using var subscription2 = subscriber.AsObservable<Event>()
    .Sample(TimeSpan.FromMilliseconds(100))
    .Subscribe(evt => ProcessEvent(evt));
```

### Multiple Subscribers (Merge)

```csharp
var obs1 = subscriber1.AsObservable<MyData>();
var obs2 = subscriber2.AsObservable<MyData>();

// Merge multiple streams
using var subscription = obs1.Merge(obs2)
    .Subscribe(data => Console.WriteLine($"From either stream: {data}"));
```

### Async Enumerable (await foreach)

```csharp
await foreach (var data in subscriber.AsAsyncEnumerable<MyData>(cancellationToken))
{
    Console.WriteLine($"Received: {data}");
    
    if (data.ShouldStop)
        break;
}
```

### Custom Polling Interval

```csharp
// Poll every 1ms for low-latency scenarios
using var subscription = subscriber.AsObservable<MyData>(
    pollingInterval: TimeSpan.FromMilliseconds(1))
    .Subscribe(data => ProcessHighFrequency(data));

// Poll every 100ms for low CPU usage
using var subscription2 = subscriber.AsObservable<MyData>(
    pollingInterval: TimeSpan.FromMilliseconds(100))
    .Subscribe(data => ProcessLowFrequency(data));
```

### With Cancellation

```csharp
using var cts = new CancellationTokenSource();

using var subscription = subscriber.AsObservable<MyData>(
    cancellationToken: cts.Token)
    .Subscribe(
        data => Console.WriteLine($"Received: {data}"),
        error => Console.WriteLine($"Error: {error}"),
        () => Console.WriteLine("Stream completed"));

// Later...
cts.Cancel(); // Stops the observable stream gracefully
```

## API Reference

### SubscriberExtensions.AsObservable<T>()

Converts a Subscriber into an `IObservable<T>` stream.

**Parameters:**
- `pollingInterval` (optional) - Polling interval (default: 10ms)
- `cancellationToken` (optional) - Token to cancel the stream

**Returns:** `IObservable<T>`

### SubscriberExtensions.AsAsyncEnumerable<T>()

Converts a Subscriber into an `IAsyncEnumerable<T>` stream.

**Parameters:**
- `pollingInterval` (optional) - Polling interval (default: 10ms)
- `cancellationToken` (optional) - Token to cancel the stream

**Returns:** `IAsyncEnumerable<T>`

## Performance Considerations

### Polling Interval Trade-offs

- **Lower interval (1-5ms)**
  - ✅ Lower latency
  - ❌ Higher CPU usage
  - Use for: Real-time systems, high-frequency data

- **Default interval (10ms)**
  - ✅ Balanced latency/CPU
  - Use for: Most applications

- **Higher interval (50-100ms)**
  - ✅ Lower CPU usage
  - ❌ Higher latency
  - Use for: Background processing, non-critical data

### CPU Usage

The observable continuously polls the subscriber. Consider:
- Adjusting `pollingInterval` based on your latency requirements
- Using operators like `Throttle()` or `Sample()` to reduce downstream processing
- Disposing subscriptions when not needed

## Examples

See the [examples directory](../../examples/) for complete working examples:
- `ObservableWaitSet` - Observable pattern with event multiplexing
- `WaitSetMultiplexing` - Async/await event handling

## Building

```bash
dotnet build
```

## Testing

```bash
dotnet test
```

## License

Dual-licensed under Apache-2.0 OR MIT
