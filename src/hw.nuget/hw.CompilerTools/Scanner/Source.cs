using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace hw.Scanner
{
    public sealed class Source : Dumpable
    {
        readonly string Id;
        readonly File File;
        public const int NodeDumpWidth = 10;
        public const int DumpWidth = 20;

        public Source(File file)
        {
            File = file;
            Data = File.String;
        }

        public string Data { get; }

        public Source(string data, string id = null)
        {
            Id = id;
            Data = data;
        }

        public char this[int index] => IsEnd(index) ? '\0' : Data[index];
        public bool IsEnd(int posn) => Length <= posn;
        public int Length => Data.Length;
        public bool IsPersistent => File != null;
        public string SubString(int start, int length) => Data.Substring(start, length);
        public SourcePart All => (this + 0).Span(Length);

        public string FilePosn(int position, string flagText, string tag = null)
        {
            if(File == null)
                return Id ?? "????";

            return Tracer.FilePosn
                (
                    File.FullName,
                    LineIndex(position),
                    ColumnIndex(position) + 1,
                    tag ?? FilePositionTag.Debug.ToString()) + flagText;
        }

        public int LineIndex(int position) { return Data.Take(position).Count(c => c == '\n'); }

        public int ColumnIndex(int position) 
            => Data
            .Take(position)
            .Aggregate(0, (current, c) => c == '\n' ? 0 : current + 1);

        public SourcePosn FromLineAndColumn(int lineIndex, int? columnIndex)
            => this + PositionFromLineAndColumn(lineIndex, columnIndex);

        int PositionFromLineAndColumn(int lineIndex, int? columnIndex)
        {
            var match = "\n".AnyChar().Find.Repeat(lineIndex, lineIndex);
            var l = (this + 0).Match(match);
            if(l == null)
                return Length;

            var nextLine = (this + l.Value).Match(Match.LineEnd.Find);
            if(nextLine != null)
            {
                var effectiveColumnIndex = nextLine.Value - 1;
                if(columnIndex != null && columnIndex.Value < effectiveColumnIndex)
                    effectiveColumnIndex = columnIndex.Value;
                return l.Value + effectiveColumnIndex;
            }

            if(columnIndex != null && l.Value + columnIndex.Value < Length)
                return l.Value + columnIndex.Value;
            return Length;
        }

        protected override string Dump(bool isRecursion) => FilePosn(0, "see there");

        public static SourcePosn operator +(Source x, int y) => new SourcePosn(x, y);

        public int LineLength(int lineIndex)
            => FromLineAndColumn(lineIndex + 1, 0) - FromLineAndColumn(lineIndex, 0);

        public SourcePart Line(int lineIndex)
            => FromLineAndColumn(lineIndex, 0).Span(FromLineAndColumn(lineIndex, null));
    }
}