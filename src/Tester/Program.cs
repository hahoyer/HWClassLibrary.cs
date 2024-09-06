using System.Diagnostics;
using System.Reflection;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Tester;

[PublicAPI]
static class Program
{
    static void Main(string[] args)
    {
        TestRunner.Configuration.IsBreakEnabled = Debugger.IsAttached;
        var result = TestRunner.RunTests(Assembly.GetExecutingAssembly());
        result.Assert();
    }
}