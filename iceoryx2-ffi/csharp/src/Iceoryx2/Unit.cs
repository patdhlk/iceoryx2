namespace Iceoryx2;

/// <summary>
/// Represents a unit type (similar to Rust's () or void but as a value).
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}