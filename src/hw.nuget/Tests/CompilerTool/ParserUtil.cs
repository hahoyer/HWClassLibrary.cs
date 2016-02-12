using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using hw.Tests.CompilerTool.Util;
using hw.UnitTest;

namespace hw.Tests.CompilerTool
{
    static class ParserUtil
    {
        internal static void ParseAndCheck(string text, string expectedResultDump, int stackFrameDepth = 0)
        {
            var result = Parse(text);
            Tracer.Assert
                (
                    result.Dump() == expectedResultDump,
                    () =>
                        "\nResult: " + result.Dump() +
                            "\nXpcted: " + expectedResultDump,
                    stackFrameDepth + 1);
        }

        internal static Syntax Parse(string text) { return Parse(text, TestRunner.IsModeErrorFocus); }

        internal static Syntax Parse(string text, bool trace)
        {
            var source = new Source(text);
            MainTokenFactory.Instance.Trace = trace;
            NestedTokenFactory.Instance.Trace = trace;
            var result = MainTokenFactory.Instance.Execute(source + 0);
            MainTokenFactory.Instance.Trace = false;
            NestedTokenFactory.Instance.Trace = false;
            return result;
        }
    }
}