using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRunner.IsModeErrorFocus = Debugger.IsAttached;
            var result = TestRunner.RunTests(Assembly.GetExecutingAssembly());
            Tracer.Assert(result);
        }
    }
}