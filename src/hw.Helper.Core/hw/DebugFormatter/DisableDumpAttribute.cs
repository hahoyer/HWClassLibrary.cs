// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Excludes the decorated field or property from the dump, overriding the default rule that public,
///     non-private members are included.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DisableDumpAttribute : DumpEnabledAttribute
{
    /// <summary>Initialises the attribute with <see cref="DumpEnabledAttribute.IsEnabled" /> set to <c>false</c>.</summary>
    public DisableDumpAttribute()
        : base(false) { }
}