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
            Tracer.Line("Start "+methodInfo.ReturnType.Name + " " + methodInfo.DeclaringType.FullName + "." + methodInfo.Name);
            Tracer.IndentStart();

            if(methodInfo.GetAttribute<ExplicitAttribute>(true) == null)
            {
                var test = Activator.CreateInstance(methodInfo.ReflectedType);
                var methods = methodInfo
                    .ReflectedType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var setups = methods
                    .Where(m => m.GetAttribute<SetUpAttribute>(false) != null)
                    .ToArray();
                foreach(var setup in setups)
                {
                    Tracer.Line("setup: " + setup.DeclaringType.FullName + "." + setup.Name);
                    setup.Invoke(test, new object[0]);
                }

                try
                {
                    methodInfo.Invoke(test, new object[0]);
                }
                catch(Exception e)
                {
                }
                ;
            }
            else
                Tracer.Line("Test not executed, ExplicitAttribute used");
            Tracer.IndentEnd();
            Tracer.Line("End " + methodInfo.ReturnType.Name + " " + methodInfo.DeclaringType.FullName + "." + methodInfo.Name);
        }
        public void End() { NotImplementedMethod();  }
    }

    internal class TestResult
    {
    }
}