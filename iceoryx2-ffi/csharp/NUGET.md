# Iceoryx2 NuGet Package

This document describes how to create, publish, and use the Iceoryx2 NuGet package.

## Creating the Package

### Prerequisites

1. Build the native iceoryx2 C library in release mode:
   ```bash
   cd /path/to/iceoryx2
   cargo build --release -p iceoryx2-ffi
   ```

2. This will create the native library at:
   - macOS: `target/release/libiceoryx2_ffi_c.dylib`
   - Linux: `target/release/libiceoryx2_ffi_c.so`
   - Windows: `target/release/iceoryx2_ffi_c.dll`

### Build the NuGet Package

From the `iceoryx2-ffi/csharp` directory:

```bash
dotnet pack src/Iceoryx2/Iceoryx2.csproj -c Release -o nupkgs
```

This will create: `nupkgs/Iceoryx2.0.7.0.nupkg`

## Publishing to NuGet.org

1. Get your API key from https://www.nuget.org/account/apikeys

2. Push the package:
   ```bash
   dotnet nuget push nupkgs/Iceoryx2.0.7.0.nupkg \
     --api-key YOUR_API_KEY \
     --source https://api.nuget.org/v3/index.json
   ```

## Using the Package

### Installing from NuGet.org

Once published, anyone can install it:

```bash
dotnet add package Iceoryx2
```

Or add to your `.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Iceoryx2" Version="0.7.0" />
</ItemGroup>
```

### Installing from Local Source

For testing before publishing:

```bash
# Add local package source
dotnet nuget add source /path/to/iceoryx2/iceoryx2-ffi/csharp/nupkgs --name iceoryx2-local

# Install the package
dotnet add package Iceoryx2 --version 0.7.0
```

## Package Contents

The NuGet package includes:

- **Managed Assemblies**: 
  - `lib/net8.0/Iceoryx2.dll` - The .NET 8.0 wrapper library
  - `lib/net9.0/Iceoryx2.dll` - The .NET 9.0 wrapper library
- **XML Documentation**: 
  - `lib/net8.0/Iceoryx2.xml` - IntelliSense documentation for .NET 8
  - `lib/net9.0/Iceoryx2.xml` - IntelliSense documentation for .NET 9
- **Native Libraries**:
  - `runtimes/osx-x64/native/libiceoryx2_ffi_c.dylib` - macOS x64
  - `runtimes/osx-arm64/native/libiceoryx2_ffi_c.dylib` - macOS ARM64
  - `runtimes/linux-x64/native/libiceoryx2_ffi_c.so` - Linux x64
  - `runtimes/win-x64/native/iceoryx2_ffi_c.dll` - Windows x64
- **README.md**: Package documentation

## Target Framework Support

The package supports multiple .NET versions:
- **.NET 8.0** (net8.0) - LTS release
- **.NET 9.0** (net9.0) - Latest release

When you reference the package, NuGet automatically selects the appropriate version for your project's target framework. Projects targeting .NET 8 will use `lib/net8.0/Iceoryx2.dll`, while .NET 9 projects will use `lib/net9.0/Iceoryx2.dll`.

## Native Library Loading

The native libraries are automatically deployed to the output directory of any project that references the Iceoryx2 NuGet package. The .NET runtime will automatically load the correct native library for the platform you're running on.

### Platform RID Support

The package uses Runtime Identifiers (RIDs) to deploy platform-specific native libraries:
- `osx-x64` - macOS on Intel
- `osx-arm64` - macOS on Apple Silicon
- `linux-x64` - Linux on x64
- `win-x64` - Windows on x64

## Version Management

To update the package version, edit `src/Iceoryx2/Iceoryx2.csproj`:

```xml
<PropertyGroup>
  <Version>0.7.0</Version>
</PropertyGroup>
```

Follow semantic versioning (MAJOR.MINOR.PATCH).

## Example Usage

After installing the package:

```csharp
using Iceoryx2;

var node = new NodeBuilder()
    .Create<IpcService>()
    .Expect("Failed to create node");

var service = node
    .ServiceBuilder("MyService")
    .PublishSubscribe<int>()
    .Open()
    .Expect("Failed to open service");

var publisher = service
    .PublisherBuilder()
    .Create()
    .Expect("Failed to create publisher");

var sample = publisher.Loan().Expect("Failed to loan sample");
sample.Payload = 42;
sample.Send().Expect("Failed to send");
```

## Troubleshooting

### Native Library Not Found

If you get a "DllNotFoundException", ensure:
1. The native library exists for your platform
2. You're running on a supported platform (macOS, Linux, or Windows)
3. The package was properly installed with native libraries

### Package Build Issues

If packaging fails:
1. Ensure the native library exists at `target/release/libiceoryx2_ffi_c.{dylib,so,dll}`
2. Build the native library with `cargo build --release -p iceoryx2-ffi`
3. Check the paths in `Iceoryx2.csproj` are correct

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Publish NuGet

on:
  release:
    types: [created]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build native library
        run: cargo build --release -p iceoryx2-ffi
      
      - name: Pack NuGet
        run: |
          cd iceoryx2-ffi/csharp
          ./pack.sh
      
      - name: Publish to NuGet
        run: |
          dotnet nuget push iceoryx2-ffi/csharp/nupkgs/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json
```

## License

The Iceoryx2 NuGet package is dual-licensed under Apache-2.0 OR MIT, matching the main iceoryx2 project.
