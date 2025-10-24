# iceoryx2-ffi-csharp

C# / .NET bindings for iceoryx2 - Zero-Copy Lock-Free IPC

## 🎯 Status

**✅ Production-Ready C# Bindings!**

- ✅ Cross-platform library loading (macOS tested, Linux/Windows ready)
- ✅ Complete P/Invoke FFI layer for all core APIs
- ✅ Memory-safe resource management with SafeHandle pattern
- ✅ High-level C# wrappers with builder pattern
- ✅ **Publish-Subscribe API** - Full implementation with type safety and zero-copy
- ✅ **Event API** - Complete notifier/listener implementation with blocking/timed waits
- ✅ **Request-Response API** - Complete client/server RPC with verified FFI signatures
- ✅ **Complex Data Types** - Full support for custom structs with sequential layout
- ✅ **Async/Await Support** - Modern async methods for all blocking operations with CancellationToken
- ✅ Tests passing on macOS
- ✅ Working examples for all major APIs (Pub/Sub, Event, RPC)
- ✅ Production-ready with proper memory management and error handling
- ⚠️ Requires native library: `libiceoryx2_ffi_c.{so|dylib|dll}`

📊 See [STATUS_REPORT.md](STATUS_REPORT.md) for detailed status.

## Overview

This package provides C# and .NET bindings for iceoryx2, enabling zero-copy inter-process communication in .NET applications. The bindings use P/Invoke to call into the iceoryx2 C FFI layer and provide idiomatic C# APIs with full memory safety.

### Key Features

- 🚀 **Zero-copy IPC** - Share memory between processes without serialization
- 🔒 **Type-safe** - Full C# type system support with compile-time checks  
- 🧹 **Memory-safe** - Automatic resource management via SafeHandle and IDisposable
- 🎯 **Idiomatic C#** - Builder pattern, Result types, LINQ-friendly APIs
- 🔧 **Cross-platform** - Works on Linux, macOS, and Windows
- 📦 **Multiple patterns** - Publish-Subscribe, Event, and Request-Response communication
- ⚡ **Async/Await** - Full async support with CancellationToken for modern C# applications

## Quick Start

### 1. Build the Native Library

```bash
# From repository root
cargo build --release --package iceoryx2-ffi-c
```

### 2. Build the C# Bindings

```bash
cd iceoryx2-ffi/csharp
dotnet build
```

### 3. Run the Publish-Subscribe Example

```bash
# Terminal 1 - Publisher
cd examples/PublishSubscribe
dotnet run -- publisher

# Terminal 2 - Subscriber  
cd examples/PublishSubscribe
dotnet run -- subscriber
```

You should see the subscriber receiving incrementing counter values from the publisher!

## Prerequisites

