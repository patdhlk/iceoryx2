# Next Steps for C# Bindings Development

This document outlines the recommended path forward for completing the C# bindings for iceoryx2.

## ✅ What's Done

- Project structure and build system
- Cross-platform library loading mechanism
- Basic P/Invoke declarations (~50 functions)
- Safe resource management (SafeHandle)
- High-level C# API wrappers
- Test framework
- Documentation
- **Tests passing on macOS!**

## 🎯 Immediate Next Steps (Critical)

### 1. Verify P/Invoke Signatures

**Problem**: Complex function calls cause test host crashes.

**Action**:
```bash
# Compare our bindings with actual C header
diff iceoryx2-ffi/csharp/src/Iceoryx2/Native/Iox2NativeMethods.cs \
     iceoryx2-ffi/c/include/iox2/iceoryx2.h
```

**Focus Areas**:
- Struct layouts and sizes
- Pointer parameters (in/out)
- Return types
- Enum values

**Test Approach**:
1. Start with simplest functions (e.g., version getters)
2. Add one function at a time
3. Validate each before moving to next

### 2. Implement Storage Allocation

**Current**: Using `IntPtr.Zero` placeholders
**Needed**: Proper memory allocation

**Example Fix**:
```csharp
// Before (WRONG):
var builderHandle = Iox2NativeMethods.iox2_node_builder_new(IntPtr.Zero);

// After (CORRECT):
// 1. Find storage size in C header or documentation
const int IOX2_NODE_BUILDER_STORAGE_SIZE = 256; // example size

// 2. Allocate memory
IntPtr storage = Marshal.AllocHGlobal(IOX2_NODE_BUILDER_STORAGE_SIZE);
try
{
    var builderHandle = Iox2NativeMethods.iox2_node_builder_new(storage);
    // ... use builder ...
}
finally
{
    Marshal.FreeHGlobal(storage);
}
```

**For Each Builder**:
- `iox2_node_builder` → needs storage
- `iox2_service_builder` → needs storage  
- `iox2_publisher_builder` → needs storage
- `iox2_subscriber_builder` → needs storage

### 3. Add String Marshaling

**Current**: `NotImplementedException` for string getters/setters

**Needed**: UTF-8 ↔ .NET string conversion

**Helper Functions to Add**:
```csharp
internal static class StringMarshaling
{
    // C string (null-terminated) → .NET string
    public static string FromUtf8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        
        // Find null terminator
        int length = 0;
        unsafe
        {
            byte* bytes = (byte*)ptr;
            while (bytes[length] != 0) length++;
        }
        
        // Convert UTF-8 to .NET string
        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);
        return System.Text.Encoding.UTF8.GetString(buffer);
    }
    
    // .NET string → C string (caller must free)
    public static IntPtr ToUtf8(string str)
    {
        if (str == null) return IntPtr.Zero;
        
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str + '\0');
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }
    
    public static void FreeUtf8(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
            Marshal.FreeHGlobal(ptr);
    }
}
```

**Usage Example**:
```csharp
public string Name
{
    get
    {
        IntPtr nameHandle;
        int result = Iox2NativeMethods.iox2_node_name(handle, out nameHandle);
        if (result != 0) return null;
        
        // Convert C string to .NET string
        return StringMarshaling.FromUtf8(nameHandle);
    }
}
```

### 4. Create End-to-End Test

**Goal**: Validate complete pub/sub workflow

**Test File**: `tests/IntegrationTests.cs`

