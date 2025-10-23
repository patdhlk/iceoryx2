# Cross-Platform Support

## Platform Status

| Platform | Status | Native Library | Notes |
|----------|--------|----------------|-------|
| **Linux (x86_64)** | ✅ Supported | `libiceoryx2_ffi_c.so` | Primary development platform |
| **macOS (Intel/ARM)** | ✅ Supported | `libiceoryx2_ffi_c.dylib` | Tested on macOS 12+ |
| **Windows** | ✅ Supported | `iceoryx2_ffi_c.dll` | Requires MSVC build tools |

## Library Loading

The C# bindings use `NativeLibrary.SetDllImportResolver` to automatically detect and load the correct native library for your platform. This happens transparently at runtime.

### How It Works

1. **Static Constructor**: When `Iox2NativeMethods` is first accessed, a static constructor registers a custom DLL import resolver
2. **Platform Detection**: The resolver uses `RuntimeInformation.IsOSPlatform()` to detect the current OS
3. **Library Search**: It tries multiple library name variants for the detected platform
4. **Fallback**: If the primary name fails, it tries alternative names

### Search Order

**Linux**:
1. `libiceoryx2_ffi_c.so`
2. `iceoryx2_ffi_c.so`

**macOS**:
1. `libiceoryx2_ffi_c.dylib`
2. `iceoryx2_ffi_c.dylib`

**Windows**:
1. `iceoryx2_ffi_c.dll`
2. `libiceoryx2_ffi_c.dll`

## Building Native Libraries

### Linux

```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Build the C FFI library
cd /path/to/iceoryx2
cargo build --release --package iceoryx2-ffi-c

# Library will be at: target/release/libiceoryx2_ffi_c.so
```

### macOS

```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Build the C FFI library
cd /path/to/iceoryx2
cargo build --release --package iceoryx2-ffi-c

# Library will be at: target/release/libiceoryx2_ffi_c.dylib
```

### Windows

```powershell
# Install Rust from https://rustup.rs/

# Install Visual Studio Build Tools
# Download from: https://visualstudio.microsoft.com/downloads/

# Build the C FFI library
cd C:\path\to\iceoryx2
cargo build --release --package iceoryx2-ffi-c

# Library will be at: target\release\iceoryx2_ffi_c.dll
```

## Deployment

### Option 1: Side-by-Side Deployment

Place the native library next to your application executable:

```
MyApp/
├── MyApp.exe (or MyApp.dll)
├── Iceoryx2.dll
└── libiceoryx2_ffi_c.{so|dylib|dll}
```

### Option 2: System-Wide Installation

**Linux**:
```bash
sudo cp libiceoryx2_ffi_c.so /usr/local/lib/
sudo ldconfig
```

**macOS**:
```bash
sudo cp libiceoryx2_ffi_c.dylib /usr/local/lib/
```

**Windows**:
```powershell
# Copy to a directory in PATH, e.g.:
copy iceoryx2_ffi_c.dll C:\Windows\System32\
```

### Option 3: NuGet Package with Runtime Assets

For NuGet distribution, include platform-specific libraries:

```
runtimes/
├── linux-x64/native/libiceoryx2_ffi_c.so
├── osx-x64/native/libiceoryx2_ffi_c.dylib
├── osx-arm64/native/libiceoryx2_ffi_c.dylib
└── win-x64/native/iceoryx2_ffi_c.dll
```

## Troubleshooting

### Library Not Found

**Symptom**: `DllNotFoundException` or similar error

**Solutions**:
1. Ensure the native library is in the same directory as your application
2. Check that the library name matches your platform (see table above)
3. Verify library is built for correct architecture (x64 vs x86)
4. On Linux, run `ldd libiceoryx2_ffi_c.so` to check dependencies
5. On macOS, run `otool -L libiceoryx2_ffi_c.dylib` to check dependencies

### Architecture Mismatch

**Symptom**: `BadImageFormatException` on Windows

**Solution**: Ensure both your .NET application and the native library are the same architecture (both x64 or both x86)

### Missing Dependencies

**Linux**: Install required system libraries:
```bash
# Ubuntu/Debian
sudo apt-get install build-essential

# Fedora/RHEL
sudo dnf install gcc
```

**macOS**: Install Xcode Command Line Tools:
```bash
xcode-select --install
```

**Windows**: Install Visual Studio Build Tools with C++ workload

## Testing Cross-Platform

### Using Docker

```bash
# Linux
docker run -it --rm -v $(pwd):/workspace mcr.microsoft.com/dotnet/sdk:6.0 bash
cd /workspace/iceoryx2-ffi/csharp
dotnet test

# Test with Alpine (musl libc)
docker run -it --rm -v $(pwd):/workspace mcr.microsoft.com/dotnet/sdk:6.0-alpine bash
```

### Using CI/CD

The project includes GitHub Actions workflows that test on:
- Ubuntu (latest)
- macOS (Intel and ARM)
- Windows (latest)

## Performance Considerations

### Library Loading Overhead

- The custom resolver adds minimal overhead (< 1ms) at first P/Invoke call
- Once loaded, there's no performance difference from standard P/Invoke
- Consider using AOT compilation (Native AOT) for fastest startup

### Platform-Specific Optimizations

- **Linux**: Use `perf` to profile IPC performance
- **macOS**: Use Instruments for profiling
- **Windows**: Use PerfView or Visual Studio Profiler

## Known Platform Differences

### Shared Memory

- **Linux**: Uses POSIX shared memory (`/dev/shm`)
- **macOS**: Uses POSIX shared memory with different limits
- **Windows**: Uses Win32 shared memory objects

### File Paths

- **Linux/macOS**: Uses `/` path separator
- **Windows**: Uses `\` path separator (handled automatically)

### Process Management

- **Linux**: Uses signals (SIGTERM, SIGKILL)
- **macOS**: Similar to Linux with some BSD differences
- **Windows**: Uses Windows process APIs

## Future Enhancements

- [ ] ARM32 support (Raspberry Pi)
- [ ] Android support via Xamarin
- [ ] iOS support via Xamarin
- [ ] RISC-V support
- [ ] Native AOT compilation support
