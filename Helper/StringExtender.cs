using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    /// <summary>
    /// String helper functions.
    /// </summary>
    public static class StringExtender 
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

        /// <summary>
        /// Dumps the bytes as hex string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string HexDump(this byte[] bytes)
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

        public static string ExecuteCommand(this string command)
        {
            var procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command)
                                    {
                                        RedirectStandardOutput = true,
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    };
            var proc = new System.Diagnostics.Process {StartInfo = procStartInfo};
            proc.Start();
            return proc.StandardOutput.ReadToEnd();
        }

    }
}