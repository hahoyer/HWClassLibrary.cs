using System;
using System.Globalization;
using hw.DebugFormatter;
using hw.ReplaceVariables;
using hw.UnitTest;
// ReSharper disable CheckNamespace

namespace Net5Tester.ReplaceVariables
{
    [UnitTest]
    public static class Example
    {
        sealed class MyData
        {
            [Visible]
            public int ImportantNumber = 42;

            [Visible]
            public DateTimeSpecial Now = new DateTimeSpecial(new DateTime(2014, 5, 27, 22, 57, 34));

            [Visible]
            public DateTimeSpecial InvoiceDate
                => new DateTimeSpecial(new DateTime(2014, 5, 27, 22, 57, 34).AddDays(-35));
        }

        sealed class DateTimeSpecial
        {
            DateTime Value;
            public DateTimeSpecial(DateTime value) => Value = value;

            [Visible]
            int Day => Value.Day;

            [Visible]
            int Month => Value.Month;

            [Visible]
            int Year => Value.Year;

            [Visible]
            string Date => Value.Date.ToShortDateString();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        [UnitTest]
        public static void TestMethod()
        {
            var myData = new MyData();
            var format =
                "i: $(InvoiceDate) - n: $(Now) - id: $(InvoiceDate.Date) - in: $(ImportantNumber) - asis: $(=ImportantNumber) ";
            var expectedResult =
                "i: 04/22/2014 22:57:34 - n: 05/27/2014 22:57:34 - id: 22.04.2014 - in: 42 - asis: $(ImportantNumber) ";
            var result = format.ReplaceVariables(myData).ReplaceProtected();
            Tracer.Assert(result == expectedResult, 
                () => "\n--Expected--\n" + expectedResult + "|----\n--Found--\n" + result + "|----\n");
        }
    }
}