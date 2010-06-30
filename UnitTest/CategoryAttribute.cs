using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using HWClassLibrary.Helper;

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



    public static class TestExtender
    {
        public static void RunTests(this Assembly rootAssembly)
        {
            var x = GetUnitTests(rootAssembly);
            foreach(var type in x)
                RunTests(type);
        }

        private static void RunTests(MethodInfo methodInfo)
        {
        }

        private static IEnumerable<MethodInfo> GetUnitTests(Assembly rootAssembly)
        {
            return GetAssemblies(rootAssembly)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetAttribute<TestFixtureAttribute>(true) != null)
                .SelectMany(type=>type.GetMethods())
                .Where(methodInfo => methodInfo.GetAttribute<TestAttribute>(true) != null);
        }

        private static IEnumerable<Assembly> GetAssemblies(Assembly rootAssembly)
        {
            var result = new[] {rootAssembly};
            for(var referencedAssemblies = result;
                referencedAssemblies.Length > 0;
                result = result.Union(referencedAssemblies).ToArray())
            {
                referencedAssemblies = referencedAssemblies
                    .SelectMany(assembly => assembly.GetReferencedAssemblies())
                    .Select(AssemblyLoad)
                    .Distinct()
                    .Where(assembly => !result.Contains(assembly))
                    .ToArray();
            }
            return result;
        }

        private static Assembly AssemblyLoad(AssemblyName yy)
        {
            try
            {
                return AppDomain.CurrentDomain.Load(yy);
            }
            catch(Exception e)
            {
                return Assembly.GetExecutingAssembly();
            }
        }
    }
}