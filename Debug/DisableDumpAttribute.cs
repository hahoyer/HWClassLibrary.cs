using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Debug
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DisableDumpAttribute : DumpEnabledAttribute
    {
        public DisableDumpAttribute()
            : base(false) { }
    }
}