// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Replaces the dump representation of the decorated class or struct with the result of its
///     <see cref="object.ToString" /> override.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[PublicAPI]
public sealed class DumpToStringAttribute : DumpClassAttribute
{
    /// <inheritdoc />
    public override string Dump(Type type, object target) => target.ToString()!;
}