using System;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
// ReSharper disable IdentifierTypo

namespace hw.Tests.Helper.Core
{
    [UnitTest]
    public static class DateTimeExtender
    {
        [UnitTest]
        public static void Format3Digits()
        {
            var t1 = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(12);
            var t2 = TimeSpan.FromMinutes(3);
            var t3 = TimeSpan.FromMinutes(3.0012);

            var format3Digits1 = t1.Format3Digits(false);
            var format3Digitss1 = t1.Format3Digits();
            var format3Digitss2 = t2.Format3Digits();


            Tracer.Assert
                (format3Digits1 == format3Digitss1, () => format3Digits1 + " != " + format3Digitss1);
            Tracer.Assert(format3Digitss2 == "3'", () => format3Digitss2);
            Tracer.Assert
            (
                t2.Format3Digits(false) != format3Digitss2,
                () => t2.Format3Digits(false) + " != " + format3Digitss2);
            Tracer.Assert
            (
                t3.Format3Digits(false) != t3.Format3Digits(),
                () => t3.Format3Digits(false) + " != " + t3.Format3Digits());

            var format3Digitsu1 = t1.Format3Digits();
            Tracer.Assert(format3Digitsu1 == "3:12'", () => format3Digitsu1);

            var t4 = TimeSpan.FromSeconds(3.1);
            var format3Digitsu4 = t4.Format3Digits();
            Tracer.Assert(format3Digitsu4 == "3.1\"", () => format3Digitsu4);
        }
    }
}