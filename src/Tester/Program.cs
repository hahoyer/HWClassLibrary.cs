using System.Diagnostics;
using System.Reflection;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Tester
{
    static class Program
    {
        static void Main(string[] args)
        {
            TestRunner.IsModeErrorFocus = Debugger.IsAttached;
            var result = TestRunner.RunTests(Assembly.GetExecutingAssembly());
            Tracer.Assert(result);
        }
    }
}