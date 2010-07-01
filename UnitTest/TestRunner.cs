using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.UnitTest
{
    internal class TestRunner : Dumpable
    {
        private List<TestResult> _testResults= new List<TestResult>();

        public void Add(MethodInfo methodInfo)
        {
            if(methodInfo.GetAttribute<ExplicitAttribute>(true) != null)
                return;
            if(methodInfo.ReflectedType.IsAbstract)
                return;

            Tracer.Line(methodInfo.ReturnType.Name + " " + methodInfo.ReflectedType.FullName + "." + methodInfo.Name);
            Tracer.IndentStart();
            var test = Activator.CreateInstance(methodInfo.ReflectedType);
            var methods = methodInfo
                .ReflectedType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance|BindingFlags.DeclaredOnly);
            var setups = methods
                .Where( m=> m.GetAttribute<SetUpAttribute>(false)!= null)
                .ToArray();
            foreach(var setup in setups)
            {
                Tracer.Line("setup: "+ setup.ReflectedType.FullName + "." + setup.Name);
                setup.Invoke(test, new object[0]);
            }
            Tracer.IndentEnd();
        }
        public void End() { NotImplementedMethod();  }
    }

    internal class TestResult
    {
    }
}