using System;
using System.Globalization;
using System.Threading;

namespace hw.Helper
{
    public static class DateTimeExtender
    {
        public static string Format(this DateTime dateTime)
        {
            var result = "";
            result += dateTime.Hour.ToString("00");
            result += ":";
            result += dateTime.Minute.ToString("00");
            result += ":";
            result += dateTime.Second.ToString("00");
            result += ".";
            result += dateTime.Millisecond.ToString("000");
            result += " ";
            result += dateTime.Day.ToString("00");
            result += ".";
            result += dateTime.Month.ToString("00");
            result += ".";
            result += dateTime.Year.ToString("00");
            return result;
        }

        public static string DynamicShortFormat(this DateTime dateTime, bool showMiliseconds)
        {
            var result = "";
            result += dateTime.Hour.ToString("00");
            result += ":";
            result += dateTime.Minute.ToString("00");
            result += ":";
            result += dateTime.Second.ToString("00");
            if(showMiliseconds)
            {
                result += ".";
                result += dateTime.Millisecond.ToString("000");
            }

            var nowDate = DateTime.Now.Date;
            var sameYear = nowDate.Year == dateTime.Year;
            var sameMonth = sameYear && nowDate.Month == dateTime.Month;
            var sameDay = sameMonth && nowDate.Day == dateTime.Day;

            if(!sameDay)
            {
                result += " ";
                result += dateTime.Day.ToString("00");
                result += ".";
            }

            if(!sameMonth)
            {
                result += dateTime.Month.ToString("00");
                result += ".";
            }

            if(!sameYear)
                result += dateTime.Year.ToString("00");
            return result;
        }

        public static string Format3Digits(this TimeSpan timeSpan, bool omitZeros = true, bool useSymbols = true)
        {
            if(timeSpan.TotalDays >= 1)
                return timeSpan.TotalDays.ToString("0.00") + "d";
            if(timeSpan.Hours > 0)
                return timeSpan.Hours + OmitCheck(":", timeSpan.Minutes, omitZeros) + "h";
            if(timeSpan.Minutes > 0)
                return timeSpan.Minutes + OmitCheck(":", timeSpan.Seconds, omitZeros) + (useSymbols ? "'" : "m");

            var nanoSeconds = ((long) (timeSpan.TotalMilliseconds * 1000 * 1000)).Format3Digits(omitZeros) + "ns";
            return nanoSeconds.Replace("kns", "µs").Replace("Mns", "ms").Replace("Gns", useSymbols ? "\"" : "s");
        }

        static string OmitCheck(string delimiter, int value, bool omitZeros)
        {
            if(omitZeros && value == 0)
                return "";
            return delimiter + value.ToString("00");
        }

        public static int WeekNumber(this DateTime dateTime, CultureInfo culture)
        {
            if(culture == null)
                culture = CultureInfo.CurrentCulture;
            var dateTimeFormatInfo = culture.DateTimeFormat;
            return dateTimeFormatInfo
                .Calendar
                .GetWeekOfYear(dateTime, dateTimeFormatInfo.CalendarWeekRule, dateTimeFormatInfo.FirstDayOfWeek);
        }

        public static TimeSpan Seconds(this double value) => TimeSpan.FromSeconds(value);
        public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);
        public static TimeSpan Minutes(this double value) => TimeSpan.FromMinutes(value);
        public static TimeSpan Minutes(this int value) => TimeSpan.FromMinutes(value);
        public static TimeSpan Hours(this double value) => TimeSpan.FromHours(value);
        public static TimeSpan Hours(this int value) => TimeSpan.FromHours(value);
        public static TimeSpan Days(this double value) => TimeSpan.FromDays(value);
        public static TimeSpan Days(this int value) => TimeSpan.FromDays(value);

        public static void Sleep(this TimeSpan value) => Thread.Sleep(value);
    }
}