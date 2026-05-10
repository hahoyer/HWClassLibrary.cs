// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Base attribute for replacing the data-portion (member listing) of the dump while keeping the
///     surrounding type/base-class framing produced by the dumper. Use <see cref="DumpClassAttribute" />
///     instead to replace the entire dump.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public abstract class DumpDataClassAttribute : Attribute
{
    /// <summary>
    ///     Produces the data-portion dump of <paramref name="target" />.
    /// </summary>
    /// <param name="type"> The declaring type whose members are being rendered. </param>
    /// <param name="target"> The instance to render. </param>
    public abstract string Dump(Type type, object target);
}