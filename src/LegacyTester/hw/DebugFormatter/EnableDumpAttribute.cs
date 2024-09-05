using System;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
[PublicAPI]
public sealed class EnableDumpAttribute : DumpEnabledAttribute
{
    public double Order;

    public EnableDumpAttribute()
        : base(true) { }
}