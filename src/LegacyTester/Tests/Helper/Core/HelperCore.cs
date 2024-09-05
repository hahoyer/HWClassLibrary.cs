using System;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
// ReSharper disable IdentifierTypo
// ReSharper disable CheckNamespace

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


            (format3Digits1 == format3Digitss1).Assert
                (() => format3Digits1 + " != " + format3Digitss1);
            (format3Digitss2 == "3'").Assert(() => format3Digitss2);
            (t2.Format3Digits(false) != format3Digitss2).Assert
            (() => t2.Format3Digits(false) + " != " + format3Digitss2);
            (t3.Format3Digits(false) != t3.Format3Digits()).Assert
            (() => t3.Format3Digits(false) + " != " + t3.Format3Digits());

            var format3Digitsu1 = t1.Format3Digits();
            (format3Digitsu1 == "3:12'").Assert(() => format3Digitsu1);

            var t4 = TimeSpan.FromSeconds(3.1);
            var format3Digitsu4 = t4.Format3Digits();
            (format3Digitsu4 == "3.1\"").Assert(() => format3Digitsu4);
        }
    }
}