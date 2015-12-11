using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;

namespace hw
{
    [UnitTest]
    public static class TestDump
    {
        class Foo
        {
            public int X = 20;
            public string S1 = "FooString";
        }

        class Bar : Foo
        {
        }

        class Bla : Bar
        {
            public int Z = 17;
            public new string S1 = "BlaString";
        }

        [UnitTest]
        public static void M1()
        {
            const string expectedTrace = @"hw.TestDump+Bla
{
    Base:hw.TestDump+Bar
    {
        Base:hw.TestDump+Foo
        {
            X=20,
            S1=""FooString""
        }
    },
    Z=17,
    S1=""BlaString""
}";

            var xxx = new Bla();
            var s = Tracer.Dump(xxx);
            Tracer.Assert(s == expectedTrace.Replace("\r", ""), () => "\n--Epected--\n" + expectedTrace + "|----\n--Found--\n" + s + "|----\n");
        }
    }
}