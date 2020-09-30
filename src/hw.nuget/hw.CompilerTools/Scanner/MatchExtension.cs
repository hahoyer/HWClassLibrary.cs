using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Scanner
{
    [PublicAPI]
    public static class MatchExtension
    {
        [PublicAPI]
        public interface IPositionExceptionFactory
        {
            Exception Create(SourcePosition sourcePosition);
        }

        sealed class ErrorMatch
            : Dumpable
                , IMatch
        {
            readonly Match.IError Error;
            public ErrorMatch(Match.IError error) => Error = error;

            int? IMatch.Match(SourcePosition sourcePosition)
            {
                if(Error is IPositionExceptionFactory positionFactory)
                    throw positionFactory.Create(sourcePosition);

                throw new Match.Exception(sourcePosition, Error);
            }
        }

        sealed class CharMatch
            : Dumpable
                , IMatch
        {
            [EnableDump]
            readonly string Data;

            readonly StringComparison Type;

            public CharMatch(string data, bool isCaseSenitive)
            {
                Data = data;
                Type = isCaseSenitive? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            }

            int? IMatch.Match(SourcePosition sourcePosition)
            {
                var result = Data.Length;
                return sourcePosition.StartsWith(Data, Type)? (int?)result : null;
            }
        }

        sealed class AnyCharMatch
            : Dumpable
                , IMatch
        {
            sealed class DefaultComparer : IEqualityComparer<char>
            {
                bool IEqualityComparer<char>.Equals(char target, char y) => target == y;
                int IEqualityComparer<char>.GetHashCode(char obj) => obj;
            }

            sealed class UpperInvariantComparer : IEqualityComparer<char>
            {
                bool IEqualityComparer<char>.Equals(char target, char y)
                    => char.ToUpperInvariant(target) == char.ToUpperInvariant(y);

                int IEqualityComparer<char>.GetHashCode(char obj) => char.ToUpperInvariant(obj);
            }

            [EnableDump]
            readonly string Data;

            readonly IEqualityComparer<char> Comparer;

            public AnyCharMatch(string data, bool isCaseSenitive = true)
            {
                Data = data;
                Comparer = isCaseSenitive? Default : UpperInvariant;
            }

            static IEqualityComparer<char> Default => new DefaultComparer();
            static IEqualityComparer<char> UpperInvariant => new UpperInvariantComparer();

            int? IMatch.Match(SourcePosition sourcePosition)
                => Data.Contains(sourcePosition.Current, Comparer)? (int?)1 : null;
        }

        sealed class ElseMatch
            : Dumpable
                , IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            [EnableDump]
            readonly IMatch Other;

            public ElseMatch(IMatch data, IMatch other)
            {
                Data = data;
                Other = other;
            }

            int? IMatch.Match(SourcePosition sourcePosition)
                => Data.Match(sourcePosition) ?? Other.Match(sourcePosition);
        }

        sealed class Repeater
            : Dumpable
                , IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            [EnableDump]
            readonly int? MaxCount;

            [EnableDump]
            readonly int MinCount;

            public Repeater(IMatch data, int minCount, int? maxCount)
            {
                Tracer.Assert(!(data is Match));
                Tracer.Assert(minCount >= 0);
                Tracer.Assert(maxCount == null || maxCount >= minCount);
                Data = data;
                MinCount = minCount;
                MaxCount = maxCount;
            }

            int? IMatch.Match(SourcePosition sourcePosition)
            {
                var count = 0;
                var current = sourcePosition;
                while(true)
                {
                    var result = current - sourcePosition;
                    if(count == MaxCount)
                        return result;

                    var length = Data.Match(current);
                    if(length == null)
                        return count < MinCount? null : (int?)result;
                    if(current.IsEnd)
                        return null;

                    current += length.Value;
                    count++;
                }
            }
        }

        public static IMatch UnBox(this IMatch data) => data is Match box? box.UnBox : data;

        public static Match AnyChar(this string data, bool isCaseSenitive = true)
            => new Match(new AnyCharMatch(data, isCaseSenitive));

        public static Match Box(this Match.IError error) => new Match(new ErrorMatch(error));

        public static Match Box
            (this string data, bool isCaseSenitive = true) => new Match(new CharMatch(data, isCaseSenitive));

        public static Match Box(this IMatch data) => data as Match ?? new Match(data);

        public static Match Repeat(this IMatch data, int minCount = 0, int? maxCount = null)
            => new Match(new Repeater(data.UnBox(), minCount, maxCount));

        public static Match Else(this string data, IMatch other) => data.Box().Else(other);
        public static Match Else(this IMatch data, string other) => data.Else(other.Box());
        public static Match Else(this Match.IError data, IMatch other) => data.Box().Else(other);
        public static Match Else(this IMatch data, Match.IError other) => data.Else(other.Box());

        public static Match Else(this IMatch data, IMatch other)
            => new Match(new ElseMatch(data.UnBox(), other.UnBox()));
    }
}