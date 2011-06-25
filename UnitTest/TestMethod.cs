using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.UnitTest
{
    internal sealed class TestMethod : Dumpable
    {
        private readonly MethodInfo _methodInfo;
        public bool IsSuspended;
        public TestMethod(MethodInfo methodInfo) { _methodInfo = methodInfo; }

        public string ConfigurationString { get { return Name + ","; } }

        public string Name { get { return _methodInfo.Name; } }

        private void ShowException(Exception e)
        {
            Tracer.Line("*********************Exception: " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            Tracer.Line(e.GetType().FullName);
            Tracer.Line(e.Message);
            Tracer.Line("*********************End Exception: " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            throw new TestFailedException();
        }

        public void Run()
        {
            Tracer.Line("Start " + _methodInfo.ReturnType.Name + " " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            Tracer.IndentStart();
            try
            {
                if(!IsSuspended)
                {
                    var test = Activator.CreateInstance(_methodInfo.ReflectedType);
                    var isBreakDisabled = Tracer.IsBreakDisabled;
                    Tracer.IsBreakDisabled = !TestRunner.IsModeErrorFocus;
                    try
                    {
                        _methodInfo.Invoke(test, new object[0]);
                    }
                    catch(Exception e)
                    {
                        ShowException(e);
                    }
                    Tracer.IsBreakDisabled = isBreakDisabled;
                }
            }
            finally
            {
                Tracer.IndentEnd();
                Tracer.Line("End " + _methodInfo.ReturnType.Name + " " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            }
        }
    }
}