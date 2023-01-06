using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Scanner;

[PublicAPI]
public sealed class Source : Dumpable
{
    public const int NodeDumpWidth = 10;
    public const int DumpWidth = 20;
    public readonly string Identifier;
    readonly ISourceProvider SourceProvider;

    public string Data => SourceProvider.Data;

    public char this[int index] => IsEnd(index)? '\0' : Data[index];
    public int Length => Data.Length;
    public bool IsPersistent => SourceProvider.IsPersistent;
    public SourcePart All => (this + 0).Span(Length);

    public Source(ISourceProvider sourceProvider, string identifier = null)
    {
        SourceProvider = sourceProvider;
        Identifier = identifier;
    }

    public Source(SmbFile file, string identifier = null)
        : this(new FileSourceProvider(file), identifier ?? file.FullName) { }

    public Source(string data, string identifier = null)
        : this(new StringSourceProvider(data), identifier ?? "????") { }

    protected override string Dump(bool isRecursion) => FilePosition(0, Length, "see there");

    public static SourcePosition operator +(Source target, int y) => new(target, y);
    public bool IsEnd(int position) => Length <= position;
    public string SubString(int start, int length) => Data.Substring(start, length);

    public TextPosition GetTextPosition(int position)
        => new() { LineNumber = LineIndex(position), ColumnNumber1 = ColumnIndex(position) + 1 };

    public string FilePosition(int position, int positionEnd, string flagText, string tag = null)
        => Tracer.FilePosition
            (
                Identifier,
                new()
                {
                    Start = GetTextPosition(position), End = GetTextPosition(positionEnd)
                },
                tag ?? FilePositionTag.Debug.ToString()) +
            flagText;

    public int LineIndex(int position) => Data.Take(position).Count(c => c == '\n');

    public int ColumnIndex(int position)
        => Data
            .Take(position)
            .Aggregate(0, (current, c) => c.In('\r', '\n')? 0 : current + 1);

    public SourcePosition FromLineAndColumn(int lineIndex, int? columnIndex)
        => this + PositionFromLineAndColumn(lineIndex, columnIndex);

    public int LineLength(int lineIndex)
        => FromLineAndColumn(lineIndex + 1, 0) - FromLineAndColumn(lineIndex, 0);

    public SourcePart Line(int lineIndex)
        => FromLineAndColumn(lineIndex, 0).Span(FromLineAndColumn(lineIndex, null));

    int PositionFromLineAndColumn(int lineIndex, int? columnIndex)
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

    public string GetDumpBeforeCurrent(int position, int dumpWidth)
    {
        if(position < dumpWidth + 3)
            return SubString(0, position);

        return "..." + SubString(position - dumpWidth, dumpWidth);
    }

    public string GetDumpAfterCurrent(int position, int dumpWidth)
    {
        if(IsEnd(position))
            return "";

        if(dumpWidth + 3 < Length - position)
            return SubString(position, dumpWidth) + "...";
        return SubString(position, Length - position);
    }
}