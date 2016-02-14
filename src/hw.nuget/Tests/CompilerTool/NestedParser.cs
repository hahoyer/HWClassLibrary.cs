using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;

namespace hw.Tests.CompilerTool
{
    [UnitTest]
    [TestFixture]
    [SimpleParser]
    public static class NestedParser
    {
        [UnitTest]
        public static void Simple()
        {
            var result = ParserUtil.Parse("--> b c");

            Tracer.Assert(result.TokenClassName == "c", result.Dump);
            Tracer.Assert(result.TokenClassIsMain, result.Dump);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClassName == "b");
            Tracer.Assert(!result.Left.TokenClassIsMain);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right == null);
        }

        [UnitTest]
        public static void MultipleEntries() => ParserUtil.ParseAndCheck
            ("--> (b c) c", "((((<null> b <null>) c <null>) () <null>) c <null>)");
    }
}