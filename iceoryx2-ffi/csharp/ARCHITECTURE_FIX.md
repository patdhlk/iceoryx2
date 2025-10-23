# Architecture Fix: Moving from Manual to Generated Bindings

**Date**: October 23, 2024  
**Issue**: P/Invoke signatures were manually edited in an auto-generated file  
**Resolution**: ✅ Moved all changes into the generator

---

## ❌ The Problem

Initially, P/Invoke signatures were corrected by manually editing `src/Iceoryx2/Native/Iox2NativeMethods.cs`, which is marked as "auto-generated". This violated the architectural principle:

> **Never manually edit auto-generated files!**

## ✅ The Solution

All manual changes were moved into the generator template in `generator/Program.cs`. Now the correct workflow is:

1. **Edit the generator**: `generator/Program.cs` → `GenerateManualBindings()` method
2. **Regenerate bindings**: `cd generator && dotnet run`
3. **Verify**: `dotnet test -c Release`

## 📝 What Was Moved Into the Generator

### 1. Struct Definitions with Proper Layout

```csharp
/// Storage for iox2_node_builder_t (18696 bytes, 8-byte aligned)
[StructLayout(LayoutKind.Sequential, Size = 18696, Pack = 8)]
internal struct iox2_node_builder_storage_t
{
    // Size attribute handles the uint8_t internal[18696] array
}

/// The actual node builder struct passed to C
[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct iox2_node_builder_t
{
    public iox2_node_builder_storage_t value;
    public IntPtr deleter; // void (*deleter)(struct iox2_node_builder_t*)
}
```

### 2. Corrected P/Invoke Signatures

**Before** (incorrect):
```csharp
[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
internal static extern IntPtr iox2_node_builder_new(IntPtr storage);
```

**After** (correct):
```csharp
/// <summary>
/// Creates a new node builder.
/// C signature: iox2_node_builder_h iox2_node_builder_new(struct iox2_node_builder_t *node_builder_struct_ptr)
/// Returns: handle to the builder (pointer to opaque type)
/// </summary>
[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
internal static extern IntPtr iox2_node_builder_new(ref iox2_node_builder_t node_builder_struct);
```

### 3. Added InternalsVisibleTo Attribute

```csharp
[assembly: InternalsVisibleTo("Iceoryx2.Tests")]
```

This allows test code to access internal P/Invoke methods for verification.

## 🔍 Verification

After moving all changes to the generator and regenerating:

```bash
cd iceoryx2-ffi/csharp/generator
dotnet run

# Output:
# iceoryx2 C# Binding Generator
# ==============================
# 
# Repository Root: /Users/patdhlk/src/patdhlk/iceoryx2
# Header File: .../target/.../iceoryx2.h
# Output Path: .../src/Iceoryx2/Native
# 
# Generating C# bindings...
#   ✓ Generated Iox2NativeMethods.cs
# 
# ✓ C# bindings generated successfully!
```

Running tests confirms everything still works:

```bash
cd iceoryx2-ffi/csharp
dotnet test -c Release

# Test summary: total: 9, failed: 0, succeeded: 7, skipped: 2
# ✅ Build succeeded with 16 warning(s)
```

## 📚 Benefits of This Approach

1. **Maintainability**: Changes are in one place (the generator)
2. **Reproducibility**: Anyone can regenerate the bindings from scratch
3. **Version Control**: Git history shows changes to the generator, not generated code
4. **Documentation**: The generator serves as documentation of our P/Invoke mapping strategy
5. **Consistency**: All bindings follow the same pattern

## 🎯 Key Takeaway

> When working with generated code, always update the generator, never the generated file.

This ensures the project remains maintainable and the build process remains reproducible.

---

## Future: Full ClangSharp Integration

The current generator uses a manual template. In the future, it could be enhanced to:

1. Parse the C header with ClangSharp's LibClang bindings
2. Automatically detect struct sizes and layouts
3. Generate proper P/Invoke signatures for all functions
4. Auto-generate XML documentation from C comments

For now, the manual template approach works well and is properly maintainable.
