using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
// ReSharper disable CheckNamespace

namespace hw.Tests.Helper.Core
{
    [UnitTest]
    public static class TypeNameExtender
    {
        [UnitTest]
        public static void SimpleTypes()
        {
            Tracer.Assert(typeof(int).PrettyName() == "int");
            Tracer.Assert(typeof(List<int>).PrettyName() == "List<int>", () => typeof(List<int>).PrettyName());
            Tracer.Assert(typeof(TestClass).PrettyName() == "Core.TypeNameExtender.TestClass", () => typeof(TestClass).PrettyName());
        }

        class TestClass{}
    }
}