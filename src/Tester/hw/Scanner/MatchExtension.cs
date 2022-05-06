using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Scanner;

[PublicAPI]
public static class MatchExtension
{
    [PublicAPI]
    public interface IPositionExceptionFactory
    {
        Exception Create(SourcePosition sourcePosition);
    }

    sealed class ErrorMatch : DumpableObject, IMatch
    {
        readonly Match.IError Error;
        public ErrorMatch(Match.IError error) => Error = error;

        int? IMatch.Match(SourcePart span, bool isForward)
        {
            if(Error is IPositionExceptionFactory positionFactory)
                throw positionFactory.Create(span.GetStart(isForward));

            throw new Match.Exception(span.GetStart(isForward), Error);
        }
    }

    sealed class CharMatch : DumpableObject, IMatch
    {
        [EnableDump]
        readonly string Data;

        readonly StringComparison Type;

        public CharMatch(string data, bool isCaseSensitive)
        {
            Data = data;
            Type = isCaseSensitive? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        }

        int? IMatch.Match(SourcePart span, bool isForward)
        {
            if(span.Length < Data.Length)
                return null;

            if(isForward)
            {
                var startPosition = span.Start;
                return startPosition.StartsWith(Data, Type)? Data.Length : null;
            }
            else
            {
                var startPosition = span.End - Data.Length;
                return startPosition.StartsWith(Data, Type)? -Data.Length : null;
            }
        }
    }

    sealed class AnyCharMatch : DumpableObject, IMatch
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

        public AnyCharMatch(string data, bool isCaseSensitive = true)
        {
            Data = data;
            Comparer = isCaseSensitive? Default : UpperInvariant;
        }

        int? IMatch.Match(SourcePart span, bool isForward)
            => span.Length > 0 && Data.Contains((span.GetStart(isForward) - (isForward? 0 : 1)).Current, Comparer)
                ? 1
                : null;

        static IEqualityComparer<char> Default => new DefaultComparer();
        static IEqualityComparer<char> UpperInvariant => new UpperInvariantComparer();
    }

    sealed class ElseMatch : DumpableObject, IMatch
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

        int? IMatch.Match(SourcePart span, bool isForward)
            => Data.Match(span, isForward) ?? Other.Match(span, isForward);
    }

    sealed class Repeater : DumpableObject, IMatch
    {
        [EnableDump]
        readonly IMatch Data;

        [EnableDump]
        readonly int? MaxCount;

        [EnableDump]
        readonly int MinCount;

        public Repeater(IMatch data, int minCount, int? maxCount)
        {
            (!(data is Match)).Assert();
            (minCount >= 0).Assert();
            (maxCount == null || maxCount >= minCount).Assert();
            Data = data;
            MinCount = minCount;
            MaxCount = maxCount;
        }

        int? IMatch.Match(SourcePart span, bool isForward)
        {
            var count = 0;
            var start = span.GetStart(isForward);
            var end = span.GetEnd(isForward);
            var current = start.Clone;

            while(true)
            {
                var result = current - start;
                if(count == MaxCount)
                    return result;

                var length = Data.Match(current.Span(end), isForward);
                if(length == null)
                    return count < MinCount? null : result;

                count++;

                if(length == 0)
                    return count < MinCount? null : result;

                if(isForward)
                    (current < end).Assert();
                else
                    (current > end).Assert();

                current += length.Value;
            }
        }
    }

    public static IMatch UnBox(this IMatch data) => data is Match box? box.UnBox : data;

    public static Match AnyChar(this string data, bool isCaseSensitive = true)
        => new(new AnyCharMatch(data, isCaseSensitive));

    public static Match Box(this Match.IError error) => new(new ErrorMatch(error));

    public static Match Box(this string data, bool isCaseSensitive = true)
        => new(new CharMatch(data, isCaseSensitive));

    public static Match Box(this IMatch data) => data as Match ?? new Match(data);

    public static Match Repeat(this IMatch data, int minCount = 0, int? maxCount = null)
        => new(new Repeater(data.UnBox(), minCount, maxCount));

    public static Match Else(this string data, IMatch other) => data.Box().Else(other);
    public static Match Else(this IMatch data, string other) => data.Else(other.Box());
    public static Match Else(this Match.IError data, IMatch other) => data.Box().Else(other);
    public static Match Else(this IMatch data, Match.IError other) => data.Else(other.Box());

    public static Match Else(this IMatch data, IMatch other) => new(new ElseMatch(data.UnBox(), other.UnBox()));
}