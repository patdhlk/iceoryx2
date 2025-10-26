# Async Patterns in Iceoryx2 C#

The iceoryx2 C# wrapper is designed to be **truly async-first**. Every potentially blocking or long-running operation has an async counterpart that accepts a `CancellationToken`, allowing seamless integration into modern asynchronous .NET applications without ever blocking threads.

## Design Philosophy

✅ **All async methods accept `CancellationToken`**  
✅ **Non-blocking - yields to thread pool**  
✅ **Compatible with async/await, Task.WhenAll, Task.WhenAny**  
✅ **Integrates with Reactive Extensions for declarative patterns**

⚠️ **Important: Polling vs. Event-Driven**

The `ReceiveAsync` methods are **polling-based** (not truly event-driven) because the underlying native C API doesn't expose a blocking receive operation. They poll with `Task.Delay()` to avoid blocking threads, but this is not a true async event notification.

For **truly event-driven, blocking operations**, use the **WaitSet** API, which is built on top of platform-specific event mechanisms (epoll on Linux, kqueue on macOS, custom implementation on Windows).

## Core Async APIs

### 1. Subscriber - Async Receive (Polling-Based)

⚠️ **Note**: `ReceiveAsync` is **polling-based**, not event-driven. It polls the subscriber in a loop with `Task.Delay()` between checks. For truly event-driven reception, use the **WaitSet** API (see section 4 below).

The `Subscriber` class provides async methods for receiving samples without blocking threads.

#### Receive with Timeout

```csharp
using var subscriber = service.SubscriberBuilder<SensorData>()
    .Create()
    .Expect("Failed to create subscriber");

var cts = new CancellationTokenSource();

// Wait up to 5 seconds for a sample
var result = await subscriber.ReceiveAsync<SensorData>(
    TimeSpan.FromSeconds(5), 
    cts.Token);

if (result.IsOk)
{
    var sample = result.Unwrap();
    if (sample != null)
    {
        using (sample)
        {
            Console.WriteLine($"Received: {sample.Payload}");
        }
    }
    else
    {
        Console.WriteLine("Timeout - no sample received");
    }
}
```

#### Receive Indefinitely

```csharp
var cts = new CancellationTokenSource();

try
{
    // Wait indefinitely (until cancelled)
    var result = await subscriber.ReceiveAsync<SensorData>(cts.Token);
    
    if (result.IsOk)
    {
        using var sample = result.Unwrap();
        Console.WriteLine($"Received: {sample.Payload}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Receive cancelled");
}
```

#### Polling Loop with Async/Await

```csharp
var cts = new CancellationTokenSource();

while (!cts.Token.IsCancellationRequested)
{
    var result = await subscriber.ReceiveAsync<SensorData>(
        TimeSpan.FromMilliseconds(100), 
        cts.Token);
    
    if (result.IsOk)
    {
        var sample = result.Unwrap();
        if (sample != null)
        {
            using (sample)
            {
                await ProcessSampleAsync(sample.Payload);
            }
        }
    }
}
```

### 2. Listener - Async Event Waiting

The `Listener` class provides async methods for waiting for events from notifiers.

#### Wait with Timeout

```csharp
using var listener = service.ListenerBuilder()
    .Create()
    .Expect("Failed to create listener");

var cts = new CancellationTokenSource();

// Wait up to 10 seconds for an event
var result = await listener.WaitAsync(
    TimeSpan.FromSeconds(10), 
    cts.Token);

if (result.IsOk)
{
    var eventId = result.Unwrap();
    if (eventId != null)
    {
        Console.WriteLine($"Received event: {eventId.Value}");
    }
    else
    {
        Console.WriteLine("Timeout - no event received");
    }
}
```

#### Wait Indefinitely

