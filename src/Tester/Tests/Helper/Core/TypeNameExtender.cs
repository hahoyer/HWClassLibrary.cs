using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

// ReSharper disable CheckNamespace
// ReSharper disable ClassCanBeSealed.Local

namespace hw.Tests.Helper.Core;

[UnitTest]
public static class TypeNameExtender
{
    class TestClass { }

    [UnitTest]
    public static void SimpleTypes()
    {
        (typeof(int).PrettyName() == "int").Assert();
        (typeof(List<int>).PrettyName() == "List<int>").Assert(() => typeof(List<int>).PrettyName());
        (typeof(TestClass).PrettyName() == "Core.TypeNameExtender.TestClass").Assert(()
            => typeof(TestClass).PrettyName());
    }
}