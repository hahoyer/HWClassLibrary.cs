using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
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

        public static string Format3Digits(this TimeSpan timeSpan)
        {
            if(timeSpan.TotalMinutes >= 1)
                return timeSpan.ToString();
            var nanoSeconds = ((long) (timeSpan.TotalMilliseconds*1000*1000)).Format3Digits() + "ns";
            return nanoSeconds.Replace("kns", "µs").Replace("Mns", "ms").Replace("Gns", "s");
        }
    }
}