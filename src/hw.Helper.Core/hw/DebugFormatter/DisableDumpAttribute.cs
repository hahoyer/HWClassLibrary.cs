// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Used to control dump of data element.
///     Inhibit dump of this member, even if it's not private
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DisableDumpAttribute : DumpEnabledAttribute
{
    public DisableDumpAttribute()
        : base(false) { }
}