```csharp
var cts = new CancellationTokenSource();

try
{
    // Wait indefinitely for an event
    var result = await listener.WaitAsync(cts.Token);
    
    if (result.IsOk)
    {
        var eventId = result.Unwrap();
        Console.WriteLine($"Event received: {eventId.Value}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Wait cancelled");
}
```

#### Event Loop with Async/Await

```csharp
var cts = new CancellationTokenSource();

while (!cts.Token.IsCancellationRequested)
{
    var result = await listener.WaitAsync(cts.Token);
    
    if (result.IsOk)
    {
        var eventId = result.Unwrap();
        await HandleEventAsync(eventId.Value);
    }
}
```

### 3. PendingResponse - Async Request/Response

The `PendingResponse<T>` class provides async methods for receiving responses in the request/response pattern.

#### Receive Response with Timeout

```csharp
var request = new CalculationRequest { A = 10, B = 20 };

var pendingResult = client.Send(request);
if (pendingResult.IsOk)
{
    using var pending = pendingResult.Unwrap();
    
    var cts = new CancellationTokenSource();
    
    // Wait up to 3 seconds for response
    var responseResult = await pending.ReceiveAsync(
        TimeSpan.FromSeconds(3), 
        cts.Token);
    
    if (responseResult.IsOk)
    {
        var response = responseResult.Unwrap();
        if (response != null)
        {
            using (response)
            {
                Console.WriteLine($"Response: {response.Payload.Result}");
            }
        }
        else
        {
            Console.WriteLine("Timeout - no response received");
        }
    }
}
```

#### Receive Response Indefinitely

```csharp
var pendingResult = client.Send(request);
if (pendingResult.IsOk)
{
    using var pending = pendingResult.Unwrap();
    
    var cts = new CancellationTokenSource();
    
    try
    {
        // Wait indefinitely for response
        var responseResult = await pending.ReceiveAsync(cts.Token);
        
        if (responseResult.IsOk)
        {
            using var response = responseResult.Unwrap();
            Console.WriteLine($"Response: {response.Payload.Result}");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Receive cancelled");
    }
}
```

### 4. WaitSet - Truly Event-Driven Async (Recommended)

⭐ **This is the truly async, event-driven approach** using platform-specific event mechanisms (epoll/kqueue).

The `WaitSet` allows you to wait on multiple event sources (Listeners, Subscribers with deadline/interval guards) with a **single blocking call** that wakes up only when events occur - no polling!

#### Basic WaitSet with Async

```csharp
using var waitset = WaitSetBuilder.New()
    .Create()
    .Expect("Failed to create WaitSet");

// Attach a listener (event source)
using var listenerGuard = waitset.AttachNotification(listener)
    .Expect("Failed to attach listener");

var cts = new CancellationTokenSource();

// Run WaitSet in background thread (truly blocks on OS primitives)
var waitTask = Task.Run(() =>
{
    while (!cts.Token.IsCancellationRequested)
    {
        var result = waitset.WaitAndProcess((attachmentId) =>
        {
            Console.WriteLine($"Event received on attachment {attachmentId}");
            
            // Check which source triggered
            if (listenerGuard.AttachmentId.HasEventFrom(attachmentId))
            {
                var eventResult = listener.TryWait();
                if (eventResult.IsOk && eventResult.Unwrap() != null)
                {
                    Console.WriteLine($"Event ID: {eventResult.Unwrap().Value}");
                }
            }
            
            return CallbackProgression.Continue;
        });
        
        if (!result.IsOk)
        {
            Console.WriteLine($"WaitSet error: {result.UnwrapErr()}");
            break;
        }
    }
}, cts.Token);

// Let it run
await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
cts.Cancel();
await waitTask;
```

#### WaitSet with Multiple Subscribers

