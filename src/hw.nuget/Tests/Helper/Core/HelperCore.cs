using System;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;

namespace hw.Tests.Helper.Core
{
    [TestFixture]
    public static class DateTimeExtender
    {
        [Test]
        public static void Format3DigitsSmart()
        {
            var t1 = TimeSpan.FromMinutes(3.12);
            var t2 = TimeSpan.FromMinutes(3);
            var t3 = TimeSpan.FromMinutes(3.0012);

            Tracer.Assert(t1.Format3Digits() == t1.Format3Digits(omitZeros:true), () => t1.Format3Digits() + " != " + t1.Format3Digits(omitZeros:true));
            Tracer.Assert(t2.Format3Digits() != t2.Format3Digits(omitZeros: true) && t2.Format3Digits(omitZeros: true) == "3m", () => t2.Format3Digits() + " != " + t2.Format3Digits(omitZeros: true));
            Tracer.Assert(t3.Format3Digits() != t3.Format3Digits(omitZeros:true), () => t3.Format3Digits() + " != " + t3.Format3Digits(omitZeros:true));
        }
    }
}