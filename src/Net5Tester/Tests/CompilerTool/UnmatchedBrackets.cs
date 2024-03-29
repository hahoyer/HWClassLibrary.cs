using hw.UnitTest;
using NUnit.Framework;
// ReSharper disable CheckNamespace

namespace Net5Tester.CompilerTool
{
    [UnitTest]
    [TestFixture]
    public sealed class UnmatchedBrackets : DependenceProvider
    {
        [UnitTest]
        [Test]
        public static void MissingRight()
            => ParserUtil.ParseAndCheck
                (
                    "{a b( } c",
                    "((((<null> a <null>) b (<null> ?(? <null>)) () <null>) c <null>)"
                );

        [UnitTest]
        [Test]
        public static void MissingLeft()
            =>  ParserUtil.ParseAndCheck
                (
                    "{a b) } c",
                    "(((((<null> a <null>) b <null>) ?)? <null>) () <null>) c <null>)"
                );
    }
}