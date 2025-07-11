using System.Diagnostics;
using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Scanner;

[DebuggerDisplay("{" + nameof(NodeDump) + "}")]
[PublicAPI]
public sealed class SourcePart
    : Dumpable
        , IAggregateable<SourcePart>
{
    [UsedImplicitly]
    const int DumpWidth = 10;

    [DisableDump]
    public readonly int Position;

    [DisableDump]
    public readonly int EndPosition;

    [DisableDump]
    public readonly Source Source;

    [DisableDump]
    public int Length => EndPosition - Position;

    public string Id => Source[Position..EndPosition];

    [DisableDump]
    public string FilePosition => "\n" + Source.GetFilePositions(Position, EndPosition, Id);

    [UsedImplicitly]
    public string NodeDump => GetDumpAroundCurrent().LogDump();

    [DisableDump]
    public SourcePosition Start => Source + Position;

    [DisableDump]
    public SourcePosition End => Source + EndPosition;

    [UsedImplicitly]
    string DumpCurrent => Id;

    [DisableDump]
    public(TextPosition start, TextPosition end) TextPosition
        => (Source.GetTextPosition(Position), Source.GetTextPosition(EndPosition));

    SourcePart(Source source, Range range)
    {
        Source = source;
        Position = range.Start.GetOffset(Source.Length);
        EndPosition = range.End.GetOffset(Source.Length);
        if(Length < 0)
            (EndPosition, Position) = (Position, EndPosition);
    }

    SourcePart IAggregateable<SourcePart>.Aggregate(SourcePart? other) => Overlay(other);

    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj) || obj is SourcePart other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Length;
            hashCode = hashCode * 397 ^ Position;
            hashCode = hashCode * 397 ^ Source.GetHashCode();
            return hashCode;
        }
    }

    public SourcePart Overlay(SourcePart? other)
    {
        if(other == null || Source != other.Source)
            return this;

        var start = Math.Min(Position, other.Position);
        var end = Math.Max(EndPosition, other.EndPosition);
        return new(Source, start..end);
    }

    public SourcePart? Intersect(SourcePart other)
    {
        if(Source != other.Source)
            return null;

        var start = Math.Max(Position, other.Position);
        var end = Math.Min(EndPosition, other.EndPosition);
        return end < start? null : new SourcePart(Source, start..end);
    }

    public static SourcePart? operator +(SourcePart? left, SourcePart? right)
        => left == null? right :
            right == null? left : left.Overlay(right);

    public string FileErrorPosition(string errorTag)
        => "\n" + Source.GetFilePositions(Position, EndPosition, Id.Quote(), "error " + errorTag);

    public string GetDumpAroundCurrent(int dumpWidth = Source.NodeDumpWidth)
        => Source.GetDumpBeforeCurrent(Position, dumpWidth)
            + "["
            + DumpCurrent
            + "]"
            + Source.GetDumpAfterCurrent(EndPosition, dumpWidth);

    public SourcePart Combine(SourcePart other)
    {
        (Source == other.Source).Assert();
        (EndPosition <= other.Position).Assert();
        return new(Source, Position .. other.EndPosition);
    }

    public static SourcePart Span(SourcePosition first, SourcePosition other)
    {
        (first.Source == other.Source).Assert();
        var length = other - first;
        return new(first.Source, first.Position..other.Position);
    }

    public static SourcePart Span(SourcePosition first, int length)
        => new(first.Source, first.Position..(first.Position + length));

    public bool Contains(SourcePosition sourcePosition)
        => Source == sourcePosition.Source
            && Position <= sourcePosition.Position
            && EndPosition > sourcePosition.Position;

    public bool Contains(SourcePart sourcePart)
        => Source == sourcePart.Source && Position <= sourcePart.Position && sourcePart.EndPosition <= EndPosition;

    public bool IsMatch(SourcePart sourcePosition)
    {
        if(Source != sourcePosition.Source)
            return false;
        if(EndPosition == sourcePosition.Position)
            return true;

        return Position == sourcePosition.EndPosition;
    }

    public static IEnumerable<SourcePart> SaveCombine(IEnumerable<SourcePart> values)
        => values
            .GroupBy(item => item.Source)
            .SelectMany(SaveCombineForSource);

    public static bool operator !=(SourcePart? left, SourcePart? right) => !(left == right);

    public static bool operator >(SourcePart? left, SourcePosition? right) => right < left;

    public static bool operator >(SourcePosition? left, SourcePart? right) => right < left;

    public static bool operator >(SourcePart? left, SourcePart? right) => right < left;

    public static bool operator <(SourcePart? left, SourcePosition? right) => left != null && left.End < right;

    public static bool operator <(SourcePosition? left, SourcePart? right) => right != null && left < right.Start;

    public static bool operator <(SourcePart? left, SourcePart? right)
        => left != null && right != null && left.End <= right.Start;

    public static bool operator ==(SourcePart? left, SourcePart? right)
    {
        if((object?)left == null)
            return (object?)right == null;
        if((object?)right == null)
            return false;

        return left.Start == right.Start && left.Length == right.Length;
    }

    static IEnumerable<SourcePart> SaveCombineForSource(IEnumerable<SourcePart> values)
    {
        var sortedValues = values
            .Where(item => item.Length > 0)
            .OrderBy(item => item.Position)
            .ToArray();

        if(!sortedValues.Any())
        {
            yield return values.First();
            yield break;
        }

        var currentValue = sortedValues[0];

        foreach(var value in sortedValues.Skip(1))
        {
            var end = currentValue.EndPosition;
            var start = value.Position;
            if(end == start)
                currentValue = currentValue.Combine(value);
            else
            {
                yield return currentValue;

                currentValue = value;
            }
        }

        yield return currentValue;
    }

    bool Equals(SourcePart other)
        => Length == other.Length
            && Position == other.Position
            && Equals(Source, other.Source);

    public SourcePosition GetStart(bool isForward) => isForward? Start : End;
    public SourcePosition GetEnd(bool isForward) => isForward? End : Start;
    public int? Match(IMatch automaton, bool isForward = true) => automaton.Match(this, isForward);

    public IEnumerable<SourcePart> Split(string delimiter)
    {
        var start = Start;
        for(var current = Start; current < End;)
            if(current.Span(delimiter.Length).Id == delimiter)
            {
                yield return start.Span(current);
                current += delimiter.Length;
                start = current;
            }
            else
                current += 1;

        yield return start.Span(End);
    }
}