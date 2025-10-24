# iceoryx2-ffi-csharp

C# / .NET bindings for iceoryx2 - Zero-Copy Lock-Free IPC

## 🎯 Status

**✅ Production-Ready C# Bindings!**

- ✅ Cross-platform library loading (macOS tested, Linux/Windows ready)
- ✅ Complete P/Invoke FFI layer for all core APIs
- ✅ Memory-safe resource management with SafeHandle pattern
- ✅ High-level C# wrappers with builder pattern
- ✅ **Publish-Subscribe API** - Full implementation with type safety
- ✅ **Event API** - Complete notifier/listener implementation
- ✅ Tests passing on macOS
- ✅ Working examples for all major APIs
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
- 📦 **Multiple patterns** - Publish-Subscribe and Event communication

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
│       ├── Types/                       # Common types and utilities
│       │   ├── Result.cs               # Result<T, E> monad
│       │   ├── Iox2Error.cs            # Error enumeration
│       │   └── ...                     # Other utility types
│       └── Iceoryx2.csproj             # Project file
├── examples/                            # C# examples
│   ├── PublishSubscribe/               # Pub/Sub example
│   ├── ComplexDataTypes/               # Complex struct example
│   └── Event/                          # Event API example
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
- 🚧 **Request-Response** - Coming soon
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
- [x] Publish-Subscribe API
- [x] Event API  
- [x] Complex data type support
- [x] Cross-platform library loading
- [ ] Request-Response API
- [ ] Pipeline API
- [ ] Service discovery and monitoring
- [ ] Performance benchmarks
- [ ] NuGet package publication

## License

Licensed under either of

- Apache License, Version 2.0 ([LICENSE-APACHE](../../LICENSE-APACHE) or <https://www.apache.org/licenses/LICENSE-2.0>)
- MIT license ([LICENSE-MIT](../../LICENSE-MIT) or <https://opensource.org/licenses/MIT>)

at your option.

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted for inclusion in the work by you, as defined in the Apache-2.0 license, shall be dual licensed as above, without any additional terms or conditions.
