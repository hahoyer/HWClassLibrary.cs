using System;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    [PublicAPI]
    public sealed class BelongsToAttribute : Attribute
    {
        public Type TokenFactory { get; }

        public BelongsToAttribute(Type tokenFactory) => TokenFactory = tokenFactory;
    }
}