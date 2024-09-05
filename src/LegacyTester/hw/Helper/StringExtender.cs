using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     String helper functions.
/// </summary>
[PublicAPI]
public static class StringExtender
{
    /// <summary>
    ///     Indent parameter by 4 times count spaces
    /// </summary>
    /// <param name="target"> The target. </param>
    /// <param name="tabString"></param>
    /// <param name="count"> The count. </param>
    /// <param name="isLineStart"></param>
    /// <returns> </returns>
    public static string Indent
        (this string target, int count = 1, string tabString = "    ", bool isLineStart = false)
    {
        var effectiveTabString = tabString.Repeat(count);
        return (isLineStart? effectiveTabString : "") + target.Replace("\n", "\n" + effectiveTabString);
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
    public static string Surround(this string data, string left, string right = null)
    {
        if(right == null)
        {
            var value = left.Single().ToString();
            var index = "<({[".IndexOf(value, StringComparison.Ordinal);
            right = ">)}]"[index].ToString();
        }

        if(data.IndexOf("\n", StringComparison.Ordinal) < 0)
            return left + data + right;
        return "\n" + left + Indent("\n" + data) + "\n" + right;
    }

    public static string SaveConcat(this string delimiter, params string[] data)
        => data.Where(d => !string.IsNullOrEmpty(d)).Stringify(delimiter);

    /// <summary>
    ///     Converts string to a string literal.
    /// </summary>
    /// <param name="target"> The target. </param>
    /// <returns> </returns>
    /// created 08.01.2007 18:37
    public static string Quote(this string target)
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
    ///     Converts string to a string literal suitable for languages like c#.
    /// </summary>
    /// <param name="target"> The target. </param>
    /// <returns> </returns>
    public static string CSharpQuote(this string target)
        => target.Aggregate("\"", (head, next) => head + CharacterQuote(next)) + "\"";

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

    public static string ExecuteCommand(this string command)
    {
        var procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
            { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
        var proc = new Process { StartInfo = procStartInfo };
        proc.Start();
        return proc.StandardOutput.ReadToEnd();
    }

    public static SmbFile ToSmbFile
        (this string name, bool autoCreateDirectories = true) => SmbFile.Create(name, autoCreateDirectories);

    public static string PathCombine
        (this string head, params string[] tail) => Path.Combine(head, Path.Combine(tail));

    public static string UnderScoreToCamelCase
        (this string name) => name.Split('_').Select(ToLowerFirstUpper).Stringify("");

    public static string ToLowerFirstUpper
        (this string text) => text.Substring(0, 1).ToUpperInvariant() + text.Substring(1).ToLowerInvariant();

    public static string TableNameToClassName(this string name) => name.UnderScoreToCamelCase().ToSingular();

    [StringFormatMethod("pattern")]
    public static string ReplaceArgs(this string pattern, params object[] args) => string.Format(pattern, args);

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

    public static string Format(this string target, StringAligner aligner) => aligner.Format(target);

    /// <summary>
    ///     Provide default string aligner with columnCount columns
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