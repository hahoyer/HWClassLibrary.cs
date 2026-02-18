namespace Tester.Tests.Core;

[UnitTest]
public static class ExceptionDump
{
    [UnitTest]
    public static void DumpException()
    {
        try
        {
            RunEx1();
        }
        catch(Exception e)
        {
            var xxx = "\n"+e.LogDump();
            xxx.Log(FilePositionTag.Debug);
        }
    }

    static void RunEx1()
    {
        try
        {
            RunEx2();
        }
        catch(Exception e)
        {
            throw new Exception("RunEx1", e);
        }

    }
    static void RunEx2()
    {
        try
        {
            RunEx3();
        }
        catch(Exception e)
        {
            throw new Exception("RunEx2", e);
        }

    }
    static void RunEx3() => throw new Exception("RunEx3");
}