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
    public static class AdvancedParser
    {
        [UnitTest]
        public static void TwoParenthesis()
        {
            ParserUtil.ParseAndCheck
                (" x()()", "((<null> x (<null> () <null>)) <nameless> (<null> () <null>))");
        }

        [UnitTest]
        public static void LotOfParenthesis()
        {
            ParserUtil.ParseAndCheck
                (
                    " x()()()()",
                    "((((<null> x (<null> () <null>)) <nameless> (<null> () <null>)) <nameless> (<null> () <null>)) <nameless> (<null> () <null>))");
        }

        [UnitTest]
        public static void Parenthesis()
        {
            ParserUtil.ParseAndCheck("(anton)", "((<null> anton <null>) () <null>)");
        }

        [UnitTest]
        public static void ParenthesisAndSuffix()
        {
            ParserUtil.ParseAndCheck
                ("(anton)berta", "(((<null> anton <null>) () <null>) berta <null>)");
        }

        [UnitTest]
        public static void ParenthesisAndPrefix()
        {
            ParserUtil.ParseAndCheck
                ("zulu(anton)", "(<null> zulu ((<null> anton <null>) () <null>))");
        }

        [UnitTest]
        public static void ParenthesisSuffixAndPrefix()
        {
            ParserUtil.ParseAndCheck
                (
                    "zulu(anton)berta",
                    "((<null> zulu ((<null> anton <null>) () <null>)) berta <null>)");
        }

        [UnitTest]
        public static void EmptyParenthesis()
        {
            var result = ParserUtil.Parse("()");

            Tracer.Assert(result.TokenClassName == "()", result.Dump);
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

            Tracer.Assert(result.Left.TokenClassName == "()", result.Dump);
            Tracer.Assert(result.Left.Left == null, result.Dump);
            Tracer.Assert(result.Left.Right == null, result.Dump);
        }

        [UnitTest]
        public static void EmptyParenthesisAndPrefix()
        {
            ParserUtil.ParseAndCheck("zulu()", "(<null> zulu (<null> () <null>))");
        }

        [UnitTest]
        public static void EmptyParenthesisSuffixAndPrefix()
        {
            ParserUtil.ParseAndCheck
                ("zulu()berta", "((<null> zulu (<null> () <null>)) berta <null>)");
        }

        [UnitTest]
        public static void ParenthesisSuffixAndPrefixAndSequence()
        {
            ParserUtil.ParseAndCheck
                (
                    "zulu aaa()(anton)berta",
                    "((((<null> zulu <null>) aaa (<null> () <null>)) <nameless> ((<null> anton <null>) () <null>)) berta <null>)"
                );
            ParserUtil.ParseAndCheck
                (
                    "zulu()(anton)berta",
                    "(((<null> zulu (<null> () <null>)) <nameless> ((<null> anton <null>) () <null>)) berta <null>)"
                );
        }

        [UnitTest]
        public static void RealParenthesisTest()
            => ParserUtil.ParseAndCheck
                (
                    " (x5 type x5) instance () dump_print           ",
                    "((((((<null> x5 <null>) type <null>) x5 <null>) () <null>) instance (<null> () <null>)) dump_print <null>)"
                );
    }
}