using System;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Scanner
{
    [PublicAPI]
    public interface IMatch
    {
        int? Match(SourcePart span, bool isForward = true);
    }

    [PublicAPI]
    public sealed class Match : DumpableObject, IMatch
    {
        public interface IError { }

        public sealed class Exception : System.Exception
        {
            public readonly IError Error;
            public readonly SourcePosition SourcePosition;

            public Exception(SourcePosition sourcePosition, IError error)
            {
                SourcePosition = sourcePosition;
                Error = error;
            }
        }

        sealed class NotMatch : DumpableObject, IMatch
        {
            readonly IMatch Data;
            public NotMatch(IMatch data) => Data = data;

            int? IMatch.Match(SourcePart span, bool isForward)
            {
                var result = Data.Match(span, isForward);
                return result != null
                    ? null
                    : isForward
                        ? 1
                        : -1;
            }
        }

        sealed class Sequence : DumpableObject, IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            [EnableDump]
            readonly IMatch Other;

            public Sequence(IMatch data, IMatch other)
            {
                Data = data;
                Other = other;
            }

            int? IMatch.Match(SourcePart span, bool isForward)
            {
                int? result = 0;
                int? otherResult = 0;

                var start = span.GetStart(isForward);
                var end = span.GetEnd(isForward);

                var current = start.Clone;
                if(isForward)
                {
                    result = Data.Match(span);

                    if(result == null)
                        return null;
                    current += result.Value;
                    (current <= end).Assert();
                }

                otherResult = Other.Match(current.Span(end), isForward);
                if(otherResult == null)
                    return null;
                current += otherResult.Value;

                if(!isForward)
                {
                    (current >= end).Assert();
                    result = Data.Match(current.Span(span.End), false);
                    if(result == null)
                        return null;
                    current += result.Value;
                }

                return current - start;
            }
        }

        sealed class FunctionalMatch : DumpableObject, IMatch
        {
            [EnableDump]
            readonly Func<char, bool> Func;

            [EnableDump]
            readonly bool IsTrue;

            public FunctionalMatch(Func<char, bool> func, bool isTrue)
            {
                Func = func;
                IsTrue = isTrue;
            }

            int? IMatch.Match(SourcePart span, bool isForward)
                => span.Length == 0 || Func((span.GetStart(isForward) - (isForward? 0 : 1)).Current) != IsTrue
                    ? null
                    : isForward
                        ? 1
                        : -1;
        }

        sealed class FindMatch : DumpableObject, IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            readonly bool IncludeMatch;

            public FindMatch(IMatch data, bool includeMatch = true)
            {
                Data = data;
                IncludeMatch = includeMatch;
            }

            int? IMatch.Match(SourcePart span, bool isForward)
            {
                var start = span.GetStart(isForward);
                var end = span.GetEnd(isForward);

                var current = start.Clone;
                while(true)
                {
                    var result = SingleMatch(current, start, end, isForward);
                    if(result != null)
                        return result;

                    if(current == end)
                        return null;

                    if(isForward)
                        (current < end).Assert();
                    else
                        (current > end).Assert();

                    current.Position += isForward? 1 : -1;
                }
            }

            int? SingleMatch(SourcePosition current, SourcePosition start, SourcePosition end, bool isForward)
            {
                var result = Data.Match(current.Span(end), isForward);
                if(result != null)
                    return current + (IncludeMatch? result.Value : 0) - start;
                return null;
            }
        }

        sealed class ValueMatch : DumpableObject, IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            [EnableDump]
            readonly Func<string, IMatch> GetMatcherForValue;

            public ValueMatch(IMatch data, Func<string, IMatch> getMatcherForValue)
            {
                Data = data;
                GetMatcherForValue = getMatcherForValue;
            }

            int? IMatch.Match(SourcePart span, bool isForward)
            {
                var length = Data.Match(span, isForward);
                if(length == null)
                    return null;

                var sourcePartOfValue = span.GetStart(isForward).Span(length.Value);
                var value = sourcePartOfValue.Id;
                var current = sourcePartOfValue.GetEnd(isForward);
                var end = span.GetEnd(isForward);

                if(isForward)
                    (current <= end).Assert();
                else
                    (current >= end).Assert();

                var funcResult = GetMatcherForValue(value).Match(current.Span(end), isForward);
                return funcResult == null? null : length.Value + funcResult;
            }
        }

        sealed class FrameMatch : DumpableObject, IMatch
        {
            int? IMatch.Match(SourcePart span, bool isForward)
                => span.Length == 0? 0 : null;
        }

        sealed class BreakMatch : DumpableObject, IMatch
        {
            int? IMatch.Match(SourcePart span, bool isForward)
            {
                Tracer.TraceBreak();
                return 0;
            }
        }


        readonly IMatch Data;

        internal Match(IMatch data)
        {
            (!(data is Match)).Assert();
            Data = data;
        }

        int? IMatch.Match(SourcePart span, bool isForward)
            => Data.Match(span, isForward);

        public static Match Break => new(new BreakMatch());

        public static Match WhiteSpace => Box(char.IsWhiteSpace);
        public static Match LineEnd => "\n".Box().Else("\r\n").Else(End);
        public static Match End => new(new FrameMatch());
        public static Match Digit => Box(char.IsDigit);
        public static Match Letter => Box(char.IsLetter);
        public static Match Any => Box(c => true);

        [DisableDump]
        public IMatch UnBox => Data.UnBox();

        [DisableDump]
        public Match Find => new(new FindMatch(Data));

        [DisableDump]
        public Match Until => new(new FindMatch(Data, false));

        [DisableDump]
        public Match Not => new(new NotMatch(this));

        public static Match Box(Func<char, bool> func) => new(new FunctionalMatch(func, true));

        public static Match operator +(string target, Match y) => target.Box() + y;
        public static Match operator +(Match target, string y) => target + y.Box();

        public static Match operator +(IError target, Match y) => target.Box() + y;
        public static Match operator +(Match target, IError y) => target + y.Box();

        public static Match operator +(Match target, Match y)
            => new(new Sequence(target.UnBox(), y.UnBox()));

        public static Match operator |(Match target, Match y) => target.Else(y);

        public Match Repeat(int minCount = 0, int? maxCount = null)
            => Data.Repeat(minCount, maxCount);

        public Match Option() => Data.Repeat(maxCount: 1);

        public Match Else(IMatch other) => Data.Else(other);
        public Match Value(Func<string, IMatch> func) => new(new ValueMatch(Data, func));
    }
}