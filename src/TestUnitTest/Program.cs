using System.Reflection;
using hw.DebugFormatter;
using hw.UnitTest;

namespace TestUnitTest;

static class Program
{
    static void Main(string[] args)
    {
        (!TestRunner.IsUnitTestType(typeof(NotTaggedClass))).Assert();
        (!TestRunner.IsUnitTestType(typeof(AbstractClass))).Assert();
        TestRunner.IsUnitTestType(typeof(BareClass)).Assert();
        TestRunner.IsUnitTestType(typeof(SealedClass)).Assert();
        TestRunner.IsUnitTestType(typeof(StaticClass)).Assert();
        TestRunner.IsUnitTestType(typeof(InterfaceTest)).Assert();
        var method = TestRunner
            .GetUnitTestTypes(Assembly.GetAssembly(typeof(Program))!)
            .Single(t => t.Type == typeof(InterfaceTest))
            .UnitTestMethods
            .Single();

        (method.Name == "Run").Assert();

        "All tests for UnitTest passed.".Log();
    }
}