```csharp
using var waitset = WaitSetBuilder.New()
    .Create()
    .Expect("Failed to create WaitSet");

// Attach multiple subscribers with deadline guards
// WaitSet wakes up when data arrives OR deadline is missed
using var guard1 = waitset.AttachDeadline(
    subscriber1, 
    TimeSpan.FromSeconds(1))
    .Expect("Failed to attach subscriber1");

using var guard2 = waitset.AttachDeadline(
    subscriber2, 
    TimeSpan.FromSeconds(2))
    .Expect("Failed to attach subscriber2");

var cts = new CancellationTokenSource();

var waitTask = Task.Run(() =>
{
    while (!cts.Token.IsCancellationRequested)
    {
        waitset.WaitAndProcess((attachmentId) =>
        {
            // Check which subscriber has data
            if (guard1.AttachmentId.HasEventFrom(attachmentId))
            {
                if (guard1.AttachmentId.HasMissedDeadline(attachmentId))
                {
                    Console.WriteLine("Subscriber1 deadline missed!");
                }
                else
                {
                    var result = subscriber1.Receive<SensorData>();
                    if (result.IsOk && result.Unwrap() != null)
                    {
                        using var sample = result.Unwrap();
                        Console.WriteLine($"Subscriber1: {sample.Payload}");
                    }
                }
            }
            else if (guard2.AttachmentId.HasEventFrom(attachmentId))
            {
                if (guard2.AttachmentId.HasMissedDeadline(attachmentId))
                {
                    Console.WriteLine("Subscriber2 deadline missed!");
                }
                else
                {
                    var result = subscriber2.Receive<SensorData>();
                    if (result.IsOk && result.Unwrap() != null)
                    {
                        using var sample = result.Unwrap();
                        Console.WriteLine($"Subscriber2: {sample.Payload}");
                    }
                }
            }
            
            return CallbackProgression.Continue;
        }).Expect("WaitSet failed");
    }
}, cts.Token);

await waitTask;
```

#### WaitSet with Interval (Periodic Wake-up)

```csharp
using var waitset = WaitSetBuilder.New()
    .Create()
    .Expect("Failed to create WaitSet");

// Wake up periodically even without events
using var intervalGuard = waitset.AttachInterval(TimeSpan.FromSeconds(5))
    .Expect("Failed to attach interval");

using var listenerGuard = waitset.AttachNotification(listener)
    .Expect("Failed to attach listener");

var cts = new CancellationTokenSource();

var waitTask = Task.Run(() =>
{
    while (!cts.Token.IsCancellationRequested)
    {
        waitset.WaitAndProcess((attachmentId) =>
        {
            if (intervalGuard.AttachmentId.HasEventFrom(attachmentId))
            {
                Console.WriteLine("Periodic wake-up - checking health...");
                // Perform periodic maintenance
            }
            else if (listenerGuard.AttachmentId.HasEventFrom(attachmentId))
            {
                Console.WriteLine("Event received!");
                var eventResult = listener.TryWait();
                // Process event...
            }
            
            return CallbackProgression.Continue;
        }).Expect("WaitSet failed");
    }
}, cts.Token);

await waitTask;
```

#### Why WaitSet is Truly Async

The `WaitSet.WaitAndProcess()` method:
- ✅ **Blocks on OS primitives** (epoll_wait, kevent, WaitForMultipleObjects)
- ✅ **No polling loop** - the OS kernel wakes the thread only when events occur
- ✅ **Low CPU usage** - thread sleeps until events
- ✅ **Low latency** - immediate wake-up on events
- ✅ **Multiplexing** - single wait for multiple sources

Compare this to `ReceiveAsync()`:
- ⚠️ Polls with `Task.Delay(10ms)` - minimum 10ms latency
- ⚠️ CPU usage from periodic wake-ups
- ⚠️ Only watches one source at a time

**Recommendation**: Use **WaitSet** for production event-driven code, use `ReceiveAsync()` for simple scripts or when you need .NET async/await style.

## Reactive Extensions - Declarative Async

For even more powerful async patterns, use the **Iceoryx2.Reactive** library which provides `IObservable<T>` and `IAsyncEnumerable<T>` support.

