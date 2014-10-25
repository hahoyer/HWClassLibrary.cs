using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using hw.Tests.CompilerTool.Util;
using hw.UnitTest;

namespace hw.Tests.CompilerTool
{
    [TestFixture]
    public static class Parser
    {
        [Test]
        public static void NormalParser()
        {
            var text = "a b c";
            var source = new Source(text);


            var result = MainTokenFactory.Parser.Execute(source + 0, null);

            Tracer.Assert(result.TokenClass.Name == "c");
            Tracer.Assert(result.TokenClass.IsMain);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClass.Name == "b");
            Tracer.Assert(result.Left.TokenClass.IsMain);
            Tracer.Assert(result.Left.Left != null);
            Tracer.Assert(result.Left.Right == null);
            Tracer.Assert(result.Left.Left.TokenClass.Name == "a");
            Tracer.Assert(result.Left.Left.TokenClass.IsMain);
            Tracer.Assert(result.Left.Left.Left == null);
            Tracer.Assert(result.Left.Left.Right == null);
        }

        [Test]
        public static void NestedParser()
        {
            var text = "--> b c";
            var source = new Source(text);

            var result = MainTokenFactory.Parser.Execute(source + 0, null);
            Tracer.Assert(result.TokenClass.Name == "c", result.Dump);
            Tracer.Assert(result.TokenClass.IsMain, result.Dump);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClass.Name == "b");
            Tracer.Assert(!result.Left.TokenClass.IsMain);
            Tracer.Assert(result.Left.Left == null);
            Tracer.Assert(result.Left.Right == null);
        }
        [Test]
        public static void NestedParserMultipleEntries()
        {
            var text = "--> (b c) c";
            var source = new Source(text);

            var result = MainTokenFactory.Parser.Execute(source + 0, null);
            Tracer.Assert(result.TokenClass.Name == "c");
            Tracer.Assert(result.TokenClass.IsMain);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClass.Name == "b");
            Tracer.Assert(!result.Left.TokenClass.IsMain);
            Tracer.Assert(result.Left.Left == null);
            Tracer.Assert(result.Left.Right == null);
        }
    }
}