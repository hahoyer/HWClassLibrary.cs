using System;
using JetBrains.Annotations;

namespace HWClassLibrary.UnitTest
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CategoryAttribute : Attribute
    {
        public readonly string Name;
        public CategoryAttribute(string name) { Name = name; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class TestAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public class TestFixtureAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class SetUp : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ExplicitAttribute : Attribute
    {
    }
}