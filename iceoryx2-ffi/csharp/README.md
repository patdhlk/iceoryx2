# iceoryx2-ffi-csharp

C# / .NET bindings for iceoryx2 - Zero-Copy Lock-Free IPC

## 🎯 Status

**✅ Basic Integration Working!**

```
Test Run Successful.
Total tests: 6
     Passed: 4    ✅
    Skipped: 2    ⏭️
 Total time: 0,3640 Seconds
```

- ✅ Cross-platform library loading (macOS tested, Linux/Windows ready)
- ✅ P/Invoke bindings for core APIs  
- ✅ Memory-safe resource management with SafeHandle
- ✅ Tests passing on macOS
- ⚠️ API coverage: ~50% (basic pub/sub pattern)
- ⚠️ Requires native library: `libiceoryx2_ffi_c.{so|dylib|dll}`

📊 See [STATUS_REPORT.md](STATUS_REPORT.md) for detailed status.

## Overview

This package provides C# and .NET bindings for iceoryx2, enabling zero-copy inter-process communication in .NET applications. The bindings are generated using ClangSharp from the C FFI layer.

## Prerequisites

- .NET 8.0 or later
- iceoryx2 C library (built from `iceoryx2-ffi-c`)

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

```bash
cd examples/PublishSubscribe
dotnet run
```

## Project Structure

```
iceoryx2-ffi/csharp/
├── src/
│   └── Iceoryx2/
│       ├── Native/          # C-bindings via P/Invoke
│       ├── Node.cs          # High-level C# wrapper for Node
│       ├── Service.cs       # High-level C# wrapper for Service
│       ├── Publisher.cs     # High-level C# wrapper for Publisher
│       ├── Subscriber.cs    # High-level C# wrapper for Subscriber
│       └── ...              # Other wrapper classes
├── generator/               # ClangSharp binding generator project
├── examples/                # C# examples
├── tests/                   # Unit tests
└── README.md
```

## Usage Example

```csharp
using Iceoryx2;

// Create a node
using var node = NodeBuilder.New()
    .Name("my_node")
    .Create()
    .Expect("Failed to create node");

// Open or create a service
using var service = node.ServiceBuilder()
    .PublishSubscribe<int>()
    .Open("MyService")
    .Expect("Failed to open service");

// Create a publisher
using var publisher = service.Publisher()
    .Create()
    .Expect("Failed to create publisher");

// Send data
var sample = publisher.Loan().Expect("Failed to loan sample");
sample.Payload = 42;
sample.Send().Expect("Failed to send sample");
```

## Naming Convention

The C# bindings follow .NET naming conventions:

- Classes use PascalCase (e.g., `Node`, `ServiceBuilder`)
- Methods use PascalCase (e.g., `Create()`, `OpenOrCreate()`)
- Properties use PascalCase (e.g., `Name`, `Id`)
- Internal/Native types use the original C naming with `iox2_` prefix

## Memory Management

The C# bindings implement proper memory management:

- All native resources implement `IDisposable`
- Use `using` statements to ensure proper cleanup
- `SafeHandle` types protect against resource leaks
- Automatic finalization for cleanup if `Dispose()` is not called

## License

Licensed under either of

- Apache License, Version 2.0 ([LICENSE-APACHE](../../LICENSE-APACHE) or <https://www.apache.org/licenses/LICENSE-2.0>)
- MIT license ([LICENSE-MIT](../../LICENSE-MIT) or <https://opensource.org/licenses/MIT>)

at your option.
