
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.PrioParser;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    [DebuggerDisplay("{NodeDump}")]
    sealed class TokenData : Dumpable, IPart, IOperatorPart
    {
        readonly int _length;
        readonly Source _source;
        readonly int _position;

        TokenData(Source source, int position, int length)
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

        public TokenData Combine(TokenData other)
        {
            Tracer.Assert(Source == other.Source);
            Tracer.Assert(Position + Length <= other.Position);
            return new TokenData(Source, Position, other.Position + other.Length - Position);
        }

        internal static TokenData Span(SourcePosn first, IPosition<IParsedSyntax> other)
        {
            var length = ((Position) other).SourcePosn - first;
            return new TokenData(first.Source, first.Position, length);
            throw new NotImplementedException();
        }
        internal static TokenData Span(SourcePosn first, int length)
        {
            return new TokenData(first.Source, first.Position, length);
            throw new NotImplementedException();
        }
    }
}