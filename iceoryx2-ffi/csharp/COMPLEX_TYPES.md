# Complex Type Support Added to C# Bindings

## Summary

Added comprehensive support for complex (struct) types in the iceoryx2 C# bindings, enabling zero-copy communication of custom data structures across process boundaries.

## Files Added

### 1. Core Library Enhancement

**`src/Iceoryx2/Iox2TypeAttribute.cs`** - NEW
- Custom attribute for specifying cross-language type names
- Allows C# structs to map to Rust/C type names
- Example: `[Iox2Type("TransmissionData")]`

**`src/Iceoryx2/Service.cs`** - MODIFIED
- Enhanced `GetRustCompatibleTypeName<T>()` to check for `Iox2TypeAttribute`
- Supports custom type name mapping for cross-language interoperability
- Maintains backward compatibility with primitive types

### 2. Complex Data Types Example

**`examples/ComplexDataTypes/ComplexDataTypes.csproj`** - NEW
- Example project demonstrating struct usage
- Shows three different complexity levels

**`examples/ComplexDataTypes/Program.cs`** - NEW
- Three example struct types:
  - `TransmissionData`: Simple struct with int and double fields
  - `SensorData`: Realistic sensor data with timestamp
  - `Point3D`: Advanced example with fixed-size arrays
- Publisher and subscriber implementations for each type
- Command-line interface for easy testing

**`examples/ComplexDataTypes/README.md`** - NEW
- Comprehensive documentation
- Best practices for struct design
- Cross-language communication guidelines
- Troubleshooting guide

## Key Features

### Type Safety
- `unmanaged` constraint ensures zero-copy compatibility
- `[StructLayout(LayoutKind.Sequential)]` guarantees memory layout
- Compile-time verification of valid types

### Cross-Language Support
- `[Iox2Type]` attribute for explicit type name mapping
- Compatible with Rust `#[repr(C)]` structs
- Compatible with C structs

### Examples Provided

#### 1. TransmissionData (Simple)
```csharp
[StructLayout(LayoutKind.Sequential)]
[Iox2Type("TransmissionData")]
public struct TransmissionData
{
    public int X;
    public int Y;
    public double Funky;
}
```

#### 2. SensorData (Realistic)
```csharp
[StructLayout(LayoutKind.Sequential)]
[Iox2Type("SensorData")]
public struct SensorData
{
    public long Timestamp;
    public float Temperature;
    public float Humidity;
    public int SensorId;
}
```

#### 3. Point3D (Advanced with Fixed Arrays)
```csharp
[StructLayout(LayoutKind.Sequential)]
[Iox2Type("Point3D")]
public unsafe struct Point3D
{
    public fixed float Coordinates[3];
    public int Id;
}
```

## Usage Examples

### Publisher
```bash
cd examples/ComplexDataTypes
dotnet run -- publisher transmission
```

### Subscriber
```bash
cd examples/ComplexDataTypes
dotnet run -- subscriber transmission
```

### Cross-Language Communication
```bash
# Terminal 1: C# Publisher
dotnet run -- publisher transmission

# Terminal 2: Rust Subscriber (matching type name)
cargo run --example transmission_subscriber
```

## Implementation Details

### Memory Layout Requirements

1. **Sequential Layout**: All structs must use `[StructLayout(LayoutKind.Sequential)]`
2. **Unmanaged Types**: Only primitive types and other unmanaged structs allowed
3. **No Managed References**: No strings, classes, or managed pointers
4. **Fixed Alignment**: Struct size and alignment must match across languages

### Type Name Resolution

The type name resolution follows this priority:

1. Check for `[Iox2Type("CustomName")]` attribute
2. Check primitive type mappings (int → "i32", etc.)
3. Fall back to C# type name

### Alignment Handling

- Primitive types: alignment = size
- Custom structs with `[StructLayout(Pack = N)]`: alignment = N
- Default struct alignment: pointer size (8 bytes on 64-bit)

## Best Practices

1. **Always use `[StructLayout(LayoutKind.Sequential)]`** for interop structs
2. **Use `[Iox2Type]`** when communicating with Rust/C
3. **Order fields largest to smallest** to minimize padding
4. **Document struct size** in bytes for verification
5. **Test cross-language** before production use
6. **Avoid complex nesting** for performance
7. **Use fixed arrays sparingly** (requires `unsafe`)

## Testing

### Build
```bash
cd /path/to/iceoryx2/iceoryx2-ffi/csharp
dotnet build examples/ComplexDataTypes/ComplexDataTypes.csproj
```

### Run Tests
```bash
# Terminal 1
cd examples/ComplexDataTypes
dotnet run -- publisher transmission

# Terminal 2
cd examples/ComplexDataTypes
dotnet run -- subscriber transmission
```

Expected output:
- Publisher: "Sending: TransmissionData { x: 0, y: 0, funky: 0.00 }"
- Subscriber: "Received: TransmissionData { x: 0, y: 0, funky: 0.00 }"

## Compatibility

- **.NET Version**: 8.0+
- **Platform**: Linux, macOS, Windows (anywhere iceoryx2 runs)
- **Cross-Language**: Compatible with Rust and C implementations
- **Zero-Copy**: Full zero-copy semantics maintained

## Future Enhancements

Potential additions:
- [ ] Automatic C# struct generation from Rust/C definitions
- [ ] Runtime size validation
- [ ] Type versioning support
- [ ] Array/slice support (variable-length collections)
- [ ] Nested struct optimization

## Documentation

See `examples/ComplexDataTypes/README.md` for:
- Detailed usage instructions
- Cross-language communication guide
- Troubleshooting common issues
- Memory layout explanations
- Advanced examples
