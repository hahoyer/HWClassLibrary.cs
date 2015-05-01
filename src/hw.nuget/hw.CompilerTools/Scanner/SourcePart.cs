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
            var end = Math.Max(EndPosition, other.EndPosition);
            return new SourcePart(Source, start, end - start);
        }

        public SourcePart Intersect(SourcePart other)
        {
            Tracer.Assert(Source == other.Source);
            var start = Math.Max(Position, other.Position);
            var end = Math.Min(EndPosition, other.EndPosition);
            var length = Math.Max(0, end - start);
            return new SourcePart(Source, start, length);
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

        public bool Contains(SourcePart sourcePart)
        {
            return Source == sourcePart.Source &&
                Position <= sourcePart.Position &&
                sourcePart.EndPosition <= EndPosition;
        }

        public bool IsMatch(SourcePart sourcePosn)
        {
            if(Source != sourcePosn.Source)
                return false;
            if(EndPosition == sourcePosn.Position)
                return true;
            return Position == sourcePosn.EndPosition;
        }

        public static IEnumerable<SourcePart> SaveCombine(IEnumerable<SourcePart> values)
        {
            return values
                .GroupBy(item => item.Source)
                .SelectMany(SaveCombineForSource);
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
                if(currentValue.EndPosition == value.Position)
                    currentValue = currentValue.Combine(value);
                else
                {
                    yield return currentValue;
                    currentValue = value;
                }
            yield return currentValue;
        }

        SourcePart ISourcePart.All { get { return this; } }

        public static bool operator !=(SourcePart left, SourcePart right)
        {
            return !(left == right);
        }

        public static bool operator >(SourcePart left, SourcePosn right) { return right < left; }
        public static bool operator >(SourcePosn left, SourcePart right) { return right < left; }
        public static bool operator >(SourcePart left, SourcePart right) { return right < left; }

        public static bool operator <(SourcePart left, SourcePosn right)
        {
            return left != null && left.End < right;
        }

        public static bool operator <(SourcePosn left, SourcePart right)
        {
            return right != null && left < right.Start;
        }

        public static bool operator <(SourcePart left, SourcePart right)
        {
            return left != null && right != null && left.End <= right.Start;
        }

        public static bool operator ==(SourcePart left, SourcePart right)
        {
            if((object) left == null)
                return ((object) right == null);
            if((object) right == null)
                return false;
            return left.Start == right.Start &&
                left.Length == right.Length;
        }
    }
}