using System;
using System.Collections.Generic;
using System.Linq;
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
            => ParserUtil.ParseAndCheck
            ("--> b c", "((<null> b <null>) c <null>)");

        [UnitTest]
        public static void MultipleEntries() 
            => ParserUtil.ParseAndCheck
            ("--> (b c) c", "((((<null> b <null>) c <null>) () <null>) c <null>)");
    }
}