
namespace Tester.Tests.Core;

[UnitTest]
public static class EnableExceptTest
{
    sealed class None : DumpableObject
    {
        public int Zwei;
        int Eins;

        public None(int eins, int zwei)
        {
            Eins = eins;
            Zwei = zwei;
            Dump().Log();
        }
    }

    sealed class Both : DumpableObject
    {
        [EnableDumpExcept(3)]
        public int Zwei;

        [EnableDumpExcept(3)]
        int Eins;

        public Both(int eins, int zwei)
        {
            Eins = eins;
            Zwei = zwei;
            Dump().Log();
        }
    }

    sealed class Public : DumpableObject
    {
        [EnableDumpExcept(3)]
        public int Zwei;

        int Eins;

        public Public(int eins, int zwei)
        {
            Eins = eins;
            Zwei = zwei;
            Dump().Log();
        }
    }

    sealed class Private : DumpableObject
    {
        public int Zwei;

        [EnableDumpExcept(3)]
        int Eins;

        public Private(int eins, int zwei)
        {
            Eins = eins;
            Zwei = zwei;
            Dump().Log();
        }
    }

    [UnitTest]
    public static void Run()
    {
        Check(new None(1, 1), "EnableExceptTest.None.1i{Zwei=1}");
        Check(new None(3, 3), "EnableExceptTest.None.2i{Zwei=3}");
        Check(new Both(1, 1), "EnableExceptTest.Both.3i{Zwei=1}");
        Check(new Both(3, 3), "EnableExceptTest.Both.4i{}");
        Check(new Public(1, 1), "EnableExceptTest.Public.5i{Zwei=1}");
        Check(new Public(3, 3), "EnableExceptTest.Public.6i{}");
        Check(new Private(1, 1), "EnableExceptTest.Private.7i{Zwei=1}");
        Check(new Private(3, 3), "EnableExceptTest.Private.8i{Zwei=3}");
    }

    static void Check(DumpableObject target, string expected)
        => (target.Dump() == expected)
            .Assert(()=>$"\nexpct: {expected.Quote()}\nfound: {target.Dump().Quote()}",stackFrameDepth:1);
}