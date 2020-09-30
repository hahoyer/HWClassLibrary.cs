using System.Reflection;

namespace hw.UnitTest
{
    public static class TestExtender
    {
        public static bool RunTests(this Assembly rootAssembly) => TestRunner.RunTests(rootAssembly);
    }
}