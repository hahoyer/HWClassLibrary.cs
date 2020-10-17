using System;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.DebugFormatter
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [MeansImplicitUse]
    [PublicAPI]
    public sealed class EnableDumpAttribute : DumpEnabledAttribute
    {
        public EnableDumpAttribute()
            : base(true) { }
    }
}