using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.UnitTest
{
    public static class TestExtender
    {
        public static void RunTests(this Assembly rootAssembly)
        {
            var tests = GetUnitTests(rootAssembly);
            var testRunner = new TestRunner();
            foreach(var methodInfo in tests)
                testRunner.Add(methodInfo);
            testRunner.End();
        }

        private static void RunTests(MethodInfo methodInfo)
        {
            Tracer.Dump(methodInfo);
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
            for(IEnumerable<Assembly> referencedAssemblies = result;
                referencedAssemblies.GetEnumerator().MoveNext();
                result = result.Union(referencedAssemblies).ToArray())
                referencedAssemblies = referencedAssemblies
                    .SelectMany(assembly => assembly.GetReferencedAssemblies())
                    .Select(AssemblyLoad)
                    .Distinct()
                    .Where(assembly => !result.Contains(assembly));
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