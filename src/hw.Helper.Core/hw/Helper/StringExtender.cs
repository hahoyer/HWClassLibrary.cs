using System.Diagnostics;
using System.Text.RegularExpressions;
using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>Extension methods for string formatting, quoting, slicing and path manipulation.</summary>
[PublicAPI]
public static class StringExtender
{
    /// <summary>
    ///     Inserts <paramref name="tabString" /> repeated <paramref name="count" /> times after every
    ///     newline in <paramref name="target" />, optionally prefixing the first line as well.
    /// </summary>
    /// <param name="target"> Text to indent. </param>
    /// <param name="count"> How many times <paramref name="tabString" /> is applied per line. </param>
    /// <param name="tabString"> The indent unit; defaults to four spaces. </param>
    /// <param name="isLineStart"> If <c>true</c>, the first line is also indented. </param>
    public static string Indent
        (this string target, int count = 1, string tabString = "    ", bool isLineStart = false)
    {
        var effectiveTabString = tabString.Repeat(count);
        return (isLineStart? effectiveTabString : "") + target.Replace("\n", "\n" + effectiveTabString);
    }

    /// <summary>Returns <paramref name="s" /> concatenated with itself <paramref name="count" /> times.</summary>
    public static string Repeat(this string s, int count)
    {
        var result = "";
        for(var i = 0; i < count; i++)
            result += s;
        return result;
    }

    /// <summary>
    ///     Wraps <paramref name="data" /> in <paramref name="left" /> and <paramref name="right" />
    ///     brackets. When <paramref name="right" /> is omitted, it is inferred from
    ///     <paramref name="left" /> via the bracket pairs <c>&lt;()&#123;[</c> →
    ///     <c>&gt;)&#125;]</c>. Multi-line content is wrapped onto its own line and indented.
    /// </summary>
    public static string Surround(this string data, string left, string? right = null)
    {
        if(right == null)
        {
            var value = left.Single().ToString();
            var index = "<({[".IndexOf(value, StringComparison.Ordinal);
            right = ">)}]"[index].ToString();
        }

        if(data.IndexOf("\n", StringComparison.Ordinal) < 0)
            return left + data + right;
        return "\n" + left + ("\n" + data).Indent() + "\n" + right;
    }

    /// <summary>Concatenates the non-empty entries of <paramref name="data" /> with <paramref name="delimiter" />, dropping empty/null fragments so duplicate delimiters do not appear.</summary>
    public static string SaveConcat(this string delimiter, params string[] data)
        => data.Where(d => !string.IsNullOrEmpty(d)).Stringify(delimiter);

    /// <summary>
    ///     Wraps <paramref name="target" /> in double quotes, escaping backslashes and embedded
    ///     quotes. Returns the literal <c>"null"</c> for a <c>null</c> input.
    /// </summary>
    public static string Quote(this string? target)
    {
        if(target == null)
            return "null";
        return "\"" + target.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    static string CharacterQuote(char character)
    {
        switch(character)
        {
            case '\\':
            case '"':
                return "\\" + character;
            case '\n':
                return "\\n";
            case '\t':
                return "\\t";
            case '\r':
                return "\\r";
            case '\f':
                return "\\f";
            default:
                if(character < 32 || character >= 127)
                    return $"\\0x{(int)character:x2}";
                return "" + character;
        }
    }

    /// <summary>
    ///     Wraps <paramref name="target" /> in double quotes and escapes every character per C#
    ///     literal rules (control codes, non-ASCII bytes and the special <c>\</c>/<c>"</c>).
    /// </summary>
    public static string CSharpQuote(this string target)
        => target.Aggregate("\"", (head, next) => head + CharacterQuote(next)) + "\"";

    /// <summary>
    ///     Two-hex-digits-per-byte rendering of <paramref name="bytes" /> grouped in spaces of four
    ///     and surrounded by <c>target[…]</c> markers.
    /// </summary>
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

    /// <summary>
    ///     Runs <paramref name="command" /> through <c>cmd /c</c> and captures its standard output.
    ///     Standard error is not redirected.
    /// </summary>
    public static string ExecuteCommand(this string command)
    {
        var procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
            { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
        var proc = new Process { StartInfo = procStartInfo };
        proc.Start();
        return proc.StandardOutput.ReadToEnd();
    }

    /// <summary>Wraps <paramref name="name" /> in an <see cref="SmbFile" />.</summary>
    /// <param name="name"> The path. </param>
    /// <param name="autoCreateDirectories"> See <see cref="SmbFile.AutoCreateDirectories" />. </param>
    public static SmbFile ToSmbFile
        (this string name, bool autoCreateDirectories = true) => SmbFile.Create(name, autoCreateDirectories);

    /// <summary>Variadic overload of <see cref="Path.Combine(string, string)" />.</summary>
    public static string PathCombine
        (this string head, params string[] tail) => Path.Combine(head, Path.Combine(tail));

    /// <summary>Converts <c>SOME_NAME</c>-style identifiers to <c>SomeName</c>.</summary>
    public static string UnderScoreToCamelCase
        (this string name) => name.Split('_').Select(ToLowerFirstUpper).Stringify();

    /// <summary>Capitalises the first character and lower-cases the rest. Empty/single-character input is returned unchanged in spirit (calls throw on empty).</summary>
    public static string ToLowerFirstUpper
        (this string text) => text.Substring(0, 1).ToUpperInvariant() + text.Substring(1).ToLowerInvariant();

    /// <summary>Heuristic for converting <c>SOME_THINGS</c> SQL identifiers to <c>SomeThing</c>-style class names (camel-cased then naively singularised).</summary>
    public static string TableNameToClassName(this string name) => name.UnderScoreToCamelCase().ToSingular();

    /// <summary>Extension wrapper around <see cref="string.Format(string, object?[])" />.</summary>
    [StringFormatMethod("pattern")]
    public static string ReplaceArgs(this string pattern, params object[] args) => string.Format(pattern, args);

    /// <summary>Whether <paramref name="input" /> contains a substring matching the regular expression <paramref name="pattern" />.</summary>
    public static bool Matches(this string input, string pattern) => new Regex(pattern).IsMatch(input);

    /// <summary>
    ///     Splits <paramref name="target" /> into chunks of the given <paramref name="sizes" />,
    ///     yielding a final chunk with the remainder. Sizes are clamped to what is left, so over-long
    ///     requests do not throw.
    /// </summary>
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

    /// <summary>Pipe-style alias for <see cref="StringAligner.Format(string)" />.</summary>
    public static string Format(this string target, StringAligner aligner) => aligner.Format(target);

    /// <summary>
    ///     Builds a <see cref="StringAligner" /> with <paramref name="columnCount" /> columns each
    ///     anchored at a two-space marker — a quick recipe for tabular text.
    /// </summary>
    public static StringAligner StringAligner(this int columnCount)
    {
        var stringAligner = new StringAligner();
        for(var i = 0; i < columnCount; i++)
            stringAligner.AddFloatingColumn("  ");
        return stringAligner;
    }

    internal static int BeginMatch(string a, string b)
    {
        for(var i = 0;; i++)
            if(i >= a.Length || i >= b.Length || a[i] != b[i])
                return i;
    }

    static string HexDumpFiller(int i, int length)
    {
        (length < 16).Assert();
        if(0 == length)
            return "target[]";
        if(i == 0)
            return "target[";
        if(i == length)
            return "]";
        if(i % 4 == 0)
            return " ";
        return "";
    }
}