using System;
using JetBrains.Annotations;

namespace hw.Parser
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    [PublicAPI]
    sealed class BelongsToAttribute : Attribute
    {
        public Type TokenFactory { get; }

        public BelongsToAttribute(Type tokenFactory) => TokenFactory = tokenFactory;
    }
}