### Observable Streams

```csharp
using Iceoryx2.Reactive;
using System.Reactive.Linq;

var cts = new CancellationTokenSource();

// Convert to observable stream
using var subscription = subscriber.AsObservable<SensorData>(
    pollingInterval: TimeSpan.FromMilliseconds(10),
    cancellationToken: cts.Token)
    .Where(data => data.Temperature > 28.0)
    .Buffer(TimeSpan.FromSeconds(2))
    .Subscribe(buffer => 
    {
        var avgTemp = buffer.Average(d => d.Temperature);
        Console.WriteLine($"Avg temp: {avgTemp:F1}°C ({buffer.Count} samples)");
    });

// Let it run
await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
```

### Async Enumerable (await foreach)

```csharp
using Iceoryx2.Reactive;

var cts = new CancellationTokenSource();

await foreach (var data in subscriber.AsAsyncEnumerable<SensorData>(
    pollingInterval: TimeSpan.FromMilliseconds(10),
    cancellationToken: cts.Token))
{
    Console.WriteLine($"Received: {data}");
    
    // Process asynchronously
    await ProcessDataAsync(data);
    
    // Can use break to exit
    if (data.Temperature > 100)
        break;
}
```

## Advanced Patterns

### Task.WhenAny - Multiple Sources

```csharp
var cts = new CancellationTokenSource();

var sensorTask = subscriber.ReceiveAsync<SensorData>(cts.Token);
var eventTask = listener.WaitAsync(cts.Token);
var responseTask = pendingResponse.ReceiveAsync(cts.Token);

var completedTask = await Task.WhenAny(sensorTask, eventTask, responseTask);

if (completedTask == sensorTask)
{
    var result = await sensorTask;
    Console.WriteLine("Sensor data arrived first");
}
else if (completedTask == eventTask)
{
    var result = await eventTask;
    Console.WriteLine("Event arrived first");
}
else
{
    var result = await responseTask;
    Console.WriteLine("Response arrived first");
}
```

### Task.WhenAll - Parallel Operations

```csharp
var cts = new CancellationTokenSource();

var tasks = new[]
{
    subscriber1.ReceiveAsync<SensorData>(cts.Token),
    subscriber2.ReceiveAsync<SensorData>(cts.Token),
    subscriber3.ReceiveAsync<SensorData>(cts.Token)
};

var results = await Task.WhenAll(tasks);

foreach (var result in results)
{
    if (result.IsOk)
    {
        using var sample = result.Unwrap();
        Console.WriteLine($"Received: {sample.Payload}");
    }
}
```

### Timeout with CancellationTokenSource

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
    var result = await subscriber.ReceiveAsync<SensorData>(cts.Token);
    
    if (result.IsOk)
    {
        using var sample = result.Unwrap();
        Console.WriteLine($"Received within timeout: {sample.Payload}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out after 5 seconds");
}
```

### Graceful Shutdown

```csharp
private static CancellationTokenSource _cts = new();

static async Task Main(string[] args)
{
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        _cts.Cancel();
        Console.WriteLine("Graceful shutdown initiated...");
    };

    await RunApplicationAsync(_cts.Token);
}

static async Task RunApplicationAsync(CancellationToken cancellationToken)
{
    var subscriber = service.SubscriberBuilder<SensorData>()
        .Create()
        .Expect("Failed to create subscriber");

    try
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await subscriber.ReceiveAsync<SensorData>(
                TimeSpan.FromSeconds(1), 
                cancellationToken);
            
            if (result.IsOk && result.Unwrap() != null)
            {
                using var sample = result.Unwrap();
                await ProcessSampleAsync(sample.Payload);
            }
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Application stopped gracefully");
    }
    finally
    {
        subscriber.Dispose();
    }
}
```

## Performance Considerations

### Polling Interval Trade-offs

All async methods use polling under the hood (since the native C API doesn't have true async support). The polling interval affects:

- **Lower interval (1-10ms)**: Lower latency, higher CPU usage
- **Higher interval (50-100ms)**: Lower CPU usage, higher latency

Default is **10ms**, which is a good balance for most use cases.

```csharp
// Low latency, higher CPU
subscriber.AsObservable<SensorData>(pollingInterval: TimeSpan.FromMilliseconds(1))