```csharp
[Fact]
public void CanSendAndReceiveData()
{
    // 1. Create node
    using var node = Node.Builder()
        .Name("test_node")
        .Create()
        .Expect("Failed to create node");
    
    // 2. Open/create service
    using var service = node.ServiceBuilder("test_service")
        .PublishSubscribe<byte[]>()
        .MaxPayloadSize(1024)
        .OpenOrCreate()
        .Expect("Failed to create service");
    
    // 3. Create publisher
    using var publisher = service.PublisherBuilder()
        .Create()
        .Expect("Failed to create publisher");
    
    // 4. Create subscriber
    using var subscriber = service.SubscriberBuilder()
        .Create()
        .Expect("Failed to create subscriber");
    
    // 5. Send data
    byte[] testData = new byte[] { 1, 2, 3, 4, 5 };
    using (var sample = publisher.LoanSliceUninit((ulong)testData.Length)
        .Expect("Failed to loan sample"))
    {
        unsafe
        {
            fixed (byte* src = testData)
            {
                Buffer.MemoryCopy(src, (void*)sample.Payload, 
                    testData.Length, testData.Length);
            }
        }
        
        publisher.Send(sample).Expect("Failed to send");
    }
    
    // 6. Receive data
    using var received = subscriber.Receive().Expect("Failed to receive");
    Assert.NotNull(received);
    
    // 7. Verify data
    byte[] receivedData = new byte[testData.Length];
    unsafe
    {
        fixed (byte* dst = receivedData)
        {
            Buffer.MemoryCopy((void*)received.Payload, dst,
                testData.Length, testData.Length);
        }
    }
    
    Assert.Equal(testData, receivedData);
}
```

## 📈 Short Term Goals (Enable Real Usage)

### 5. Add Missing P/Invoke Declarations

**Reference**: `iceoryx2-ffi/c/include/iox2/iceoryx2.h`

**Categories to Add**:
- Version info functions (`iox2_version_*`)
- Configuration functions
- Statistics/monitoring functions
- Additional builder configuration methods

**Process**:
1. Find function in C header
2. Determine correct signature
3. Add to `Iox2NativeMethods.cs`
4. Add high-level wrapper
5. Write test
6. Update `IMPLEMENTATION_STATUS.md`

### 6. Implement Event Pattern

**Files to Create**:
- `src/Iceoryx2/Event.cs`
- `tests/EventTests.cs`

**APIs Needed**:
- `Notifier` - sends events
- `Listener` - receives events
- Event IDs and payloads

### 7. Implement Request-Response Pattern

**Files to Create**:
- `src/Iceoryx2/RequestResponse.cs`
- `tests/RequestResponseTests.cs`

**APIs Needed**:
- `Client` - sends requests, receives responses
- `Server` - receives requests, sends responses
- Request/response correlation

## 🚀 Medium Term Goals (Production Ready)

### 8. Cross-Platform Testing

**Linux Testing**:
```bash
# On Linux machine
cd iceoryx2-ffi/csharp
cargo build --release --package iceoryx2-ffi-c
cp ../../target/release/libiceoryx2_ffi_c.so tests/bin/Release/net6.0/
dotnet test -c Release
```

**Windows Testing**:
```powershell
# On Windows machine
cd iceoryx2-ffi\csharp
cargo build --release --package iceoryx2-ffi-c
copy ..\..\target\release\iceoryx2_ffi_c.dll tests\bin\Release\net6.0\
dotnet test -c Release
```

**Fix Platform Issues**:
- Path separators
- Library loading quirks
- Calling convention differences

### 9. Create NuGet Package

**File**: `Iceoryx2.nuspec`

```xml
<?xml version="1.0"?>
<package>
  <metadata>
    <id>Iceoryx2</id>
    <version>0.1.0</version>
    <authors>Eclipse iceoryx Contributors</authors>
    <description>C# bindings for iceoryx2 - Zero-Copy Lock-Free IPC</description>
    <repository type="git" url="https://github.com/eclipse-iceoryx/iceoryx2"/>
    <license type="expression">Apache-2.0 OR MIT</license>
    <tags>ipc zero-copy lock-free inter-process-communication</tags>
  </metadata>
  <files>
    <!-- Managed assembly -->
    <file src="bin/Release/net6.0/Iceoryx2.dll" target="lib/net6.0" />
    
    <!-- Native libraries -->
    <file src="runtimes/linux-x64/native/libiceoryx2_ffi_c.so" 
          target="runtimes/linux-x64/native" />
    <file src="runtimes/osx-x64/native/libiceoryx2_ffi_c.dylib" 
          target="runtimes/osx-x64/native" />
    <file src="runtimes/win-x64/native/iceoryx2_ffi_c.dll" 
          target="runtimes/win-x64/native" />
  </files>
</package>
```

**Build Package**:
```bash
nuget pack Iceoryx2.nuspec
```

### 10. Add CI/CD

**File**: `.github/workflows/csharp-bindings.yml`

