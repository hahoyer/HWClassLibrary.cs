#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using hw.Debug;
using JetBrains.Annotations;

namespace hw.Helper
{
    /// <summary>
    ///     String helper functions.
    /// </summary>
    public static class StringExtender
    {
        /// <summary>
        ///     Indent paramer by 4 spaces
        /// </summary>
        /// <param name="x"> The x. </param>
        /// <returns> </returns>
        public static string Indent(this string x) { return x.Replace("\n", "\n    "); }

        /// <summary>
        ///     Indent paramer by 4 times count spaces
        /// </summary>
        /// <param name="x"> The x. </param>
        /// <param name="count"> The count. </param>
        /// <returns> </returns>
        public static string Indent(this string x, int count) { return x.Replace("\n", "\n" + Repeat("    ", count)); }

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
        /// <param name="Left"> </param>
        /// <param name="data"> </param>
        /// <param name="Right"> </param>
        /// <returns> </returns>
        public static string Surround(this string data, string Left, string Right)
        {
            if(data.IndexOf("\n", StringComparison.Ordinal) < 0)
                return Left + data + Right;
            return "\n" + Left + Indent("\n" + data) + "\n" + Right;
        }

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

        public static File FileHandle(this string name) { return File.Create(name); }
        public static string PathCombine(this string head, params string[] tail) { return Path.Combine(head, Path.Combine(tail)); }

        public static string UnderScoreToCamelCase(this string name) { return name.Split('_').Select(ToLowerFirstUpper).Stringify(""); }

        public static string ToLowerFirstUpper(this string text) { return text.Substring(0, 1).ToUpperInvariant() + text.Substring(1).ToLowerInvariant(); }
        public static string TableNameToClassName(this string name) { return name.UnderScoreToCamelCase().ToSingular(); }
        [StringFormatMethod("pattern")]
        public static string ReplaceArgs(this string pattern, params object[] args) { return String.Format(pattern, args); }
        public static bool Matches(this string input, string pattern) { return new Regex(pattern).IsMatch(input); }

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
    }
}