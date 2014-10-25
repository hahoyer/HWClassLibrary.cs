using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using JetBrains.Annotations;

namespace hw.Scanner
{
    [DebuggerDisplay("{NodeDump}")]
    public sealed class SourcePart : Dumpable
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
        Source Source { get { return _source; } }

        [DisableDump]
        int Position { get { return _position; } }

        [DisableDump]
        int Length { get { return _length; } }

        internal string Name { get { return Source.SubString(Position, Length); } }

        [DisableDump]
        internal string FilePosition { get { return "\n" + Source.FilePosn(Position, Name); } }

        internal string FileErrorPosition(string errorTag) { return "\n" + Source.FilePosn(Position, Name, "error " + errorTag); }

        [UsedImplicitly]
        string DumpCurrent { get { return Name; } }

        const int DumpWidth = 10;

        [UsedImplicitly]
        string DumpAfterCurrent
        {
            get
            {
                if(Source.IsEnd(Position + Length))
                    return "";
                var length = Math.Min(DumpWidth, Source.Length - Position - Length);
                var result = Source.SubString(Position + Length, length);
                if(length == DumpWidth)
                    result += "...";
                return result;
            }
        }

        [UsedImplicitly]
        string DumpBeforeCurrent
        {
            get
            {
                var start = Math.Max(0, Position - DumpWidth);
                var result = Source.SubString(start, Position - start);
                if(Position >= DumpWidth)
                    result = "..." + result;
                return result;
            }
        }

        [UsedImplicitly]
        string NodeDump
        {
            get
            {
                return DumpBeforeCurrent
                    + "["
                    + DumpCurrent
                    + "]"
                    + DumpAfterCurrent;
            }
        }
        public SourcePosn Start { get { return Source+Position; } }

        public SourcePart Combine(SourcePart other)
        {
            Tracer.Assert(Source == other.Source);
            Tracer.Assert(Position + Length <= other.Position);
            return new SourcePart(Source, Position, other.Position + other.Length - Position);
        }

        internal static SourcePart Span(SourcePosn first, SourcePosn other)
        {
            var length = other - first;
            return new SourcePart(first.Source, first.Position, length);
        }
        internal static SourcePart Span(SourcePosn first, int length)
        {
            return new SourcePart(first.Source, first.Position, length);
        }
    }
}