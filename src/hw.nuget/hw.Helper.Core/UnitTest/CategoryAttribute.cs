using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace hw.UnitTest
{
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public sealed class TestAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public sealed class TestFixtureAttribute : Attribute
    {
        public readonly string DefaultMethod;
    }
}