using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace hw
{
    [TestFixture]
    public static class TestDump
    {
        class Foo
        {
            public int X = 20;
            public string S1 = "FooString";
        }

        class Bar : Foo
        {
            public int Y = 111;
            public new string S1 = "BarString";
        }

        class Bla : Bar
        {
            public int Z = 17;
            public new string S1 = "BlaString";
        }

        [Test]
        public static void M1()
        {
            const string expectedTrace = @"hw.TestDump+Bla
{
    Base:hw.TestDump+Bar
    {
        Base:hw.TestDump+Foo
        {
            X=20,
            S1=FooString
        }
        Y=111,
        S1=BarString
    }
    Z=17,
    S1=BlaString
}";

            var xxx = new Bla();
            var s = Tracer.Dump(xxx);
            Tracer.Assert(s == expectedTrace.Replace("\r", ""), () => "\n--Epected--\n" + expectedTrace + "|----\n--Found--\n" + s + "|----\n");
        }
    }
}