using System;
using System.Diagnostics;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using SourcePosn = hw.Scanner.SourcePosition;

// ReSharper disable CheckNamespace

namespace hw.Scanner;

/// <summary>
///     Source and position for compilation process
/// </summary>
[DebuggerDisplay("{" + nameof(NodeDump) + "}")]
[PublicAPI]
public sealed class SourcePosition : Dumpable, IEquatable<SourcePosition>
{
    public readonly Source Source;
    public int Position;

    /// <summary>
    ///     ctor from source and position
    /// </summary>
    /// <param name="source"></param>
    /// <param name="position"></param>
    public SourcePosition(Source source, int position)
    {
        Position = position;
        Source = source;
    }

    public bool Equals(SourcePosition other)
        => !(other is null) && Equals(Source, other.Source) && Position == other.Position;

    public override bool Equals(object target)
    {
        if(ReferenceEquals(null, target))
            return false;
        if(ReferenceEquals(this, target))
            return true;
        return target is SourcePosition position && Equals(position);
    }

    public override int GetHashCode() => 0;

    /// <summary>
    ///     Default dump behaviour
    /// </summary>
    /// <returns>The file position of source file</returns>
    protected override string Dump(bool isRecursion)
    {
        if(Source.IsPersistent)
            return "\n" + FilePosition("see there");
        return GetDumpAroundCurrent(Source.DumpWidth);
    }

    public bool IsValid
    {
        get => 0 <= Position && Position <= Source.Length;
        set => Position = value? 0 : -1;
    }

    /// <summary>
    ///     The current character
    /// </summary>
    public char Current => Source[Position];

    /// <summary>
    ///     Natural indexer
    /// </summary>
    public char this[int index] => Source[Position + index];

    /// <summary>
    ///     Checks if at or beyond end of source
    /// </summary>
    /// <value> </value>
    public bool IsEnd => Source.IsEnd(Position);

    [UsedImplicitly]
    string NodeDump => GetDumpAroundCurrent(Source.NodeDumpWidth);

    public SourcePosition Clone => new(Source, Position);

    public TextPosition TextPosition => Source.GetTextPosition(Position);

    public int LineIndex => Source.LineIndex(Position);
    public int ColumnIndex => Source.ColumnIndex(Position);

    public static SourcePosition operator +(SourcePosition target, int y)
        => target.Source + (target.Position + y);

    public static SourcePosition operator -(SourcePosition target, int y)
        => target.Source + (target.Position - y);

    public static int operator -(SourcePosition target, SourcePosition y)
    {
        (target.Source == y.Source).Assert();
        return target.Position - y.Position;
    }

    public static bool operator <(SourcePosition left, SourcePosition right)
        => left != null &&
            right != null &&
            left.Source == right.Source &&
            left.Position < right.Position;

    public static bool operator <=(SourcePosition left, SourcePosition right) => left < right || left == right;

    public static bool operator >(SourcePosition left, SourcePosition right) => right < left;
    public static bool operator >=(SourcePosition left, SourcePosition right) => right <= left;
    public static bool operator !=(SourcePosition left, SourcePosition right) => !(left == right);

    public static bool operator ==(SourcePosition left, SourcePosition right)
    {
        if((object)left == null)
            return (object)right == null;
        if((object)right == null)
            return false;
        return left.Source == right.Source &&
            left.Position == right.Position;
    }

    public bool Equals(SourcePosition target, SourcePosition y) => target.Equals(y);

    /// <summary>
    ///     Obtains a piece
    /// </summary>
    /// <param name="start">start position</param>
    /// <param name="length">number of characters</param>
    /// <returns></returns>
    public string SubString(int start, int length) => Source.SubString(Position + start, length);

    /// <summary>
    ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
    /// </summary>
    /// <param name="flagText">the flag text</param>
    /// <returns>the "FileName(lineNumber,ColNr): tag: " string</returns>
    public string FilePosition(string flagText) => Source.FilePosition(Position, Position, flagText);

    public string GetDumpAroundCurrent(int dumpWidth)
    {
        if(IsValid)
            return
                Source.GetDumpBeforeCurrent(Position, dumpWidth) +
                "[]" +
                Source.GetDumpAfterCurrent(Position, dumpWidth);
        return "<invalid>";
    }

    public int? Match(IMatch automaton, bool isForward = true)
    {
        var span = Span(Source + (isForward? Source.Length : 0));
        return automaton.Match(span, isForward);
    }

    public bool StartsWith(string data, StringComparison type = StringComparison.InvariantCulture)
    {
        var length = data.Length;
        return !Source.IsEnd(Position + length - 1) && Source.SubString(Position, length).Equals(data, type);
    }

    public SourcePart Span(SourcePosition other) => SourcePart.Span(new(Source, Position), other);
    public SourcePart Span(int length) => SourcePart.Span(new(Source, Position), length);
}