using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Helper
{
    /// <summary>
    ///     String helper functions.
    /// </summary>
    public static class StringExtender
    {
        /// <summary>
        ///     Indent paramer by 4 times count spaces
        /// </summary>
        /// <param name="x"> The x. </param>
        /// <param name="tabString"></param>
        /// <param name="count"> The count. </param>
        /// <param name="isLineStart"></param>
        /// <returns> </returns>
        public static string Indent(this string x, int count = 1, string tabString = "    ", bool isLineStart = false)
        {
            var effectiveTabString = tabString.Repeat(count);
            return (isLineStart ? effectiveTabString : "") + x.Replace("\n", "\n" + effectiveTabString);
        }

        /// <summary>
        ///     Repeats the specified s.
        /// </summary>
        /// <param name="s"> The s. </param>
        /// <param name="count"> The count. </param>
        /// <returns> </returns>
        /// created 15.10.2006 14:38
        public static string Repeat(this string s, int count)
        {
            var result = "";
            for(var i = 0; i < count; i++)
                result += s;
            return result;
        }

        /// <summary>
        ///     Surrounds string by left and right parenthesis. If string contains any carriage return, some indenting is done also
        /// </summary>
        /// <param name="left"> </param>
        /// <param name="data"> </param>
        /// <param name="right"> </param>
        /// <returns> </returns>
        public static string Surround(this string data, string left, string right)
        {
            if(data.IndexOf("\n", StringComparison.Ordinal) < 0)
                return left + data + right;
            return "\n" + left + Indent("\n" + data) + "\n" + right;
        }

        public static string SaveConcat(this string delim, params string[] data) { return data.Where(d => !string.IsNullOrEmpty(d)).Stringify(delim); }

        /// <summary>
        ///     Converts string to a string literal.
        /// </summary>
        /// <param name="x"> The x. </param>
        /// <returns> </returns>
        /// created 08.01.2007 18:37
        public static string Quote(this string x)
        {
            if(x == null)
                return "null";
            return "\"" + x.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        /// <summary>
        ///     Dumps the bytes as hex string.
        /// </summary>
        /// <param name="bytes"> The bytes. </param>
        /// <returns> </returns>
        public static string HexDump(this byte[] bytes)
        {
            var result = "";
            for(var i = 0; i < bytes.Length; i++)
            {
                result += HexDumpFiller(i, bytes.Length);
                result += bytes[i].ToString("x2");
            }
            result += HexDumpFiller(bytes.Length, bytes.Length);
            return result;
        }

        static string HexDumpFiller(int i, int length)
        {
            Tracer.Assert(length < 16);
            if(0 == length)
                return "x[]";
            if(i == 0)
                return "x[";
            if(i == length)
                return "]";
            if(i % 4 == 0)
                return " ";
            return "";
        }

        public static string ExecuteCommand(this string command)
        {
            var procStartInfo = new ProcessStartInfo("cmd", "/c " + command) {RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true};
            var proc = new Process {StartInfo = procStartInfo};
            proc.Start();
            return proc.StandardOutput.ReadToEnd();

        }

        public static hw.Helper.File FileHandle(this string name) => hw.Helper.File.Create(name);
        public static SmbFile ToSmbFile(this string name, bool autoCreateDirectories = true) => SmbFile.Create(name, autoCreateDirectories);
        public static string PathCombine(this string head, params string[] tail) => Path.Combine(head, Path.Combine(tail));

        public static string UnderScoreToCamelCase(this string name) => name.Split('_').Select(ToLowerFirstUpper).Stringify("");

        public static string ToLowerFirstUpper(this string text) => text.Substring(0, 1).ToUpperInvariant() + text.Substring(1).ToLowerInvariant();
        public static string TableNameToClassName(this string name) => name.UnderScoreToCamelCase().ToSingular();

        [StringFormatMethod("pattern")]
        public static string ReplaceArgs(this string pattern, params object[] args) => String.Format(pattern, args);

        public static bool Matches(this string input, string pattern) => new Regex(pattern).IsMatch(input);

        public static IEnumerable<string> Split(this string target, params int[] sizes)
        {
            var start = 0;
            foreach(var length in sizes.Select(size => Math.Max(0, Math.Min(target.Length - start, size))))
            {
                yield return target.Substring(start, length);
                start += length;
            }
            yield return target.Substring(start);
        }

        public static string Format(this string x, StringAligner aligner) => aligner.Format(x);

        internal static int BeginMatch(string a, string b)
        {
            for(var i = 0;; i++)
                if(i >= a.Length || i >= b.Length || a[i] != b[i])
                    return i;
        }

        /// <summary>
        ///     Provide deafault string aligner with columnCount columns
        /// </summary>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        public static StringAligner StringAligner(this int columnCount)
        {
            var stringAligner = new StringAligner();
            for(var i = 0; i < columnCount; i++)
                stringAligner.AddFloatingColumn("  ");
            return stringAligner;
        }
    }
}