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
        /// <param name="x"></param>
        /// <returns></returns>
        public static string Indent(string x)
        {
            return x.Replace("\n", "\n    ");
        }

        /// <summary>
        /// Indent paramer by 4 times count spaces
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static string Indent(string x, int count)
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
        public static string Repeat(string s, int count)
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
        public static string Surround(string Left, string data, string Right)
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
        public static string Quote(string x)
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
    }

    /// <summary>
    /// Class to align strings
    /// </summary>
    public class StringAligner
    {
        private readonly List<FloatingColumn> _floatingColumns = new List<FloatingColumn>();

        /// <summary>
        /// Adds the floating column.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// created 15.10.2006 14:58
        public void AddFloatingColumn(params string[] pattern)
        {
            var c = _floatingColumns.Count;
            _floatingColumns.Add(new StringPattern(pattern));
            if(c > 0)
                _floatingColumns[c - 1].FindStartFailed = _floatingColumns[c].FindStart;
        }

        /// <summary>
        /// Formats the specified string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns></returns>
        /// created 15.10.2006 14:59
        public string Format(string s)
        {
            var ss = s.Split('\n');
            var p = new int[ss.Length];
            for(var i = 0; i < _floatingColumns.Count; i++)
                _floatingColumns[i].Format(ss, p);
            var result = "";
            for(var i = 0; i < ss.Length; i++)
                result += ss[i] + "\n";
            return result;
        }
    }

    /// <summary>
    /// Floating column with string pattern. Spaces inserted before pattern. 
    /// </summary>
    public class StringPattern : FloatingColumn
    {
        private readonly string[] _pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringPattern"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// created 15.10.2006 14:57
        public StringPattern(string[] pattern)
        {
            _pattern = pattern;
        }

        /// <summary>
        /// Finds the start.of column marker
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="start">The old start position.</param>
        /// <returns></returns>
        /// created 15.10.2006 15:22
        /// created 15.10.2006 15:23
        public override int FindStart(string s, int start)
        {
            var result = s.IndexOf(_pattern[0], start);
            for(var i = 1; i < _pattern.Length; i++)
            {
                var result1 = s.IndexOf(_pattern[i], start);
                if(result == -1 || result1 != -1 && result1 < result)
                    result = result1;
            }

            if(result != -1)
                return result;

            return FindStartFailed(s, start);
        }

        /// <summary>
        /// Finds the end.of column marker
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="start">The old start position.</param>
        /// <returns></returns>
        /// created 15.10.2006 15:22
        public override int FindEnd(string s, int start)
        {
            var result = s.IndexOf(_pattern[0], start);
            var ip = 0;
            for(var i = 1; i < _pattern.Length; i++)
            {
                var result1 = s.IndexOf(_pattern[i], start);
                if(result == -1 || result1 != -1 && result1 < result)
                {
                    ip = i;
                    result = result1;
                }
            }

            if(result != -1)
                return result + _pattern[ip].Length;

            return start;
        }
    }

    /// <summary>
    /// Description of a floating column
    /// </summary>
    public abstract class FloatingColumn : Dumpable
    {
        /// <summary>
        /// called when find start failed
        /// </summary>
        public FindStartFailedDelegate FindStartFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingColumn"/> class.
        /// </summary>
        /// created 15.10.2006 15:44
        protected FloatingColumn()
        {
            FindStartFailed = ((s, i) => s.Length);
        }

        /// <summary>
        /// Formats the specified lines beginning at specified positions .
        /// </summary>
        /// <param name="lines">The lines, will be modified.</param>
        /// <param name="positions">The positions, will be set to end of part handled.</param>
        /// created 15.10.2006 15:07
        public void Format(string[] lines, int[] positions)
        {
            var count = lines.Length;
            for(var i = 0; i < count; i++)
                positions[i] = FindStart(lines[i], positions[i]);
            while(Levelling(count, lines, positions))
                continue;
            for(var i = 0; i < count; i++)
                positions[i] = FindStart(lines[i], positions[i]);
            for(var i = 0; i < count; i++)
                positions[i] = FindEnd(lines[i], positions[i]);
        }

        private static void FormatLine(ref string line, ref int position, int offset)
        {
            if(offset == 0)
                return;
            line = line.Insert(position, HWString.Repeat(" ", offset));
            position += offset;
        }

        private static bool Levelling(int count, string[] lines, int[] positions)
        {
            for(var i = 1; i < count; i++)
            {
                var delta = positions[i] - positions[i - 1];
                if(delta < -1)
                {
                    FormatLine(ref lines[i], ref positions[i], -delta - 1);
                    return true;
                }
                if(delta > 1)
                {
                    FormatLine(ref lines[i - 1], ref positions[i - 1], delta - 1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the start.of column marker
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="i">The old start position.</param>
        /// <returns></returns>
        /// created 15.10.2006 15:22
        public abstract int FindStart(string s, int i);

        /// <summary>
        /// Finds the end.of column marker
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="i">The old start position.</param>
        /// <returns></returns>
        /// created 15.10.2006 15:22
        public abstract int FindEnd(string s, int i);

        /// <summary>
        /// called when find start failed
        /// </summary>
        public delegate int FindStartFailedDelegate(string s, int i);
    }
}