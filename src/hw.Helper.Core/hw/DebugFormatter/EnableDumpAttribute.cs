// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Forces inclusion of the decorated field or property in the dump, even when default rules would
///     skip it (e.g. for private members or members whose name contains "Dump").
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
[PublicAPI]
public sealed class EnableDumpAttribute : DumpEnabledAttribute
{
    /// <summary>
    ///     Sort key used when several <see cref="EnableDumpAttribute" />-decorated members are listed
    ///     together. Lower values appear first; defaults to 0.
    /// </summary>
    public double Order;

    /// <summary>Initialises the attribute with <see cref="DumpEnabledAttribute.IsEnabled" /> set to <c>true</c>.</summary>
    public EnableDumpAttribute()
        : base(true) { }
}