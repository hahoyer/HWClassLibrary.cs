using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;

namespace HWClassLibrary.Debug
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class EnableDumpAttribute : DumpEnabledAttribute
    {
        public EnableDumpAttribute()
            : base(true) { }
    }
}