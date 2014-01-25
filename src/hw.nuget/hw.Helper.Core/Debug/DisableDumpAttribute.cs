using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Debug
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DisableDumpAttribute : DumpEnabledAttribute
    {
        public DisableDumpAttribute()
            : base(false) { }
    }
}