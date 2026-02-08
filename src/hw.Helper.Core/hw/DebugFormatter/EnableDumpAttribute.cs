// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Used to control dump of data element.
///     Enable dump of this member, even if it's private
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
[PublicAPI]
public sealed class EnableDumpAttribute : DumpEnabledAttribute
{
    public double Order;

    public EnableDumpAttribute()
        : base(true) { }
}