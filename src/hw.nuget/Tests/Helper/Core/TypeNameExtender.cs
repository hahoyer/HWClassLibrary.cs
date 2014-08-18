using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;

namespace hw.Tests.Helper.Core
{
    [TestFixture]
    public static class TypeNameExtender
    {
        [Test]
        public static void SimpleTypes()
        {
            Tracer.Assert(typeof(int).PrettyName() == "int");
            Tracer.Assert(typeof(List<int>).PrettyName() == "List<int>", () => typeof(List<int>).PrettyName());
            Tracer.Assert(typeof(TestClass).PrettyName() == "Core.TypeNameExtender.TestClass", () => typeof(TestClass).PrettyName());
        }

        class TestClass{}
    }
}