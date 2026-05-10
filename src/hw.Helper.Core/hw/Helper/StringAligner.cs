using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Aligns multi-line text into columns by inserting spaces. Columns are anchored at literal
///     marker substrings ("floating columns"); the first marker that appears on a line determines the
///     column position, and subsequent lines are padded so that the markers line up.
/// </summary>
[PublicAPI]
public sealed class StringAligner
{
    readonly List<FloatingColumn> FloatingColumns = new();

    /// <summary>
    ///     Appends a floating column anchored at the first occurrence of any of
    ///     <paramref name="pattern" />. Columns are processed in the order they were added.
    /// </summary>
    /// <param name="pattern"> Marker strings; the first one found per line wins. </param>
    public void AddFloatingColumn(params string[] pattern)
    {
        var c = FloatingColumns.Count;
        FloatingColumns.Add(new StringPattern(pattern));
        if(c > 0)
            FloatingColumns[c - 1].FindStartFailed = FloatingColumns[c].FindStart;
    }

    /// <summary>Aligns a single string by splitting it on <c>\n</c> and re-joining the result with <c>\n</c>-terminators.</summary>
    public string Format
        (string lines) => Format(lines.Split('\n')).Aggregate("", (current, temp) => current + temp + "\n");

    /// <summary>Aligns each line, padding so all configured markers line up across rows.</summary>
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
///     One column definition for <see cref="StringAligner" />: locates the start and end of a marker
///     in a line and pads other lines so all markers line up.
/// </summary>
[PublicAPI]
public abstract class FloatingColumn : Dumpable
{
    /// <summary>
    ///     Fallback used when the marker is not found in the current line. By default returns the
    ///     line length so that the column "ends" at end-of-line; <see cref="StringAligner" /> wires
    ///     this to delegate to the next column when there is one.
    /// </summary>
    public Func<string, int, int> FindStartFailed;

    /// <summary>Constructs a column with the default end-of-line fallback.</summary>
    protected FloatingColumn() => FindStartFailed = (s, i) => s.Length;

    /// <summary>Returns the column-marker start position in <paramref name="s" /> at or after <paramref name="i" />.</summary>
    /// <param name="s"> Line to search. </param>
    /// <param name="i"> Position to start searching from. </param>
    public abstract int FindStart(string s, int i);

    /// <summary>Returns the position just past the column marker.</summary>
    /// <param name="s"> Line to search. </param>
    /// <param name="i"> Position to start searching from. </param>
    public abstract int FindEnd(string s, int i);

    /// <summary>
    ///     Pads <paramref name="lines" /> in place so that this column's marker lines up across rows.
    ///     <paramref name="positions" /> is updated to point past the handled portion of each line.
    /// </summary>
    /// <param name="lines"> Lines to align (modified in place). </param>
    /// <param name="positions"> Per-line cursor positions (modified in place). </param>
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