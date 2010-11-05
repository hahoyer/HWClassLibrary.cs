using System;
using JetBrains.Annotations;

namespace HWClassLibrary.UnitTest
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CategoryAttribute : Attribute
    {
        public readonly string Name;
        public CategoryAttribute(string name) { Name = name; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public sealed class TestAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public sealed class TestFixtureAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public abstract class SetUpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ExplicitAttribute : Attribute
    {
    }
}