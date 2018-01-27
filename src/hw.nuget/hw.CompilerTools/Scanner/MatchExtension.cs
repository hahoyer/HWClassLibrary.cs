using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    public static class MatchExtension
    {
        sealed class ErrorMatch : Dumpable, IMatch
        {
            readonly Match.IError _error;
            public ErrorMatch(Match.IError error) => _error = error;

            int? IMatch.Match(SourcePosn sourcePosn)
            {
                if(_error is IPositionExceptionFactory positionFactory)
                    throw positionFactory.Create(sourcePosn);

                throw new Match.Exception(sourcePosn, _error);
            }
        }

        sealed class CharMatch : Dumpable, IMatch
        {
            [EnableDump]
            readonly string _data;

            readonly StringComparison Type;

            public CharMatch(string data, bool isCaseSenitive)
            {
                _data = data;
                Type = isCaseSenitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            }

            int? IMatch.Match(SourcePosn sourcePosn)
            {
                var result = _data.Length;
                return sourcePosn.StartsWith(_data, Type) ? (int?) result : null;
            }
        }

        sealed class AnyCharMatch : Dumpable, IMatch
        {
            sealed class DefaultComparer : IEqualityComparer<char>
            {
                bool IEqualityComparer<char>.Equals(char x, char y) => x == y;
                int IEqualityComparer<char>.GetHashCode(char obj) => obj;
            }

            sealed class UpperInvariantComparer : IEqualityComparer<char>
            {
                bool IEqualityComparer<char>.Equals(char x, char y)
                    => char.ToUpperInvariant(x) == char.ToUpperInvariant(y);

                int IEqualityComparer<char>.GetHashCode(char obj) => char.ToUpperInvariant(obj);
            }

            static IEqualityComparer<char> Default => new DefaultComparer();
            static IEqualityComparer<char> UpperInvariant => new UpperInvariantComparer();

            [EnableDump]
            readonly string _data;

            readonly IEqualityComparer<char> Comparer;

            public AnyCharMatch(string data, bool isCaseSenitive = true)
            {
                _data = data;
                Comparer = isCaseSenitive ? Default : UpperInvariant;
            }

            int? IMatch.Match(SourcePosn sourcePosn)
                => _data.Contains(sourcePosn.Current, Comparer) ? (int?) 1 : null;
        }

        sealed class ElseMatch : Dumpable, IMatch
        {
            [EnableDump]
            readonly IMatch _data;

            [EnableDump]
            readonly IMatch _other;

            public ElseMatch(IMatch data, IMatch other)
            {
                _data = data;
                _other = other;
            }

            int? IMatch.Match(SourcePosn sourcePosn)
                => _data.Match(sourcePosn) ?? _other.Match(sourcePosn);
        }

        sealed class Repeater : Dumpable, IMatch
        {
            [EnableDump]
            readonly IMatch _data;

            [EnableDump]
            readonly int? _maxCount;

            [EnableDump]
            readonly int _minCount;

            public Repeater(IMatch data, int minCount, int? maxCount)
            {
                Tracer.Assert(!(data is Match));
                Tracer.Assert(minCount >= 0);
                Tracer.Assert(maxCount == null || maxCount >= minCount);
                _data = data;
                _minCount = minCount;
                _maxCount = maxCount;
            }

            int? IMatch.Match(SourcePosn sourcePosn)
            {
                var count = 0;
                var current = sourcePosn;
                while(true)
                {
                    var result = current - sourcePosn;
                    if(count == _maxCount)
                        return result;

                    var length = _data.Match(current);
                    if(length == null)
                        return count < _minCount ? null : (int?) result;
                    if(current.IsEnd)
                        return null;

                    current += length.Value;
                    count++;
                }
            }
        }

        public interface IPositionExceptionFactory
        {
            Exception Create(SourcePosn sourcePosn);
        }

        public static IMatch UnBox(this IMatch data) => data is Match box ? box.UnBox : data;

        public static Match AnyChar(this string data, bool isCaseSenitive = true) 
            => new Match(new AnyCharMatch(data, isCaseSenitive));

        public static Match Box(this Match.IError error) => new Match(new ErrorMatch(error));
        public static Match Box(this string data, bool isCaseSenitive = true) => new Match(new CharMatch(data,isCaseSenitive));
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