- **.NET 8.0 SDK or later** ([Download](https://dotnet.microsoft.com/download))
- **Rust toolchain** (for building the iceoryx2 C FFI library) - Install via [rustup](https://rustup.rs/)
- **C compiler** (gcc/clang on Linux/macOS, MSVC on Windows)
- **CMake** (optional, for C examples and tests)

## Build Instructions

### Prerequisites

- **.NET 8.0 SDK or later** ([Download](https://dotnet.microsoft.com/download))
- **Rust toolchain** (for building the C FFI library)
- **C compiler** (gcc/clang on Linux/macOS, MSVC on Windows)

### Platform-Specific Native Library Names

The C# bindings automatically detect and load the correct native library for your platform:

| Platform | Library Names (tried in order) |
|----------|--------------------------------|
| **Linux**   | `libiceoryx2_ffi_c.so`, `iceoryx2_ffi_c.so` |
| **macOS**   | `libiceoryx2_ffi_c.dylib`, `iceoryx2_ffi_c.dylib` |
| **Windows** | `iceoryx2_ffi_c.dll`, `libiceoryx2_ffi_c.dll` |

### Build Steps

### 1. Build the C FFI Library

First, build the iceoryx2 C FFI library:

```bash
cd ../../..  # Navigate to repository root
cargo build --release --package iceoryx2-ffi-c
```

### 2. Generate C# Bindings (Optional - pre-generated bindings are included)

The C# bindings are generated using ClangSharp. To regenerate them:

```bash
cd iceoryx2-ffi/csharp/generator
dotnet run
```

### 3. Build the C# Library

```bash
cd iceoryx2-ffi/csharp
dotnet build
```

### 4. Run Tests

```bash
dotnet test
```

### 5. Copy Native Library to Output

For **Linux**:
```bash
cp ../../../target/release/libiceoryx2_ffi_c.so bin/Release/net6.0/
```

For **macOS**:
```bash
cp ../../../target/release/libiceoryx2_ffi_c.dylib bin/Release/net6.0/
```

For **Windows**:
```powershell
copy ..\..\..\target\release\iceoryx2_ffi_c.dll bin\Release\net6.0\
```

### 6. Run Examples

**Publish-Subscribe Example:**

```bash
# Terminal 1 - Run publisher
cd examples/PublishSubscribe
dotnet run -- publisher

# Terminal 2 - Run subscriber
cd examples/PublishSubscribe
dotnet run -- subscriber
```

**Event Example:**

```bash
# Terminal 1 - Run notifier (event sender)
cd examples/Event
dotnet run -- notifier

# Terminal 2 - Run listener (event receiver)
cd examples/Event
dotnet run -- listener
```

**Request-Response Example:**

```bash
# Terminal 1 - Run server
cd examples/RequestResponse
dotnet run -- server

# Terminal 2 - Run client
cd examples/RequestResponse
dotnet run -- client
```

**Complex Data Types Example:**

```bash
# Terminal 1 - Run publisher
cd examples/ComplexDataTypes
dotnet run -- publisher TransmissionData

# Terminal 2 - Run subscriber
cd examples/ComplexDataTypes
dotnet run -- subscriber TransmissionData
```

## Project Structure

```
iceoryx2-ffi/csharp/
├── src/
│   └── Iceoryx2/
│       ├── Native/                      # C-bindings via P/Invoke
│       │   └── Iox2NativeMethods.cs    # Complete FFI declarations
│       ├── SafeHandles/                 # Memory-safe resource management
│       │   ├── SafeNodeHandle.cs       # Node resource management
│       │   ├── SafeServiceHandle.cs    # Service resource management
│       │   ├── SafePublisherHandle.cs  # Publisher resource management
│       │   ├── SafeSubscriberHandle.cs # Subscriber resource management
│       │   ├── SafeEventServiceHandle.cs # Event service management
│       │   ├── SafeNotifierHandle.cs   # Notifier resource management
│       │   └── SafeListenerHandle.cs   # Listener resource management
│       ├── Core/                        # High-level API wrappers
│       │   ├── Node.cs                 # Node wrapper
│       │   ├── NodeBuilder.cs          # Node builder pattern
│       │   ├── ServiceBuilder.cs       # Service builder pattern
│       │   └── ...                     # Other core classes
│       ├── PublishSubscribe/            # Pub/Sub messaging pattern
│       │   ├── Service.cs              # Service wrapper for pub/sub
│       │   ├── Publisher.cs            # Publisher wrapper
│       │   ├── Subscriber.cs           # Subscriber wrapper
│       │   ├── Sample.cs               # Data sample wrapper
│       │   └── ...                     # Related classes
│       ├── Event/                       # Event-based communication
│       │   ├── EventService.cs         # Event service wrapper
│       │   ├── Notifier.cs             # Event notifier (sender)
│       │   ├── Listener.cs             # Event listener (receiver)
│       │   ├── EventId.cs              # Event identifier type
│       │   └── EventServiceBuilder.cs  # Event service builder
│       ├── RequestResponse/             # Request-Response (RPC) pattern
│       │   ├── RequestResponseService.cs       # RPC service wrapper
│       │   ├── RequestResponseServiceBuilder.cs # RPC service builder
│       │   ├── Client.cs               # RPC client (request sender)
│       │   ├── Server.cs               # RPC server (request receiver)
│       │   ├── Request.cs              # Received request
│       │   ├── RequestMut.cs           # Mutable request to send
│       │   ├── Response.cs             # Received response
│       │   ├── ResponseMut.cs          # Mutable response to send
│       │   └── PendingResponse.cs      # Async response handle
│       ├── Types/                       # Common types and utilities
│       │   ├── Result.cs               # Result<T, E> monad
│       │   ├── Iox2Error.cs            # Error enumeration
│       │   └── ...                     # Other utility types
│       └── Iceoryx2.csproj             # Project file
├── examples/                            # C# examples
│   ├── PublishSubscribe/               # Pub/Sub example
│   ├── ComplexDataTypes/               # Complex struct example
│   ├── Event/                          # Event API example
│   └── RequestResponse/                # Request-Response RPC example
├── tests/                               # Unit tests
│   └── Iceoryx2Tests/
│       ├── BasicTests.cs               # Core functionality tests
│       └── ...                         # Additional test suites
└── README.md
```

## Usage Examples

### Publish-Subscribe Pattern

```csharp
using Iceoryx2;

// Create a node
var nodeResult = NodeBuilder.New()
    .Name("my_node")
    .Create();

if (!nodeResult.IsOk)
{
    Console.WriteLine($"Failed to create node: {nodeResult}");
    return;
}

using var node = nodeResult.Unwrap();

// Open or create a service for pub/sub
var serviceResult = node.ServiceBuilder()
    .PublishSubscribe<int>()
    .Open("MyService");

if (!serviceResult.IsOk)
{
    Console.WriteLine($"Failed to open service: {serviceResult}");
    return;
}

using var service = serviceResult.Unwrap();

// Publisher example
var publisherResult = service.CreatePublisher();
if (!publisherResult.IsOk)
{
    Console.WriteLine($"Failed to create publisher: {publisherResult}");
    return;
}

using var publisher = publisherResult.Unwrap();

var sampleResult = publisher.Loan();
if (!sampleResult.IsOk)
{
    Console.WriteLine($"Failed to loan sample: {sampleResult}");
    return;
}

using var sample = sampleResult.Unwrap();
sample.Payload = 42;

var sendResult = sample.Send();
if (!sendResult.IsOk)
{
    Console.WriteLine($"Failed to send: {sendResult}");
}

// Subscriber example
var subscriberResult = service.CreateSubscriber();
if (!subscriberResult.IsOk)
{
    Console.WriteLine($"Failed to create subscriber: {subscriberResult}");
    return;
}

using var subscriber = subscriberResult.Unwrap();

var receiveResult = subscriber.Receive();
if (!receiveResult.IsOk)
{
    Console.WriteLine($"Failed to receive: {receiveResult}");
    return;
}

var receivedSample = receiveResult.Unwrap();
if (receivedSample != null)
{
    Console.WriteLine($"Received: {receivedSample.Payload}");
}
```

### Event Pattern

```csharp
using Iceoryx2;
using Iceoryx2.Event;

// Create a node
var nodeResult = NodeBuilder.New()
    .Name("event_node")
    .Create();

if (!nodeResult.IsOk)
{
    Console.WriteLine($"Failed to create node: {nodeResult}");
    return;
}

using var node = nodeResult.Unwrap();

// Open or create an event service
var serviceResult = node.ServiceBuilder()
    .Event()
    .Open("MyEventService");

if (!serviceResult.IsOk)
{
    Console.WriteLine($"Failed to open event service: {serviceResult}");
    return;
}

using var service = serviceResult.Unwrap();

// Notifier example (event sender)
var notifierResult = service.CreateNotifier(defaultEventId: new EventId(100));
if (!notifierResult.IsOk)
{
    Console.WriteLine($"Failed to create notifier: {notifierResult}");
    return;
}

using var notifier = notifierResult.Unwrap();

var notifyResult = notifier.Notify(new EventId(5));
if (!notifyResult.IsOk)
{
    Console.WriteLine($"Failed to notify: {notifyResult}");
}

// Listener example (event receiver)
var listenerResult = service.CreateListener();
if (!listenerResult.IsOk)
{
    Console.WriteLine($"Failed to create listener: {listenerResult}");
    return;
}

using var listener = listenerResult.Unwrap();

// Non-blocking wait
var tryWaitResult = listener.TryWait();
if (!tryWaitResult.IsOk)
{
    Console.WriteLine($"Failed to wait: {tryWaitResult}");
    return;
}

var eventId = tryWaitResult.Unwrap();
if (eventId.HasValue)
{
    Console.WriteLine($"Received event: {eventId.Value}");
}

// Timed wait (1 second timeout)
var timedWaitResult = listener.TimedWait(TimeSpan.FromSeconds(1));
if (!timedWaitResult.IsOk)
{
    Console.WriteLine($"Failed to wait: {timedWaitResult}");
    return;
}

var timedEventId = timedWaitResult.Unwrap();
if (timedEventId.HasValue)
{
    Console.WriteLine($"Received event: {timedEventId.Value}");
}
else
{
    Console.WriteLine("Timeout - no event received");
}

// Blocking wait
var blockingWaitResult = listener.BlockingWait();
if (!blockingWaitResult.IsOk)
{
    Console.WriteLine($"Failed to wait: {blockingWaitResult}");
    return;
}

var blockingEventId = blockingWaitResult.Unwrap();
Console.WriteLine($"Received event: {blockingEventId}");
```

### Request-Response Pattern (RPC)

The Request-Response API provides a complete client-server RPC implementation with support for both convenience methods and zero-copy operations.

```csharp
using Iceoryx2;
using Iceoryx2.RequestResponse;
using System.Runtime.InteropServices;

// Define request and response types
[StructLayout(LayoutKind.Sequential)]
public struct AddRequest
{
    public int Value;
}

[StructLayout(LayoutKind.Sequential)]
public struct AddResponse
{
    public int Sum;
}

// Create a node
var nodeResult = NodeBuilder.New()
    .Name("rpc_node")
    .Create();

if (!nodeResult.IsOk)
{
    Console.WriteLine($"Failed to create node: {nodeResult}");
    return;
}

using var node = nodeResult.Unwrap();

// Open or create a request-response service
var serviceResult = node.ServiceBuilder()
    .RequestResponse<AddRequest, AddResponse>()
    .Open("AddService");

if (!serviceResult.IsOk)
{
    Console.WriteLine($"Failed to open service: {serviceResult}");
    return;
}

using var service = serviceResult.Unwrap();

// Client example - send request and wait for response
var clientResult = service.CreateClient();
if (!clientResult.IsOk)
{
    Console.WriteLine($"Failed to create client: {clientResult}");
    return;
}

using var client = clientResult.Unwrap();

// Option 1: SendCopy() - Convenience method that copies data
var pendingResult = client.SendCopy(new AddRequest { Value = 42 });
if (!pendingResult.IsOk)
{
    Console.WriteLine($"Failed to send request: {pendingResult}");
    return;
}

using var pending = pendingResult.Unwrap();

// Wait for response with timeout (non-blocking, timed, or blocking)
var responseResult = pending.TimedReceive(TimeSpan.FromSeconds(2));
if (!responseResult.IsOk)
{
    Console.WriteLine($"Failed to receive response: {responseResult}");
    return;
}

var response = responseResult.Unwrap();
if (response != null)
{
    using (response)
    {
        Console.WriteLine($"Response sum: {response.Payload.Sum}");
    }
}
else
{
    Console.WriteLine("Request timed out");
}

// Option 2: Loan() - Zero-copy method for better performance
var loanResult = client.Loan();
if (loanResult.IsOk)
{
    using var request = loanResult.Unwrap();
    request.Payload = new AddRequest { Value = 42 };
    
    var sendResult = request.Send();
    if (sendResult.IsOk)
    {
        using var pendingResponse = sendResult.Unwrap();
        // Handle response...
    }
}

// Server example - receive request and send response
var serverResult = service.CreateServer();
if (!serverResult.IsOk)
{
    Console.WriteLine($"Failed to create server: {serverResult}");
    return;
}

using var server = serverResult.Unwrap();

while (true)
{
    var requestResult = server.Receive();
    if (!requestResult.IsOk)
    {
        Console.WriteLine($"Failed to receive request: {requestResult}");
        break;
    }

    var request = requestResult.Unwrap();
    if (request != null)
    {
        using (request)
        {
            int value = request.Payload.Value;
            
            // Option 1: SendCopyResponse() - Convenience method
            var sendResult = request.SendCopyResponse(new AddResponse { Sum = value + 100 });
            if (!sendResult.IsOk)
            {
                Console.WriteLine($"Failed to send response: {sendResult}");
            }
            
            // Option 2: LoanResponse() - Zero-copy method
            // var loanResult = request.LoanResponse();
            // if (loanResult.IsOk)
            // {
            //     using var response = loanResult.Unwrap();
            //     response.Payload = new AddResponse { Sum = value + 100 };
            //     response.Send();
            // }
        }
    }
    
    Thread.Sleep(100); // Small delay between checks
}
```

**Key Features:**
- ✅ Fully verified FFI signatures matching the C API exactly
- ✅ Both convenience methods (`SendCopy`, `SendCopyResponse`) and zero-copy methods (`Loan`, `LoanResponse`)
- ✅ Three response waiting modes: non-blocking (`TryReceive`), timed (`TimedReceive`), and blocking (`BlockingReceive`)
- ✅ Proper memory management with automatic cleanup
- ✅ Type-safe request/response handling with generic types

### Complex Data Types

The bindings support complex data types using sequential layout:

```csharp
using System.Runtime.InteropServices;
using Iceoryx2;

[StructLayout(LayoutKind.Sequential)]
[Iox2Type("TransmissionData")]  // Optional: specify custom type name
public struct TransmissionData
{
    public int X;
    public int Y;
    public double Value;
}

// Use with publish-subscribe
var service = node.ServiceBuilder()
    .PublishSubscribe<TransmissionData>()
    .Open("ComplexDataService")
    .Unwrap();

using var publisher = service.CreatePublisher().Unwrap();
using var sample = publisher.Loan().Unwrap();

sample.Payload = new TransmissionData 
{ 
    X = 10, 
    Y = 20, 
    Value = 3.14 
};

sample.Send();
```

## Async/Await Support

The C# bindings provide full async/await support for all blocking operations, enabling modern asynchronous programming patterns with proper cancellation support.

### Benefits

- **Non-blocking** - Operations yield to the thread pool instead of blocking threads
- **Composable** - Use `Task.WhenAll()`, `Task.WhenAny()` for concurrent operations
- **Cancellable** - All async methods accept `CancellationToken` for cooperative cancellation
- **Efficient** - Better thread pool utilization compared to polling with `Thread.Sleep()`

### Async Methods

All classes with blocking operations provide async equivalents:

#### PendingResponse (Request-Response)

```csharp
// Synchronous methods (block the calling thread)
Result<Response<T>?, Iox2Error> TryReceive()
Result<Response<T>?, Iox2Error> TimedReceive(TimeSpan timeout)
Result<Response<T>, Iox2Error> BlockingReceive()

// Asynchronous methods (yield to thread pool)
Task<Result<Response<T>?, Iox2Error>> ReceiveAsync(TimeSpan timeout, CancellationToken ct = default)
Task<Result<Response<T>, Iox2Error>> ReceiveAsync(CancellationToken ct = default)
```

#### Listener (Events)

```csharp
// Synchronous methods (block the calling thread)
Result<EventId?, Iox2Error> TryWait()
Result<EventId?, Iox2Error> TimedWait(TimeSpan timeout)
Result<EventId, Iox2Error> BlockingWait()

// Asynchronous methods (offload to background thread)
Task<Result<EventId?, Iox2Error>> WaitAsync(TimeSpan timeout, CancellationToken ct = default)
Task<Result<EventId, Iox2Error>> WaitAsync(CancellationToken ct = default)
```

#### Subscriber (Publish-Subscribe)

```csharp
// Synchronous method (non-blocking poll)
Result<Sample<T>?, Iox2Error> Receive<T>()

// Asynchronous methods (poll with yielding to thread pool)
Task<Result<Sample<T>?, Iox2Error>> ReceiveAsync<T>(TimeSpan timeout, CancellationToken ct = default)
Task<Result<Sample<T>, Iox2Error>> ReceiveAsync<T>(CancellationToken ct = default)
```

**Note:** Subscriber async methods use polling (every 10ms) since the native API doesn't provide blocking receive. However, they yield to the thread pool efficiently.

### Example: Async Request-Response Client

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Iceoryx2;
using Iceoryx2.RequestResponse;

public async Task RunClientAsync(CancellationToken cancellationToken = default)
{
    // Create node and service (same as sync version)
    var node = NodeBuilder.New()
        .Name("async_client")
        .Create()
        .Unwrap();
    
    using var service = node.ServiceBuilder()
        .RequestResponse<ulong, MyResponse>()
        .Open("MyService")
        .Unwrap();
    
    using var client = service.CreateClient().Unwrap();
    
    // Send request
    var sendResult = client.SendCopy(42ul);
    using var pendingResponse = sendResult.Unwrap();
    
    // Wait for response asynchronously with timeout
    var responseResult = await pendingResponse.ReceiveAsync(
        TimeSpan.FromSeconds(2), 
        cancellationToken);
    
    if (responseResult.IsOk)
    {
        var response = responseResult.Unwrap();
        if (response != null)
        {
            using (response)
            {
                Console.WriteLine($"Received: {response.Payload}");
            }
        }
        else
        {
            Console.WriteLine("Request timed out");
        }
    }
}
```

### Example: Async Event Listener

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Iceoryx2;

public async Task RunListenerAsync(CancellationToken cancellationToken = default)
{
    var node = NodeBuilder.New()
        .Name("async_listener")
        .Create()
        .Unwrap();
    
    using var service = node.ServiceBuilder()
        .Event()
        .Open("MyEvents")
        .Unwrap();
    
    using var listener = service.CreateListener().Unwrap();
    
    // Wait for events asynchronously
    while (!cancellationToken.IsCancellationRequested)
    {
        var result = await listener.WaitAsync(
            TimeSpan.FromSeconds(5), 
            cancellationToken);
        
        if (result.IsOk)
        {
            var eventId = result.Unwrap();
            if (eventId.HasValue)
            {
                Console.WriteLine($"Received event: {eventId.Value}");
            }
            else
            {
                Console.WriteLine("Timeout - no event");
            }
        }
    }
}
```

### Best Practices

**1. Use async methods in async contexts:**
```csharp
// ✅ GOOD: Async all the way
public async Task ProcessDataAsync()
{
    var response = await pendingResponse.ReceiveAsync(TimeSpan.FromSeconds(1));
    // ... process response
}

// ❌ BAD: Blocking in async method
public async Task ProcessDataAsync()
{
    var response = pendingResponse.TimedReceive(TimeSpan.FromSeconds(1)); // Blocks!
}
```

**2. Always pass CancellationToken:**
```csharp
// ✅ GOOD: Cancellable operation
public async Task WorkAsync(CancellationToken ct)
{
    var response = await pendingResponse.ReceiveAsync(TimeSpan.FromSeconds(10), ct);
}

// ⚠️ OK but less flexible: No cancellation
public async Task WorkAsync()
{
    var response = await pendingResponse.ReceiveAsync(TimeSpan.FromSeconds(10));
}
```

**3. Use ConfigureAwait(false) in libraries:**
```csharp
// In library code, avoid capturing SynchronizationContext
var response = await pendingResponse
    .ReceiveAsync(timeout, ct)
    .ConfigureAwait(false);
```

**4. Combine with Task composition:**
```csharp
// Wait for multiple responses concurrently
var tasks = new[]
{
    pending1.ReceiveAsync(timeout, ct),
    pending2.ReceiveAsync(timeout, ct),
    pending3.ReceiveAsync(timeout, ct)
};

var responses = await Task.WhenAll(tasks);

// Or race for the first response
var firstResponse = await Task.WhenAny(tasks);
```

## Naming Convention

The C# bindings follow .NET naming conventions:

- **Classes** use PascalCase (e.g., `Node`, `ServiceBuilder`, `EventService`)
- **Methods** use PascalCase (e.g., `Create()`, `OpenOrCreate()`, `Notify()`)
- **Properties** use PascalCase (e.g., `Name`, `Id`, `Payload`)
- **Internal/Native types** use the original C naming with `iox2_` prefix
- **Result pattern** uses `IsOk` property and `Unwrap()` method for error handling

## API Patterns

### Result Type

All fallible operations return a `Result<T, Iox2Error>` type:

```csharp
var result = node.ServiceBuilder().Event().Open("MyService");

// Check for success
if (!result.IsOk)
{
    Console.WriteLine($"Error: {result}");
    return;
}

// Unwrap the value (only call after checking IsOk)
using var service = result.Unwrap();
```

### Builder Pattern

The bindings use a fluent builder pattern for configuration:

```csharp
var node = NodeBuilder.New()
    .Name("my_node")
    .Create()
    .Unwrap();

var service = node.ServiceBuilder()
    .PublishSubscribe<int>()
    .Open("MyService")
    .Unwrap();

var publisher = service.CreatePublisher()
    .Unwrap();
```

## Memory Management

The C# bindings implement proper memory management with multiple layers of safety:

- **All native resources implement `IDisposable`** - ensures cleanup even if exceptions occur
- **Use `using` statements** to ensure proper cleanup of resources
- **`SafeHandle` types** protect against resource leaks and race conditions
- **Automatic finalization** for cleanup if `Dispose()` is not called (though explicit disposal is recommended)
- **No manual memory management required** - the bindings handle all FFI marshalling

### Best Practices

```csharp
// ✅ GOOD: Using statement ensures disposal
using var node = NodeBuilder.New().Create().Unwrap();
using var service = node.ServiceBuilder().Event().Open("MyService").Unwrap();
using var notifier = service.CreateNotifier().Unwrap();

// ✅ GOOD: Explicit disposal in try-finally
var node = NodeBuilder.New().Create().Unwrap();
try 
{
    // Use node...
}
finally
{
    node.Dispose();
}

// ❌ BAD: No disposal - relies on finalizer (slower, not deterministic)
var node = NodeBuilder.New().Create().Unwrap();
// ... use node without disposing
```

## Features

### Supported Communication Patterns

- ✅ **Publish-Subscribe** - One-to-many data distribution with zero-copy
- ✅ **Event** - Lightweight notification system with custom event IDs
- ✅ **Request-Response** - Client-server RPC with async response handling
- 🚧 **Pipeline** - Coming soon

### Supported Platforms

- ✅ **macOS** (tested on Apple Silicon and Intel)
- ✅ **Linux** (x86_64, ARM64)
- ✅ **Windows** (x86_64)

### Type System

- ✅ **Primitive types** - int, uint, long, ulong, float, double, bool
- ✅ **Complex types** - Structs with `[StructLayout(LayoutKind.Sequential)]`
- ✅ **Custom type names** - Use `[Iox2Type("name")]` attribute
- ⚠️ **Zero-copy** - Requires sequential layout and unmanaged types

## Troubleshooting

### Native Library Not Found

If you get a `DllNotFoundException`, ensure:

1. The native library is built: `cargo build --release --package iceoryx2-ffi-c`
2. The library is in one of these locations:
   - Same directory as your executable
   - System library path (`/usr/lib`, `/usr/local/lib`, etc.)
   - Path specified in `LD_LIBRARY_PATH` (Linux), `DYLD_LIBRARY_PATH` (macOS), or `PATH` (Windows)

### Type Name Mismatches

If services can't connect, verify type names match:

```csharp
// Use Iox2Type attribute to ensure consistent naming
[Iox2Type("MyData")]
public struct MyData { ... }
```

For complex types, the bindings automatically generate length-prefixed names (e.g., `16TransmissionData` for a 16-character struct name). Primitive types use Rust naming (`i32`, `u64`, etc.).

### Memory Errors or Crashes

- Ensure all resources use `using` statements or are properly disposed
- Don't access samples after calling `Send()` or `Dispose()`
- Use `Result<T, E>` pattern - always check `IsOk` before calling `Unwrap()`

## Examples

The repository includes several complete examples:

### 1. PublishSubscribe
**Location:** `examples/PublishSubscribe/`

Demonstrates basic pub/sub pattern with primitive types:
- Publisher sends incrementing counter values
- Subscriber receives and displays values
- Shows proper resource management with `using` statements

### 2. Event
**Location:** `examples/Event/`

Demonstrates event-based communication:
- Notifier sends events with custom event IDs (0-11)
- Listener receives events with timeout support
- Shows three wait modes: non-blocking, timed, and blocking

### 3. ComplexDataTypes  
**Location:** `examples/ComplexDataTypes/`

Demonstrates zero-copy sharing of complex structs:
- Defines custom `TransmissionData` struct
- Shows struct layout and type naming
- Demonstrates cross-process struct sharing

### 4. RequestResponse
**Location:** `examples/RequestResponse/`

Demonstrates client-server RPC pattern with fully verified C API compatibility:
- Client sends `AddRequest` messages with integer values
- Server maintains a running sum and responds with `AddResponse`
- Shows async response handling with three wait modes (non-blocking, timed, blocking)
- Demonstrates both `SendCopy()` convenience method and `Loan()`/`LoanResponse()` for zero-copy
- FFI signatures verified to exactly match the C API for reliable operation

### 5. AsyncPubSub
**Location:** `examples/AsyncPubSub/`

Demonstrates modern async/await patterns for publish-subscribe:
- Async publisher using `await Task.Delay()` instead of blocking
- Async subscriber with timeout using `ReceiveAsync()`
- Async subscriber blocking until data arrives
- Multiple concurrent subscribers processing data in parallel
- Proper cancellation support with `CancellationToken`
- Shows best practices for async IPC in modern C# applications

**Run with:**
```bash
# Terminal 1 - Async publisher
cd examples/AsyncPubSub
dotnet run publisher

# Terminal 2 - Async subscriber with timeout
dotnet run subscriber

# Or try other modes: blocking, multi
dotnet run blocking
dotnet run multi
```

## Contributing

Contributions are welcome! Here are some areas where you can help:

- 🧪 **Testing** - Add more unit tests and integration tests
- 📚 **Documentation** - Improve XML docs and add tutorials
- 🎯 **Examples** - Create examples for specific use cases
- 🐛 **Bug fixes** - Report and fix issues
- ✨ **New features** - Implement missing APIs (request-response, pipeline, etc.)

### Development Workflow

1. **Fork and clone** the repository
2. **Build the native library**: `cargo build --release --package iceoryx2-ffi-c`
3. **Build the C# bindings**: `cd iceoryx2-ffi/csharp && dotnet build`
4. **Run tests**: `dotnet test`
5. **Make your changes** and ensure tests pass
6. **Submit a pull request** with a clear description

### Code Style

- Follow standard C# conventions (PascalCase for public APIs)
- Add XML documentation comments to all public APIs
- Use `Result<T, E>` for fallible operations
- Implement `IDisposable` for resources that wrap native handles
- Use `SafeHandle` for all P/Invoke handles

## Roadmap

- [x] Core infrastructure (Node, Service, Builder patterns)
- [x] Publish-Subscribe API with zero-copy support
- [x] Event API with blocking/timed/non-blocking waits
- [x] Request-Response API (RPC) with verified FFI compatibility
- [x] Complex data type support with sequential layout
- [x] Cross-platform library loading (macOS, Linux, Windows)
- [x] Comprehensive examples for all major APIs
- [x] Memory-safe resource management with SafeHandle pattern
- [x] Full async/await support with CancellationToken
- [ ] Pipeline API
- [ ] Service discovery and monitoring
- [ ] Performance benchmarks vs other IPC solutions
- [ ] NuGet package publication
- [ ] XML documentation improvements
- [ ] Additional integration tests

## License

Licensed under either of

- Apache License, Version 2.0 ([LICENSE-APACHE](../../LICENSE-APACHE) or <https://www.apache.org/licenses/LICENSE-2.0>)
- MIT license ([LICENSE-MIT](../../LICENSE-MIT) or <https://opensource.org/licenses/MIT>)

at your option.

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted for inclusion in the work by you, as defined in the Apache-2.0 license, shall be dual licensed as above, without any additional terms or conditions.
