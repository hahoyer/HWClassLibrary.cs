using System.Diagnostics;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

namespace Tester
{
    [PublicAPI]
    static class Program
    {
        static void Main(string[] args)
        {
            var configuration = TestRunner.Configuration;

            configuration.IsBreakEnabled = Debugger.IsAttached;
            configuration.SaveResults = true;

            if(Debugger.IsAttached)
            {
                configuration.SkipSuccessfulMethods = true;
                configuration.SaveResults = false;
                PendingTests.Run();
            }

            configuration.TestsFileName = (SmbFile.SourceFolder! / "PendingTests.cs").FullName;
            var result = TestRunner.RunTests(Assembly.GetExecutingAssembly());
            result.Assert();
        }
    }
}