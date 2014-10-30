using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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


            var result = MainTokenFactory.Instance.Execute(source + 0, null);

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

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
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
            var result = Parse("--> (b c) c");

            Tracer.Assert(result.TokenClass.Name == "c");
            Tracer.Assert(result.TokenClass.IsMain, result.Dump);
            Tracer.Assert(result.Left != null);
            Tracer.Assert(result.Right == null);
            Tracer.Assert(result.Left.TokenClass.Name == "c", result.Dump);
            Tracer.Assert(!result.Left.TokenClass.IsMain);
            Tracer.Assert(result.Left.Left != null, result.Dump);
            Tracer.Assert(result.Left.Right == null);
            Tracer.Assert(result.Left.Left.TokenClass.Name == "b", result.Dump);
            Tracer.Assert(!result.Left.Left.TokenClass.IsMain);
            Tracer.Assert(result.Left.Left.Left == null);
            Tracer.Assert(result.Left.Left.Right == null);
        }

        [Test]
        public static void Parenthesis()
        {
            var text = "(anton)";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClass.Name == "anton");
            Tracer.Assert(result.Left == null);
            Tracer.Assert(result.Right == null);
        }
        [Test]
        public static void ParenthesisAndSuffix() { ParseAndCheck("(anton)berta", "((<null> anton <null>) berta <null>)"); }

        static void ParseAndCheck(string text, string expectedResultDump, int stackFrameDepth = 0)
        {
            var result = Parse(text);
            Tracer.Assert(result.Dump() == expectedResultDump, result.Dump, stackFrameDepth + 1);
        }

        static Syntax Parse(string text)
        {
            var source = new Source(text);
            MainTokenFactory.Instance.Trace = TestRunner.IsModeErrorFocus;
            NestedTokenFactory.Instance.Trace = TestRunner.IsModeErrorFocus;
            var result = MainTokenFactory.Instance.Execute(source + 0);
            MainTokenFactory.Instance.Trace = false;
            NestedTokenFactory.Instance.Trace = false;
            return result;
        }

        [Test]
        public static void ParenthesisAndPrefix()
        {
            var text = "zulu(anton)";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClass.Name == "zulu", result.Dump);
            Tracer.Assert(result.Left == null, result.Dump);
            Tracer.Assert(result.Right != null, result.Dump);

            Tracer.Assert(result.Right.TokenClass.Name == "anton", result.Dump);
            Tracer.Assert(result.Right.Left == null, result.Dump);
            Tracer.Assert(result.Right.Right == null, result.Dump);
        }

        [Test]
        public static void ParenthesisSuffixAndPrefix()
        {
            var text = "zulu(anton)berta";
            var source = new Source(text);
            var result = MainTokenFactory.Instance.Execute(source + 0, null);

            Tracer.Assert(result.TokenClass.Name == "berta", result.Dump);
            Tracer.Assert(result.Left != null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);

            Tracer.Assert(result.Left.TokenClass.Name == "zulu", result.Dump);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right != null, result.Dump);

            Tracer.Assert(result.Left.Right.TokenClass.Name == "anton", result.Dump);
            Tracer.Assert(result.Left.Right.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right.Right == null, result.Dump);
        }
        [Test]
        public static void EmptyParenthesis()
        {
            var text = "()";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClass.Name == "", result.Dump);
            Tracer.Assert(result.Left == null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);
        }

        [Test]
        public static void EmptyParenthesisAndSuffix()
        {
            var text = "()berta";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClass.Name == "berta", result.Dump);
            Tracer.Assert(result.Left != null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);

            Tracer.Assert(result.Left.TokenClass.Name == "", result.Dump);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right == null, result.Dump);
        }

        [Test]
        public static void EmptyParenthesisAndPrefix()
        {
            var text = "zulu()";
            var source = new Source(text);

            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            Tracer.Assert(result.TokenClass.Name == "zulu", result.Dump);
            Tracer.Assert(result.Left == null, result.Dump);
            Tracer.Assert(result.Right != null, result.Dump);

            Tracer.Assert(result.Right.TokenClass.Name == "", result.Dump);
            Tracer.Assert(result.Right.Left == null, result.Dump);
            Tracer.Assert(result.Right.Right == null, result.Dump);
        }

        [Test]
        public static void EmptyParenthesisSuffixAndPrefix()
        {
            var text = "zulu()berta";
            var source = new Source(text);
            var result = MainTokenFactory.Instance.Execute(source + 0, null);

            Tracer.Assert(result.TokenClass.Name == "berta", result.Dump);
            Tracer.Assert(result.Left != null, result.Dump);
            Tracer.Assert(result.Right == null, result.Dump);

            Tracer.Assert(result.Left.TokenClass.Name == "zulu", result.Dump);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right != null, result.Dump);

            Tracer.Assert(result.Left.Right.TokenClass.Name == "", result.Dump);
            Tracer.Assert(result.Left.Right.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right.Right == null, result.Dump);
        }

        [Test]
        public static void ParenthesisSuffixAndPrefixAndSequence()
        {
            var text = "zulu()(anton)berta";
            var source = new Source(text);
            var result = MainTokenFactory.Instance.Execute(source + 0, null);

            Tracer.Assert
                (result.Dump() == "(((<null> zulu (<null>  <null>))  (<null> anton <null>)) berta <null>)", result.Dump());
        }

        [Test]
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
        [Test]
        public static void LotOfParenthesisTest()
        {
            var text = " x()()";
            var source = new Source(text);
            MainTokenFactory.Instance.Trace = TestRunner.IsModeErrorFocus;
            var result = MainTokenFactory.Instance.Execute(source + 0, null);
            MainTokenFactory.Instance.Trace = false;

            Tracer.Assert
                (result.Dump() == "((<null> x (<null>  <null>))  (<null>  <null>))", result.Dump());
        }
    }
}