using hw.DebugFormatter;
using hw.UnitTest;
#pragma warning disable 414

namespace hw
{
    [UnitTest]
    public static class TestDump
    {
        class Foo
        {
            public string S1 = "FooString";
            public int X = 20;
        }

        class Bar : Foo { }

        class Bla : Bar
        {
            public new string S1 = "BlaString";
            public int Z = 17;
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
            S1=""FooString"",
            X=20
        }
    },
    S1=""BlaString"",
    Z=17
}";

            var xxx = new Bla();
            var s = Tracer.Dump(xxx);
            Tracer.Assert(s == expectedTrace.Replace("\r", "")
                , () => "\n--Expected--\n" + expectedTrace + "|----\n--Found--\n" + s + "|----\n");
        }
    }
}