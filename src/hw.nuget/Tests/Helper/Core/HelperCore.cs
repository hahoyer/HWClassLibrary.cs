﻿using System;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;

namespace hw.Tests.Helper.Core
{
    [TestFixture]
    public static class DateTimeExtender
    {
        [Test]
        public static void Format3Digits()
        {
            var t1 = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(12);
            var t2 = TimeSpan.FromMinutes(3);
            var t3 = TimeSpan.FromMinutes(3.0012);

            var format3Digits1 = t1.Format3Digits();
            var format3Digitss1 = t1.Format3Digits(omitZeros:true);
            var format3Digitss2 = t2.Format3Digits(omitZeros: true);



            Tracer.Assert(format3Digits1 == format3Digitss1, () => format3Digits1 + " != " + format3Digitss1);
            Tracer.Assert(format3Digitss2 == "3m", () => format3Digitss2);
            Tracer.Assert(t2.Format3Digits() != format3Digitss2 , () => t2.Format3Digits() + " != " + format3Digitss2);
            Tracer.Assert(t3.Format3Digits() != t3.Format3Digits(omitZeros:true), () => t3.Format3Digits() + " != " + t3.Format3Digits(omitZeros:true));

            var format3Digitsu1 = t1.Format3Digits(useSymbols: true);
            Tracer.Assert(format3Digitsu1 == "3:12'", () => format3Digitsu1);

            var t4 = TimeSpan.FromSeconds(3.1);
            var format3Digitsu4 = t4.Format3Digits(useSymbols: true);
            Tracer.Assert(format3Digitsu4 == "3.10\"", () => format3Digitsu4);

        }
    }
}