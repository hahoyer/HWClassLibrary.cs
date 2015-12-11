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
    public static class Parser
    {
        [UnitTest]
        public static void WrongExpression()
        {
            ParseAndCheck
                (
                    "(a;b);c",
                    "(((<null> ?(? (<null> a <null>)) ; ((<null> b <null>) ?)? <null>)) ; (<null> c <null>))"
                );
        }

        [UnitTest]
        [Test]
        public static void InvalidComment()
        {
            ParseAndCheck
                (
                    "#( \na",
                    "(<null> ?EOFInComment <null>)"
                );
        }

        [UnitTest]
        [Test]
        public static void PreceededInvalidComment()
        {
            var result = Parse("    #( \na");
            Tracer.Assert(result.Token.PrecededWith.Any());
        }

        [UnitTest]
        [Test]
        public static void Expression()
        {
            var expr = "(a*b+c*d)*(e*f+g*h)";
            var subResult = Parse(expr).Dump();

            var result = Parse(expr + " + " + expr).Dump();
            var xresult = result
                .Replace(subResult, "<expr>")
                .Replace("(<null> a <null>)", "<a>")
                .Replace("(<null> b <null>)", "<b>")
                .Replace("(<null> c <null>)", "<c>")
                .Replace("(<null> d <null>)", "<d>")
                .Replace("(<null> e <null>)", "<e>")
                .Replace("(<null> f <null>)", "<f>")
                .Replace("(<null> g <null>)", "<g>")
                .Replace("(<null> h <null>)", "<h>")
                ;

            ParseAndCheck(expr + " + " + expr, "(" + subResult + " + " + subResult + ")");
        }

        [UnitTest]
        public static void NormalParser()
        {
            var text = "a b c";
            var source = new Source(text);


            var result = MainTokenFactory.Instance.Execute(source + 0, null);

            Tracer.Assert(result.TokenClassName == "c");
            Tracer.Assert(result.TokenClassIsMain);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClassName == "b");
            Tracer.Assert(result.Left.TokenClassIsMain);
            Tracer.Assert(result.Left.Left != null);
            Tracer.Assert(result.Left.Right == null);
            Tracer.Assert(result.Left.Left.TokenClassName == "a");
            Tracer.Assert(result.Left.Left.TokenClassIsMain);
            Tracer.Assert(result.Left.Left.Left == null);
            Tracer.Assert(result.Left.Left.Right == null);
        }

        [UnitTest]
        public static void NestedParser()
        {
            var result = Parse("--> b c");

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
            var result = Parse("--> (b c) c");

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
            ParseAndCheck("(anton)berta", "((<null> anton <null>) berta <null>)");
        }

        static void ParseAndCheck(string text, string expectedResultDump, int stackFrameDepth = 0)
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

        static Syntax Parse(string text) { return Parse(text, TestRunner.IsModeErrorFocus); }

        static Syntax Parse(string text, bool trace)
        {
            var source = new Source(text);
            MainTokenFactory.Instance.Trace = trace;
            NestedTokenFactory.Instance.Trace = trace;
            var result = MainTokenFactory.Instance.Execute(source + 0);
            MainTokenFactory.Instance.Trace = false;
            NestedTokenFactory.Instance.Trace = false;
            return result;
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
            var result = Parse("()");

            Tracer.Assert(result.TokenClassName == "", result.Dump);
            Tracer.Assert(result.Left == null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);
        }

        [UnitTest]
        public static void EmptyParenthesisAndSuffix()
        {
            var result = Parse("()berta");

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
            var result = Parse("zulu()berta");

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
            ParseAndCheck
                (
                    "zulu aaa()(anton)berta",
                    "((((<null> zulu <null>) aaa (<null>  <null>))  (<null> anton <null>)) berta <null>)"
                );
            ParseAndCheck
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
            ParseAndCheck(" x()()", "((<null> x (<null>  <null>))  (<null>  <null>))");
        }
    }
}