```yaml
name: C# Bindings

on: [push, pull_request]

jobs:
  test:
    strategy:
      matrix:
        os: [ubuntu-latest, macos-latest, windows-latest]
    
    runs-on: ${{ matrix.os }}
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '6.0.x'
      
      - name: Setup Rust
        uses: actions-rs/toolchain@v1
        with:
          toolchain: stable
      
      - name: Build native library
        run: cargo build --release --package iceoryx2-ffi-c
      
      - name: Build C# bindings
        run: dotnet build -c Release
        working-directory: iceoryx2-ffi/csharp
      
      - name: Run tests
        run: dotnet test -c Release
        working-directory: iceoryx2-ffi/csharp
```

## 📚 Long Term Goals (Complete Feature Set)

### 11. Async/Await Support

Add Task-based async API for non-blocking operations:

```csharp
public async Task<Result<Sample<T>, Iox2Error>> ReceiveAsync(
    CancellationToken cancellationToken = default)
{
    return await Task.Run(() => Receive(), cancellationToken);
}
```

### 12. Blackboard Pattern

Implement publish-subscribe with persistence.

### 13. Performance Benchmarks

Create `benchmarks/` directory with:
- Latency measurements
- Throughput tests
- Memory usage analysis
- Comparison with other IPC methods

### 14. Advanced Examples

Create example applications:
- Multi-process data streaming
- Distributed sensor network
- Request-response services
- Event-driven architecture

### 15. API Documentation

Generate API docs with DocFX:
```bash
dotnet tool install -g docfx
docfx init
docfx build
```

## 🤝 Contributing Guide

### For New Contributors

**Easy First Issues**:
1. Add missing XML documentation comments (fix CS1591 warnings)
2. Add more P/Invoke declarations from C header
3. Write additional unit tests
4. Improve example applications

**Medium Difficulty**:
1. Implement string marshaling helpers
2. Fix storage allocation for builders
3. Add new high-level wrapper classes
4. Write integration tests

**Advanced**:
1. Optimize zero-copy performance
2. Implement async/await support
3. Create NuGet packaging
4. Set up CI/CD pipeline

### Development Workflow

1. **Pick a task** from this document
2. **Create a branch**: `git checkout -b feature/your-feature`
3. **Make changes**: Follow existing code style
4. **Write tests**: Ensure they pass
5. **Update docs**: IMPLEMENTATION_STATUS.md and this file
6. **Submit PR**: With clear description

### Code Style

- Use C# naming conventions (PascalCase for public, camelCase for private)
- Add XML documentation for all public APIs
- Use `unsafe` only when necessary
- Prefer SafeHandle over manual resource management
- Follow Result<T, E> pattern for error handling

## 📋 Progress Tracking

Update `IMPLEMENTATION_STATUS.md` after completing each task:
- Move items from ❌ to 🔄 when starting
- Move from 🔄 to ✅ when complete
- Add notes about challenges or decisions

## 🐛 Debugging Tips

### Test Host Crashes
- Verify P/Invoke signatures match C header exactly
- Check struct layouts with `Marshal.SizeOf<T>()`
- Use `DllImport(..., SetLastError=true)` and check `Marshal.GetLastWin32Error()`
- Enable native debugging: `<PropertyGroup><AllowUnsafeBlocks>true</AllowUnsafeBlocks><EnableNativeDebugging>true</EnableNativeDebugging></PropertyGroup>`

### Memory Leaks
- Run with memory profiler (dotMemory, PerfView)
- Verify all SafeHandles are disposed
- Check for circular references
- Use `using` statements consistently

### Cross-Platform Issues
- Test on actual target platforms, not just emulators
- Check library search paths on each OS
- Verify calling conventions (Cdecl vs StdCall)
- Watch for path separator differences (/ vs \)

## 📞 Getting Help

- **iceoryx2 Docs**: https://iceoryx.io/
- **C# Interop Guide**: https://learn.microsoft.com/en-us/dotnet/standard/native-interop/
- **ClangSharp Docs**: https://github.com/dotnet/ClangSharp
- **Issue Tracker**: GitHub Issues

---

**Ready to contribute?** Pick any task from this document and get started! 🚀
