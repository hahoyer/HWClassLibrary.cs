using System.Reflection;

// ReSharper disable CheckNamespace

namespace hw.UnitTest;

public static class TestExtender
{
    public static bool RunTests(this Assembly rootAssembly) => TestRunner.RunTests(rootAssembly);
}