// Lower CPU, higher latency
subscriber.AsObservable<SensorData>(pollingInterval: TimeSpan.FromMilliseconds(50))
```

### Thread Pool Utilization

All async methods use `Task.Delay()` with `ConfigureAwait(false)` to yield to the thread pool efficiently:

```csharp
// Good - doesn't block thread
await subscriber.ReceiveAsync<SensorData>(cancellationToken);

// Bad - blocks thread (don't do this)
var result = subscriber.ReceiveAsync<SensorData>(cancellationToken).GetAwaiter().GetResult();
```

## Summary

The iceoryx2 C# wrapper provides two approaches for async operations:

### Polling-Based Async (Simple, but not truly event-driven)

| Operation | Async Method | Timeout Support | Cancellation Token | Event-Driven |
|-----------|--------------|-----------------|-------------------|--------------|
| **Subscriber.Receive** | `ReceiveAsync<T>()` | ✅ Yes | ✅ Yes | ❌ Polling (10ms) |
| **Listener.Wait** | `WaitAsync()` | ✅ Yes | ✅ Yes | ⚠️ Delegates to native blocking |
| **PendingResponse.Receive** | `ReceiveAsync()` | ✅ Yes | ✅ Yes | ❌ Polling (10ms) |

### Event-Driven Async (Truly async, recommended for production)

| Operation | Method | Timeout Support | Cancellation Token | Event-Driven |
|-----------|--------|-----------------|-------------------|--------------|
| **WaitSet.WaitAndProcess** | Blocking | ❌ No | ⚠️ Manual (Task.Run) | ✅ epoll/kqueue |
| **WaitSet + Deadline** | Blocking | ✅ Yes (deadline) | ⚠️ Manual (Task.Run) | ✅ epoll/kqueue |
| **WaitSet + Interval** | Blocking | ✅ Yes (interval) | ⚠️ Manual (Task.Run) | ✅ epoll/kqueue |

### Reactive Extensions (Declarative polling)

| Operation | Method | Timeout Support | Cancellation Token | Event-Driven |
|-----------|--------|-----------------|-------------------|--------------|
| **Reactive Observable** | `AsObservable<T>()` | ⚠️ N/A | ✅ Yes | ❌ Polling (configurable) |
| **Reactive AsyncEnum** | `AsAsyncEnumerable<T>()` | ⚠️ N/A | ✅ Yes | ❌ Polling (configurable) |

### Recommendations

**For truly event-driven, low-latency, low-CPU applications:**
- ✅ Use **WaitSet** - blocks on OS primitives, wakes only on events
- ✅ Supports multiple event sources with single wait
- ✅ No polling overhead

**For simple scripts or when you need async/await style:**
- ⚠️ Use `ReceiveAsync()` / `WaitAsync()` - polling-based but non-blocking
- ⚠️ Minimum 10ms latency, periodic CPU wake-ups
- ⚠️ Good for simple scenarios, not ideal for production

**For declarative, composable streams:**
- ⚠️ Use **Iceoryx2.Reactive** - IObservable<T> with LINQ operators
- ⚠️ Polling-based (configurable interval)
- ⚠️ Great for data processing pipelines

**All methods**:
- ✅ Accept `CancellationToken`
- ✅ Yield to thread pool (don't block)
- ✅ Support timeout variants
- ✅ Work with `Task.WhenAny`, `Task.WhenAll`
- ✅ Compatible with async/await patterns
- ✅ Provide graceful shutdown support

This enables truly modern, non-blocking asynchronous applications with iceoryx2!
