using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    /// <summary>
    /// String helper functions.
    /// </summary>
    public static class HWString 
    {
        /// <summary>
        /// Indent paramer by 4 spaces
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public static string Indent(this string x)
        {
            return x.Replace("\n", "\n    ");
        }

        /// <summary>
        /// Indent paramer by 4 times count spaces
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static string Indent(this string x, int count)
        {
            return x.Replace("\n", "\n" + Repeat("    ", count));
        }

        /// <summary>
        /// Repeats the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// created 15.10.2006 14:38
        public static string Repeat(this string s, int count)
        {
            var result = "";
            for(var i = 0; i < count; i++)
                result += s;
            return result;
        }

        /// <summary>
        /// Surrounds string by left and right parenthesis. 
        /// If string contains any carriage return, some indenting is done also 
        /// </summary>
        /// <param name="Left"></param>
        /// <param name="data"></param>
        /// <param name="Right"></param>
        /// <returns></returns>
        public static string Surround(this string data, string Left, string Right)
        {
            if(data.IndexOf("\n") < 0)
                return Left + data + Right;
            return "\n" + Left + Indent("\n" + data) + "\n" + Right;
        }

        /// <summary>
        /// Converts string to a string literal.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        /// created 08.01.2007 18:37
        public static string Quote(this string x)
        {
            return "\"" + x.Replace("\"", "\"\"") + "\"";
        }

        public static string HexDump(byte[] bytes)
        {
            var result = "";
            for (var i = 0; i < bytes.Length; i++)
            {
                result += HexDumpFiller(i, bytes.Length);
                result += bytes[i].ToString("x2");
            }
            result += HexDumpFiller(bytes.Length, bytes.Length);
            return result;
        }

        private static string HexDumpFiller(int i, int length)
        {
            Tracer.Assert(length < 16);
            if(0 == length)
                return "x[]";
            if(i == 0)
                return "x[";
            if(i == length)
                return "]";
            if(i%4 == 0)
                return " ";
            return "";
        }

        public static Sequence<T> Sequence<T>() { return new Sequence<T>(); }
        public static Sequence<T> Sequence<T>(T a) { return new Sequence<T>(a); }
        public static Sequence<T> Sequence<T>(IList<T> a) { return new Sequence<T>(a); }
    }

    static public class HWDateTime
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
            if (showMiliseconds)
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
            if (!sameYear)
                result += dateTime.Year.ToString("00");
            return result;
        }
    }
}