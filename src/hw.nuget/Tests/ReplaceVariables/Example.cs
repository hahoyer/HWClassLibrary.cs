using System;
using hw.Debug;
using hw.ReplaceVariables;
using hw.UnitTest;

namespace hw.Tests.ReplaceVariables
{
    [TestFixture]
    static public class Example
    {
        [Test]
        public static void TestMethod()
        {
            var myData = new MyData();
            var format = "i: $(InvoiceDate) - n: $(Now) - id: $(InvoiceDate.Date) - in: $(ImportantNumber) - asis: $(=ImportantNumber) ";
            var expectedResult = "i: 22.04.2014 22:57:34 - n: 27.05.2014 22:57:34 - id: 22.04.2014 - in: 42 - asis: $(ImportantNumber) ";
            var result = format.ReplaceVariables(myData);
            Tracer.Assert(result == expectedResult);
        }

        sealed class MyData
        {
            [Visible]
            public DateTimeSpecial InvoiceDate { get { return new DateTimeSpecial(new DateTime(2014, 5, 27, 22, 57, 34).AddDays(-35)); } }
            [Visible]
            public DateTimeSpecial Now = new DateTimeSpecial(new DateTime(2014, 5, 27, 22, 57, 34));
            [Visible]
            public int ImportantNumber = 42;
        }

        sealed class DateTimeSpecial
        {
            DateTime _value;
            public DateTimeSpecial(DateTime value) { _value = value; }
            [Visible]
            int Day { get { return _value.Day; } }
            [Visible]
            int Month { get { return _value.Month; } }
            [Visible]
            int Year { get { return _value.Year; } }
            [Visible]
            string Date { get { return _value.Date.ToShortDateString(); } }
            public override string ToString() { return _value.ToString(); }
        }
    }
}