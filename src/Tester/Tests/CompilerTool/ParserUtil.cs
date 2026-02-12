using hw.Scanner;
using Tester.Tests.CompilerTool.Util;

namespace Tester.Tests.CompilerTool
{
    static class ParserUtil
    {
        internal static void ParseAndCheck(string text, string expectedResultDump, int stackFrameDepth = 0)
        {
            var result = Parse(text);
            (result.Dump() == expectedResultDump).Assert
                (() =>
                        "\nResult: " + result.Dump() +
                            "\nXpcted: " + expectedResultDump,
                    stackFrameDepth + 1);
        }

        internal static Syntax Parse(string text) 
            => Parse(text, TestRunner.Configuration.IsBreakEnabled);

        [PublicAPI]
        internal static Syntax Parse(string text, bool trace)
        {
            var source = new Source(text);
            MainTokenFactory.Parser.Trace = trace;
            NestedTokenFactory.Parser.Trace = trace;
            var result = MainTokenFactory.Parser.Execute(source);
            MainTokenFactory.Parser.Trace = false;
            NestedTokenFactory.Parser.Trace = false;
            return result!;
        }
    }
}