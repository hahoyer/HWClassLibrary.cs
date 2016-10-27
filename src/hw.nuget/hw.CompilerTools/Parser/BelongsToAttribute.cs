using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace hw.Parser
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    sealed class BelongsToAttribute : Attribute
    {
        public System.Type TokenFactory { get; }

        public BelongsToAttribute(System.Type tokenFactory) { TokenFactory = tokenFactory; }
    }
}