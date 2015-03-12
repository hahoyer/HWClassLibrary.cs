using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.Scanner
{
    [DebuggerDisplay("{NodeDump}")]
    public sealed class SourcePart : Dumpable, IAggregateable<SourcePart>, ISourcePart
    {
        readonly int _length;
        readonly Source _source;
        readonly int _position;

        SourcePart(Source source, int position, int length)
        {
            _source = source;
            _length = length;
            _position = position;
        }

        [DisableDump]
        public Source Source { get { return _source; } }

        [DisableDump]
        public int Position { get { return _position; } }

        [DisableDump]
        public int Length { get { return _length; } }

        [DisableDump]
        public int EndPosition { get { return Position + Length; } }


        SourcePart IAggregateable<SourcePart>.Aggregate(SourcePart other) { return Overlay(other); }

        public SourcePart Overlay(SourcePart other)
        {
            if(Source != other.Source)
                return this;
            var start = Math.Min(Position, other.Position);
            var end = Math.Max(EndPosition, other.Position + other.Length);
            return new SourcePart(Source, start, end - start);
        }

        public static SourcePart operator +(SourcePart left, SourcePart right)
        {
            return left == null
                ? right
                : right == null
                    ? left
                    : left.Overlay(right);
        }

        public string Id { get { return Source.SubString(Position, Length); } }

        [DisableDump]
        public string FilePosition { get { return "\n" + Source.FilePosn(Position, Id); } }

        public string FileErrorPosition(string errorTag)
        {
            return "\n" + Source.FilePosn(Position, Id, "error " + errorTag);
        }

        [UsedImplicitly]
        string DumpCurrent { get { return Id; } }

        const int DumpWidth = 10;

        string GetDumpAfterCurrent(int dumpWidth)
        {
            if(Source.IsEnd(EndPosition))
                return "";
            var length = Math.Min(dumpWidth, Source.Length - EndPosition);
            var result = Source.SubString(EndPosition, length);
            if(length == dumpWidth)
                result += "...";
            return result;
        }

        string GetDumpBeforeCurrent(int dumpWidth)
        {
            var start = Math.Max(0, Position - dumpWidth);
            var result = Source.SubString(start, Position - start);
            if(Position >= dumpWidth)
                result = "..." + result;
            return result;
        }

        [UsedImplicitly]
        public string NodeDump { get { return GetDumpAroundCurrent(Source.NodeDumpWidth); } }

        public string GetDumpAroundCurrent(int dumpWidth)
        {
            return GetDumpBeforeCurrent(dumpWidth)
                + "["
                + DumpCurrent
                + "]"
                + GetDumpAfterCurrent(dumpWidth);
        }

        [DisableDump]
        public SourcePosn Start { get { return Source + Position; } }
        [DisableDump]
        public SourcePosn End { get { return Source + EndPosition; } }

        public SourcePart Combine(SourcePart other)
        {
            Tracer.Assert(Source == other.Source);
            Tracer.Assert(EndPosition <= other.Position);
            return new SourcePart(Source, Position, other.EndPosition - Position);
        }

        public static SourcePart Span(SourcePosn first, SourcePosn other)
        {
            var length = other - first;
            return new SourcePart(first.Source, first.Position, length);
        }

        public static SourcePart Span(SourcePosn first, int length)
        {
            return new SourcePart(first.Source, first.Position, length);
        }

        public bool Contains(SourcePosn sourcePosn)
        {
            return Source == sourcePosn.Source &&
                Position <= sourcePosn.Position &&
                EndPosition > sourcePosn.Position;
        }

        SourcePart ISourcePart.All { get { return this; } }
    }
}