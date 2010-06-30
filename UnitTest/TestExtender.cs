using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Helper;

namespace HWClassLibrary.UnitTest
{
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
                .Where(methodInfo => ReflectionExtender.GetAttribute<TestAttribute>((MethodInfo) methodInfo, true) != null);
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