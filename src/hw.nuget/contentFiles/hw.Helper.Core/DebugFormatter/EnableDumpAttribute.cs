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
        public int Order = 0;

        public EnableDumpAttribute()
            : base(true) { }
    }
}