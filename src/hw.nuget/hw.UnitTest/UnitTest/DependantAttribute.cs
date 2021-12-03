using System;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.UnitTest
{
    [PublicAPI]
    [Obsolete("Use DependenceProvider", true)]
    // ReSharper disable once IdentifierTypo
    public abstract class DependantAttribute : DependenceProvider 
    {
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LowPriority : Attribute { }
}