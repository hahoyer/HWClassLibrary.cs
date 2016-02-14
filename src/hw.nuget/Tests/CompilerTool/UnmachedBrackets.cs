using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using NUnit.Framework;

namespace hw.Tests.CompilerTool
{
    [UnitTest]
    [TestFixture]
    public sealed class UnmachedBrackets : DependantAttribute
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