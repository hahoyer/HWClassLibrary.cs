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

        public string Name { get { return Source.SubString(Position, Length); } }

        [DisableDump]
        public string FilePosition { get { return "\n" + Source.FilePosn(Position, Name); } }

        public string FileErrorPosition(string errorTag) { return "\n" + Source.FilePosn(Position, Name, "error " + errorTag); }

        [UsedImplicitly]
        string DumpCurrent { get { return Name; } }

        const int DumpWidth = 10;

        string GetDumpAfterCurrent(int dumpWidth)
        {
            if(Source.IsEnd(Position + Length))
                return "";
            var length = Math.Min(dumpWidth, Source.Length - Position - Length);
            var result = Source.SubString(Position + Length, length);
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

        public SourcePart Combine(SourcePart other)
        {
            Tracer.Assert(Source == other.Source);
            Tracer.Assert(Position + Length <= other.Position);
            return new SourcePart(Source, Position, other.Position + other.Length - Position);
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
    }
}