using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.UnitTest;

[PublicAPI]
public abstract class DependenceProvider : Attribute
{
    internal TestType AsTestType
        (IEnumerable<TestType> testTypes) => testTypes.SingleOrDefault(target => target.Type == GetType());
}

[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class LowPriority : Attribute { }