using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public sealed class StringAligner
{
    readonly List<FloatingColumn> FloatingColumns = new();

    public void AddFloatingColumn(params string[] pattern)
    {
        var c = FloatingColumns.Count;
        FloatingColumns.Add(new StringPattern(pattern));
        if(c > 0)
            FloatingColumns[c - 1].FindStartFailed = FloatingColumns[c].FindStart;
    }

    public string Format
        (string lines) => Format(lines.Split('\n')).Aggregate("", (current, temp) => current + temp + "\n");

    public string[] Format(string[] lines)
    {
        var result = lines.Select(target => target).ToArray();
        var p = new int[result.Length];
        foreach(var column in FloatingColumns)
            column.Format(result, p);
        return result;
    }
}

/// <summary>
///     Description of a floating column
/// </summary>
[PublicAPI]
public abstract class FloatingColumn : Dumpable
{
    /// <summary>
    ///     called when find start failed
    /// </summary>
    public Func<string, int, int> FindStartFailed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FloatingColumn" /> class.
    /// </summary>
    /// created 15.10.2006 15:44
    protected FloatingColumn() => FindStartFailed = (s, i) => s.Length;

    /// <summary>
    ///     Finds the start.of column marker
    /// </summary>
    /// <param name="s"> The string. </param>
    /// <param name="i"> The old start position. </param>
    /// <returns> </returns>
    /// created 15.10.2006 15:22
    public abstract int FindStart(string s, int i);

    /// <summary>
    ///     Finds the end.of column marker
    /// </summary>
    /// <param name="s"> The string. </param>
    /// <param name="i"> The old start position. </param>
    /// <returns> </returns>
    /// created 15.10.2006 15:22
    public abstract int FindEnd(string s, int i);

    /// <summary>
    ///     Formats the specified lines beginning at specified positions .
    /// </summary>
    /// <param name="lines"> The lines, will be modified. </param>
    /// <param name="positions"> The positions, will be set to end of part handled. </param>
    /// created 15.10.2006 15:07
    public void Format(string[] lines, int[] positions)
    {
        var count = lines.Length;
        if(count > 0 && lines[count - 1] == "")
            count--;
        for(var i = 0; i < count; i++)
            positions[i] = FindStart(lines[i], positions[i]);
        while(Levelling(count, lines, positions)) { }

        for(var i = 0; i < count; i++)
            positions[i] = FindStart(lines[i], positions[i]);
        for(var i = 0; i < count; i++)
            positions[i] = FindEnd(lines[i], positions[i]);
    }

    static void FormatLine(ref string line, ref int position, int offset)
    {
        if(offset == 0)
            return;
        line = line.Insert(position, " ".Repeat(offset));
        position += offset;
    }

    // ReSharper disable once IdentifierTypo
    static bool Levelling(int count, string[] lines, int[] positions)
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
}

/// <summary>
///     Floating column with string pattern. Spaces inserted before pattern.
/// </summary>
sealed class StringPattern : FloatingColumn
{
    readonly string[] Pattern;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StringPattern" /> class.
    /// </summary>
    /// <param name="pattern"> The pattern. </param>
    /// created 15.10.2006 14:57
    public StringPattern(string[] pattern) => Pattern = pattern;

    /// <summary>
    ///     Finds the start.of column marker
    /// </summary>
    /// <param name="s"> The string. </param>
    /// <param name="start"> The old start position. </param>
    /// <returns> </returns>
    /// created 15.10.2006 15:22
    /// created 15.10.2006 15:23
    public override int FindStart(string s, int start)
    {
        var result = s.IndexOf(Pattern[0], start, StringComparison.Ordinal);
        for(var i = 1; i < Pattern.Length; i++)
        {
            var result1 = s.IndexOf(Pattern[i], start, StringComparison.Ordinal);
            if(result == -1 || result1 != -1 && result1 < result)
                result = result1;
        }

        if(result != -1)
            return result;

        return FindStartFailed(s, start);
    }

    /// <summary>
    ///     Finds the end.of column marker
    /// </summary>
    /// <param name="s"> The string. </param>
    /// <param name="start"> The old start position. </param>
    /// <returns> </returns>
    /// created 15.10.2006 15:22
    public override int FindEnd(string s, int start)
    {
        var result = s.IndexOf(Pattern[0], start, StringComparison.Ordinal);
        var ip = 0;
        for(var i = 1; i < Pattern.Length; i++)
        {
            var result1 = s.IndexOf(Pattern[i], start, StringComparison.Ordinal);
            if(result == -1 || result1 != -1 && result1 < result)
            {
                ip = i;
                result = result1;
            }
        }

        if(result != -1)
            return result + Pattern[ip].Length;

        return start;
    }
}