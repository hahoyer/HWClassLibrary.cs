using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;

namespace hw.Tests.Helper.Core
{
    [TestFixture]
    public static class TestLongExtender
    {
        [Test]
        public static void TestMethod()
        {
            InternalTest(1, "1");
            InternalTest(12, "12");
            InternalTest(123, "123");
            InternalTest(1234, "1.23k");
            InternalTest(12345, "12.3k");
            InternalTest(123456, "123k");
            InternalTest(1234567, "1.23M");
            InternalTest(12345678, "12.3M");
            InternalTest(123456789, "123M");
            InternalTest(1234567890, "1.23G");
            InternalTest(12345678901, "12.3G");
            InternalTest(123456789012, "123G");
            InternalTest(1234567890123, "1.23T");
            InternalTest(12345678901234, "12.3T");
            InternalTest(123456789012345, "123T");
            InternalTest(1234567890123456, "1.23P");
            InternalTest(12345678901234567, "12.3P");
            InternalTest(123456789012345678, "123P");

            InternalTest(1000, "1k");
            InternalTest(1200, "1.2k");
            InternalTest(1230, "1.23k");
            InternalTest(1234, "1.23k");

            InternalTest(10000, "10k");
            InternalTest(12000, "12k");
            InternalTest(12300, "12.3k");
            InternalTest(12340, "12.3k");

            InternalTest(100000, "100k");
            InternalTest(120000, "120k");
            InternalTest(123000, "123k");
            InternalTest(123400, "123k");
        }

        static void InternalTest(long x, string y) { Tracer.Assert(x.Format3Digits() == y, () => x + " != " + y, stackFrameDepth: 1); }
    }
}