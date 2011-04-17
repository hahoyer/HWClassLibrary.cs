using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace HWClassLibrary.UnitTest
{
    [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
    public sealed class TestAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    public sealed class TestFixtureAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ExplicitAttribute : Attribute
    {}
}