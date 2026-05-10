// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Common base for attributes that toggle whether a member is included in the dump.
///     <see cref="EnableDumpAttribute" /> sets it to <c>true</c>, <see cref="DisableDumpAttribute" />
///     to <c>false</c>.
/// </summary>
public abstract class DumpEnabledAttribute : DumpAttributeBase
{
    /// <summary>Whether the decorated member participates in the dump.</summary>
    public bool IsEnabled { get; }

    /// <param name="isEnabled"> <c>true</c> to include the member, <c>false</c> to suppress it. </param>
    protected DumpEnabledAttribute(bool isEnabled) => IsEnabled = isEnabled;
}