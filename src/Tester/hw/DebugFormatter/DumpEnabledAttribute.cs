// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

public abstract class DumpEnabledAttribute : DumpAttributeBase
{
    public bool IsEnabled { get; }
    protected DumpEnabledAttribute(bool isEnabled) => IsEnabled = isEnabled;
}