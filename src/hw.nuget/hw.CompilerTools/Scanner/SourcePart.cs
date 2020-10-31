using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Scanner
{
    [DebuggerDisplay("{" + nameof(NodeDump) + "}")]
    [PublicAPI]
    public sealed class SourcePart
        : Dumpable
            , IAggregateable<SourcePart>
    {
        [UsedImplicitly]
        const int DumpWidth = 10;

        [DisableDump]
        public int Length { get; }

        [DisableDump]
        public int Position { get; }

        [DisableDump]
        public Source Source { get; }

        SourcePart(Source source, int position, int length)
        {
            Source = source;
            Length = length;
            Position = position;
        }

        [DisableDump]
        public int EndPosition => Position + Length;

        public string Id => Source.SubString(Position, Length);

        [DisableDump]
        public string FilePosition => "\n" + Source.FilePosition(Position, EndPosition, Id);

        [UsedImplicitly]
        public string NodeDump => GetDumpAroundCurrent(Source.NodeDumpWidth);

        [DisableDump]
        public SourcePosition Start => Source + Position;

        [DisableDump]
        public SourcePosition End => Source + EndPosition;

        [UsedImplicitly]
        string DumpCurrent => Id;

        SourcePart IAggregateable<SourcePart>.Aggregate(SourcePart other) => Overlay(other);

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
            if(Source != other.Source)
                return null;

            var start = Math.Max(Position, other.Position);
            var end = Math.Min(EndPosition, other.EndPosition);
            return end < start? null : new SourcePart(Source, start, end - start);
        }

        public static SourcePart operator +(SourcePart left, SourcePart right)
            => left == null
                ? right
                : right == null
                    ? left
                    : left.Overlay(right);

        public string FileErrorPosition(string errorTag)
            => "\n" + Source.FilePosition(Position, EndPosition, Id.Quote(), "error " + errorTag);

        public string GetDumpAroundCurrent(int dumpWidth)
            => Source.GetDumpBeforeCurrent(Position, dumpWidth) +
               "[" +
               DumpCurrent +
               "]" +
               Source.GetDumpAfterCurrent(EndPosition, dumpWidth);

        public SourcePart Combine(SourcePart other)
        {
            (Source == other.Source).Assert();
            (EndPosition <= other.Position).Assert();
            return new SourcePart(Source, Position, other.EndPosition - Position);
        }

        public static SourcePart Span(SourcePosition first, SourcePosition other)
        {
            var length = other - first;
            return new SourcePart(first.Source, first.Position, length);
        }

        public static SourcePart Span(SourcePosition first, int length)
            => new SourcePart(first.Source, first.Position, length);

        public bool Contains(SourcePosition sourcePosition)
            => Source == sourcePosition.Source &&
               Position <= sourcePosition.Position &&
               EndPosition > sourcePosition.Position;

        public bool Contains(SourcePart sourcePart)
            => Source == sourcePart.Source &&
               Position <= sourcePart.Position &&
               sourcePart.EndPosition <= EndPosition;

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

        public static bool operator !=(SourcePart left, SourcePart right) => !(left == right);

        public static bool operator >(SourcePart left, SourcePosition right) => right < left;
        public static bool operator >(SourcePosition left, SourcePart right) => right < left;
        public static bool operator >(SourcePart left, SourcePart right) => right < left;

        public static bool operator <(SourcePart left, SourcePosition right)
            => left != null && left.End < right;

        public static bool operator <(SourcePosition left, SourcePart right)
            => right != null && left < right.Start;

        public static bool operator <(SourcePart left, SourcePart right)
            => left != null && right != null && left.End <= right.Start;

        public static bool operator ==(SourcePart left, SourcePart right)
        {
            if((object)left == null)
                return (object)right == null;
            if((object)right == null)
                return false;

            return left.Start == right.Start &&
                   left.Length == right.Length;
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
    }
}