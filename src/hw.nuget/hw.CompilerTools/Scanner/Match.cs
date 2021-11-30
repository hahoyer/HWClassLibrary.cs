using System;
using hw.DebugFormatter;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Scanner
{
    [PublicAPI]
    public interface IMatch
    {
        int? Match(SourcePosition sourcePosition);
    }

    [PublicAPI]
    public sealed class Match
        : Dumpable
            , IMatch
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

        sealed class NotMatch
            : Dumpable
                , IMatch
        {
            readonly IMatch Data;
            public NotMatch(IMatch data) => Data = data;

            int? IMatch.Match(SourcePosition sourcePosition)
            {
                var result = Data.Match(sourcePosition);
                return result == null? 1 : null;
            }
        }

        sealed class Sequence
            : Dumpable
                , IMatch
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

            int? IMatch.Match(SourcePosition sourcePosition)
            {
                var result = Data.Match(sourcePosition);
                if(result == null)
                    return null;

                var otherResult = Other.Match(sourcePosition + result.Value);
                if(otherResult == null)
                    return null;

                return result.Value + otherResult.Value;
            }
        }

        sealed class FunctionalMatch
            : Dumpable
                , IMatch
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

            int? IMatch.Match(SourcePosition sourcePosition)
                => Func(sourcePosition.Current) != IsTrue? null : 1;
        }

        sealed class FindMatch
            : Dumpable
                , IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            public FindMatch(IMatch data) => Data = data;

            int? IMatch.Match(SourcePosition sourcePosition)
            {
                var current = sourcePosition.Clone;
                while(true)
                {
                    var result = Data.Match(current);
                    if(result != null)
                        return current - sourcePosition + result;

                    if(current.IsEnd)
                        return null;

                    current.Position += 1;
                }
            }
        }

        sealed class ValueMatch
            : Dumpable
                , IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            [EnableDump]
            readonly Func<string, IMatch> Func;

            public ValueMatch(IMatch data, Func<string, IMatch> func)
            {
                Data = data;
                Func = func;
            }

            int? IMatch.Match(SourcePosition sourcePosition)
            {
                var length = Data.Match(sourcePosition);
                if(length == null)
                    return null;

                var value = sourcePosition.SubString(0, length.Value);
                var funcResult = Func(value).Match(sourcePosition + length.Value);
                return funcResult == null? null : length.Value + funcResult;
            }
        }

        sealed class EndMatch
            : Dumpable
                , IMatch
        {
            int? IMatch.Match(SourcePosition sourcePosition) => sourcePosition.IsEnd? 0 : null;
        }

        sealed class BreakMatch : IMatch
        {
            public int? Match(SourcePosition sourcePosition)
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

        public static Match Break => new Match(new BreakMatch());

        public static Match WhiteSpace => Box(char.IsWhiteSpace);
        public static Match LineEnd => "\n".Box().Else("\r\n").Else(End);
        public static Match End => new Match(new EndMatch());
        public static Match Digit => Box(char.IsDigit);
        public static Match Letter => Box(char.IsLetter);
        public static Match Any => Box(c => true);

        [DisableDump]
        public IMatch UnBox => Data.UnBox();

        [DisableDump]
        public Match Find => new Match(new FindMatch(Data));

        [DisableDump]
        public Match Not => new Match(new NotMatch(this));

        int? IMatch.Match(SourcePosition sourcePosition) => Data.Match(sourcePosition);

        public static Match Box(Func<char, bool> func) => new Match(new FunctionalMatch(func, true));

        public static Match operator +(string target, Match y) => target.Box() + y;
        public static Match operator +(Match target, string y) => target + y.Box();

        public static Match operator +(IError target, Match y) => target.Box() + y;
        public static Match operator +(Match target, IError y) => target + y.Box();

        public static Match operator +(Match target, Match y)
            => new Match(new Sequence(target.UnBox(), y.UnBox()));

        public static Match operator |(Match target, Match y) => target.Else(y);

        public Match Repeat(int minCount = 0, int? maxCount = null)
            => Data.Repeat(minCount, maxCount);

        public Match Option() => Data.Repeat(maxCount: 1);

        public Match Else(IMatch other) => Data.Else(other);
        public Match Value(Func<string, IMatch> func) => new Match(new ValueMatch(Data, func));
    }
}