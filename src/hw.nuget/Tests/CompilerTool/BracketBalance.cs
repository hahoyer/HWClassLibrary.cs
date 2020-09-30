using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.UnitTest;
using NUnit.Framework;

namespace hw.Tests.CompilerTool
{
    [TestFixture]
    [UnitTest]
    public sealed class BracketBalance
    {
        [UnitTest]
        public static void Simple()
        {
            var expr = "a b c";
            var result = ParserUtil.Parse(expr);
            var items = result.Tokens;
            var depth = 0;
            foreach(var syntax in items)
                depth += syntax.BracketBalance();
            Tracer.Assert
                (
                    depth == 0,
                    () =>
                        items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                            .Stringify(" "));
        }

        [UnitTest]
        [Test]
        public static void Simple1()
        {
            var expr = "(a b c)";
            var result = ParserUtil.Parse(expr);
            var items = result.Tokens;
            var depth = 0;
            foreach(var syntax in items)
                depth += syntax.BracketBalance();
            Tracer.Assert
                (
                    depth == 0,
                    () =>
                        items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                            .Stringify(" "));
        }

        [UnitTest]
        [Test]
        public static void Mixed()
        {
            var expr = "(a b {}c)";
            var result = ParserUtil.Parse(expr);
            var items = result.Tokens;
            var depth = 0;
            foreach(var syntax in items)
                depth += syntax.BracketBalance();
            Tracer.Assert
                (
                    depth == 0,
                    () =>
                        items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                            .Stringify(" "));
        }

        [UnitTest]
        [Test]
        public static void MixedUnmatched1()
        {
            var expr = "a s{a b (c}";
            var result = ParserUtil.Parse(expr);
            var items = result.Tokens;
            var depth = 0;
            foreach(var syntax in items)
                depth += syntax.BracketBalance();
            Tracer.Assert
                (
                    depth == 0,
                    () =>
                        items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                            .Stringify(" "));
        }

        [UnitTest]
        [Test]
        public static void MixedUnmatched2()
        {
            var expr = "a s{a b )c}";
            var result = ParserUtil.Parse(expr);
            var items = result.Tokens;
            var depth = 0;
            foreach(var syntax in items)
                depth += syntax.BracketBalance();
            Tracer.Assert
                (
                    depth == 0,
                    () =>
                        items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                            .Stringify(" "));
        }

        [UnitTest]
        [Test]
        public static void Unmatched1()
        {
            var expr = "a sa b (c";
            var result = ParserUtil.Parse(expr);
            var items = result.Tokens;
            var depth = 0;
            foreach (var syntax in items)
                depth += syntax.BracketBalance();
            Tracer.Assert
                (
                    depth == 0,
                    () =>
                        items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                            .Stringify(" "));
        }

        [UnitTest]
        [Test]
        public static void Unmatched2()
        {
            var expr = "a sa b )c";
            var result = ParserUtil.Parse(expr);
            var items = result.Tokens;
            var depth = 0;
            foreach (var syntax in items)
                depth += syntax.BracketBalance();
            Tracer.Assert
                (
                    depth == 0,
                    () =>
                        items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                            .Stringify(" "));
        }
    }
}