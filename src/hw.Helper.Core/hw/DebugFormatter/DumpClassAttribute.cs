// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Base attribute for replacing the standard dump representation of a class or struct.
///     Derive from this attribute and override <see cref="Dump" /> to control how instances are rendered.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public abstract class DumpClassAttribute : Attribute
{
    /// <summary>
    ///     Produces the dump representation for <paramref name="target" />.
    /// </summary>
    /// <param name="type"> The type currently being processed. May be a base type of <paramref name="target" />'s runtime type. </param>
    /// <param name="target"> The instance to render. </param>
    public abstract string Dump(Type type, object target);
}