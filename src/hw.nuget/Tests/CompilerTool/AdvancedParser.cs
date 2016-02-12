using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using hw.Tests.CompilerTool.Util;
using hw.UnitTest;
using NUnit.Framework;

namespace hw.Tests.CompilerTool
{
    [UnitTest]
    [TestFixture]
    [SimpleParser]
    public static class AdvancedParser
    {
        [UnitTest]
        public static void NestedParser()
        {
            var result = ParserUtil.Parse("--> b c");

            Tracer.Assert(result.TokenClassName == "c", result.Dump);
            Tracer.Assert(result.TokenClassIsMain, result.Dump);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClassName == "b");
            Tracer.Assert(!result.Left.TokenClassIsMain);
            Tracer.Assert(result.Left.Left == null);
            Tracer.Assert(result.Left.Right == null);
        }

        [UnitTest]
        public static void NestedParserMultipleEntries()
        {
            var result = ParserUtil.Parse("--> (b c) c");

            Tracer.Assert(result.TokenClassName == "c");
            Tracer.Assert(result.TokenClassIsMain, result.Dump);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClassName == "c", result.Dump);
            Tracer.Assert(!result.Left.TokenClassIsMain);
            Tracer.Assert(result.Left.Left != null, result.Dump);
            Tracer.Assert(result.Left.Right == null);
            Tracer.Assert(result.Left.Left.TokenClassName == "b", result.Dump);
            Tracer.Assert(!result.Left.Left.TokenClassIsMain);
            Tracer.Assert(result.Left.Left.Left == null);
            Tracer.Assert(result.Left.Left.Right == null);
        }

        [UnitTest]
        public static void Parenthesis()
        {
            var text = "(anton)";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClassName == "anton");
            Tracer.Assert(result.Left == null);
            Tracer.Assert(result.Right == null);
        }

        [UnitTest]
        public static void ParenthesisAndSuffix()
        {
            ParserUtil.ParseAndCheck("(anton)berta", "((<null> anton <null>) berta <null>)");
        }

        [UnitTest]
        public static void ParenthesisAndPrefix()
        {
            var text = "zulu(anton)";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClassName == "zulu", result.Dump);
            Tracer.Assert(result.Left == null, result.Dump);
            Tracer.Assert(result.Right != null, result.Dump);

            Tracer.Assert(result.Right.TokenClassName == "anton", result.Dump);
            Tracer.Assert(result.Right.Left == null, result.Dump);
            Tracer.Assert(result.Right.Right == null, result.Dump);
        }

        [UnitTest]
        public static void ParenthesisSuffixAndPrefix()
        {
            var text = "zulu(anton)berta";
            var source = new Source(text);
            var result = MainTokenFactory.Instance.Execute(source + 0, null);

            Tracer.Assert(result.TokenClassName == "berta", result.Dump);
            Tracer.Assert(result.Left != null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);

            Tracer.Assert(result.Left.TokenClassName == "zulu", result.Dump);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right != null, result.Dump);

            Tracer.Assert(result.Left.Right.TokenClassName == "anton", result.Dump);
            Tracer.Assert(result.Left.Right.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right.Right == null, result.Dump);
        }

        [UnitTest]
        public static void EmptyParenthesis()
        {
            var result = ParserUtil.Parse("()");

            Tracer.Assert(result.TokenClassName == "", result.Dump);
            Tracer.Assert(result.Left == null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);
        }

        [UnitTest]
        public static void EmptyParenthesisAndSuffix()
        {
            var result = ParserUtil.Parse("()berta");

            Tracer.Assert(result.TokenClassName == "berta", result.Dump);
            Tracer.Assert(result.Left != null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);

            Tracer.Assert(result.Left.TokenClassName == "", result.Dump);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right == null, result.Dump);
        }

        [UnitTest]
        public static void EmptyParenthesisAndPrefix()
        {
            var text = "zulu()";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClassName == "zulu", result.Dump);
            Tracer.Assert(result.Left == null, result.Dump);
            Tracer.Assert(result.Right != null, result.Dump);

            Tracer.Assert(result.Right.TokenClassName == "", result.Dump);
            Tracer.Assert(result.Right.Left == null, result.Dump);
            Tracer.Assert(result.Right.Right == null, result.Dump);
        }

        [UnitTest]
        public static void EmptyParenthesisSuffixAndPrefix()
        {
            var result = ParserUtil.Parse("zulu()berta");

            Tracer.Assert(result.TokenClassName == "berta", result.Dump);
            Tracer.Assert(result.Left != null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);

            Tracer.Assert(result.Left.TokenClassName == "zulu", result.Dump);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right != null, result.Dump);

            Tracer.Assert(result.Left.Right.TokenClassName == "", result.Dump);
            Tracer.Assert(result.Left.Right.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right.Right == null, result.Dump);
        }

        [UnitTest]
        public static void ParenthesisSuffixAndPrefixAndSequence()
        {
            ParserUtil.ParseAndCheck
                (
                    "zulu aaa()(anton)berta",
                    "((((<null> zulu <null>) aaa (<null>  <null>))  (<null> anton <null>)) berta <null>)"
                );
            ParserUtil.ParseAndCheck
                (
                    "zulu()(anton)berta",
                    "(((<null> zulu (<null>  <null>))  (<null> anton <null>)) berta <null>)"
                );
        }

        [UnitTest]
        public static void RealParenthesisTest()
        {
            var text = " (x5 type x5) instance () dump_print           ";
            var source = new Source(text);
            var result = MainTokenFactory.Instance.Execute(source + 0, null);

            Tracer.Assert
                (
                    result.Dump()
                        == "(((((<null> x5 <null>) type <null>) x5 <null>) instance (<null>  <null>)) dump_print <null>)",
                    result.Dump());
        }

        [UnitTest]
        public static void LotOfParenthesisTest()
        {
            ParserUtil.ParseAndCheck(" x()()", "((<null> x (<null>  <null>))  (<null>  <null>))");
        }
    }
}