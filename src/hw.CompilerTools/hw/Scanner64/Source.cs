using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Scanner64;

[PublicAPI]
public sealed class Source : DumpableObject
{
    public const int NodeDumpWidth = 10;
    public const int DumpWidth = 20;
    static int NextObjectId;
    public readonly string? Identifier;
    public readonly ISourceProvider SourceProvider;

    public ITextProvider Data => SourceProvider.Data;

    public char this[Index64 position] => IsEndPosition(position)? '\0' : Data[position];

    public string this[Range64 range]
        => range.Start.GetOffset(Length) < range.End.GetOffset(Length)
            ? Data[range]
            : Data[range.End / range.Start];

    public long Length => SourceProvider.Length;
    public bool IsPersistent => SourceProvider.IsPersistent;
    public SourcePart All => (this + 0).Span(Length);

    public Source(ISourceProvider sourceProvider, string? identifier = null)
        : base(NextObjectId++)
    {
        SourceProvider = sourceProvider;
        Identifier = identifier;
    }

    public Source(SmbFile file, string? identifier = null)
        : this(new FileSourceProvider(file), identifier ?? file.FullName) { }

    public Source(string data, string? identifier = null)
        : this(new StringSourceProvider(data), identifier ?? "????") { }

    protected override string Dump(bool isRecursion) => GetFilePositions(0, Length, "see there");

    public static SourcePosition operator +(Source target, long y) => new(target, y);
    public static SourcePosition operator +(Source target, Index64 y) => new(target, y);

    public bool IsValidPosition(Index64 position)
        => position.GetOffset(Length) < Length;

    public bool IsEndPosition(Index64 position)
        => position.GetOffset(Length) >= Length;

    public TextPosition GetTextPosition(long position)
        => new() { LineNumber = checked((int)LineIndex(position)), ColumnNumber = ColumnIndex(position) };

    public string GetIdentifier(int start) => "";

    SourcePart[] GetProviderSplits(long position, long positionEnd)
    {
        if(SourceProvider is not IMultiSourceProvider target)
            return [(this + position).Span(this + positionEnd)];

        var resultStart = target.Position(position, false);
        var resultEnd = target.Position(positionEnd, true);
        if(resultStart.Source == resultEnd.Source)
            return [resultStart.Span(resultEnd)];
        NotImplementedMethod(position, positionEnd);
        return default!;
    }

    public string GetFilePosition(long position, long positionEnd, string flagText, string? tag = null)
        => Tracer.FilePosition
            (
                Identifier,
                new()
                {
                    Start = GetTextPosition(position), End = GetTextPosition(positionEnd)
                },
                tag ?? nameof(FilePositionTag.Debug))
            + flagText;

    public string GetFilePositions(long position, long positionEnd, string flagText, string? tag = null)
    {
        var splits = GetProviderSplits(position, positionEnd);
        return splits
            .Select(s => s.Source.GetFilePosition(s.Position, s.EndPosition, flagText, tag))
            .Stringify("\n");
    }

    public long LineIndex(long position) => Data.Take(checked((int)position)).LongCount(c => c == '\n');

    public int ColumnIndex(long position)
        => Data
            .Take(checked((int)position))
            .Aggregate(0, (current, c) => c.In('\r', '\n')? 0 : current + 1);

    public SourcePosition FromLineAndColumn(int lineIndex, int? columnIndex)
        => this + PositionFromLineAndColumn(lineIndex, columnIndex);

    public long LineLength(int lineIndex)
        => FromLineAndColumn(lineIndex + 1, 0) - FromLineAndColumn(lineIndex, 0);

    public SourcePart Line(int lineIndex)
        => FromLineAndColumn(lineIndex, 0).Span(FromLineAndColumn(lineIndex, null));

    long PositionFromLineAndColumn(int lineIndex, int? columnIndex)
    {
        var match = "\n".AnyChar().Find.Repeat(lineIndex, lineIndex);
        var l = (this + 0).Match(match);
        if(l == null)
            return Length;

        var nextLine = (this + l.Value).Match(Match.LineEnd.Not.Repeat());
        if(nextLine != null)
        {
            var effectiveColumnIndex = nextLine.Value;
            if(columnIndex < effectiveColumnIndex)
                effectiveColumnIndex = columnIndex.Value;
            return l.Value + effectiveColumnIndex;
        }

        if(columnIndex != null && l.Value + columnIndex.Value < Length)
            return l.Value + columnIndex.Value;

        return Length;
    }

    public string GetDumpBeforeCurrent(long position, int dumpWidth)
    {
        if(position < dumpWidth + 3)
            return this[new Range64(0,position)];

        return "..." + this[new Index64(position - dumpWidth) / position];
    }

    public string GetDumpAfterCurrent(long position, int dumpWidth)
    {
        if(IsEndPosition(position))
            return "";

        var index = new Index64(position);
        if(dumpWidth + 3 < Length - position)
        {
            return this[index/(position + dumpWidth)] + "...";
        }

        return this[new Range64(index)];
    }
}