using System;

namespace hw.DebugFormatter
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DisableDumpAttribute : DumpEnabledAttribute
    {
        public DisableDumpAttribute()
            : base(false) { }
    }
}