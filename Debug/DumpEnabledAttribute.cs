using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Debug
{
    public abstract class DumpEnabledAttribute : DumpAttributeBase
    {
        private readonly bool _isEnabled;

        protected DumpEnabledAttribute(bool isEnabled) { _isEnabled = isEnabled; }

        public bool IsEnabled { get { return _isEnabled; } }
    }
}