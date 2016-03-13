using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    public interface IMatch
    {
        int? Match(SourcePosn sourcePosn);
    }

    public sealed class Match : Dumpable, IMatch
    {
        readonly IMatch _data;
        internal Match(IMatch data)
        {
            Tracer.Assert(!(data is Match));
            _data = data;
        }

        int? IMatch.Match(SourcePosn sourcePosn) => _data.Match(sourcePosn);

        [DisableDump]
        public IMatch UnBox => _data.UnBox();
        public static Match Break => new Match(new BreakMatch());

        public Match Repeat(int minCount = 0, int? maxCount = null) => _data.Repeat(minCount, maxCount);
        public Match Else(IMatch other) => _data.Else(other);
        public Match Value(Func<string, IMatch> func) => new Match(new ValueMatch(_data, func));
        [DisableDump]
        public Match Find => new Match(new FindMatch(_data));

        public static Match WhiteSpace => Box(char.IsWhiteSpace);
        public static Match LineEnd => "\n".Box().Else("\r\n").Else(End);
        public static Match End => new Match(new EndMatch());
        public static Match Digit => Box(char.IsDigit);
        public static Match Letter => Box(char.IsLetter);
        [DisableDump]
        public Match Not => new Match(new NotMatch(this));

        public static Match Box(Func<char, bool> func) => new Match(new FunctionalMatch(func, true));

        public static Match operator +(string x, Match y) => x.Box() + y;
        public static Match operator +(Match x, string y) => x + y.Box();

        public static Match operator +(IError x, Match y) => x.Box() + y;
        public static Match operator +(Match x, IError y) => x + y.Box();

        public static Match operator +(Match x, Match y) => new Match(new Sequence(x.UnBox(), y.UnBox()));

        sealed class NotMatch : Dumpable, IMatch
        {
            readonly IMatch _data;
            public NotMatch(IMatch data) { _data = data; }
            int? IMatch.Match(SourcePosn sourcePosn)
            {
                var result = _data.Match(sourcePosn);
                return result == null ? 1 : (int?) null;
            }
        }

        sealed class Sequence : Dumpable, IMatch
        {
            [EnableDump]
            readonly IMatch _data;
            [EnableDump]
            readonly IMatch _other;
            public Sequence(IMatch data, IMatch other)
            {
                _data = data;
                _other = other;
            }
            int? IMatch.Match(SourcePosn sourcePosn)
            {
                var result = _data.Match(sourcePosn);
                if(result == null)
                    return null;

                var otherResult = _other.Match(sourcePosn + result.Value);
                if(otherResult == null)
                    return null;

                return result.Value + otherResult.Value;
            }
        }

        sealed class FunctionalMatch : Dumpable, IMatch
        {
            [EnableDump]
            readonly Func<char, bool> _func;
            [EnableDump]
            readonly bool _isTrue;
            public FunctionalMatch(Func<char, bool> func, bool isTrue)
            {
                _func = func;
                _isTrue = isTrue;
            }

            int? IMatch.Match(SourcePosn sourcePosn) => _func(sourcePosn.Current) != _isTrue ? null : (int?) 1;
        }

        sealed class FindMatch : Dumpable, IMatch
        {
            [EnableDump]
            readonly IMatch _data;
            public FindMatch(IMatch data) { _data = data; }

            int? IMatch.Match(SourcePosn sourcePosn)
            {
                var current = sourcePosn.Clone;
                while(true)
                {
                    var result = _data.Match(current);
                    if(result != null)
                        return (current - sourcePosn) + result;

                    if(current.IsEnd)
                        return null;
                    current.Position += 1;
                }
            }
        }

        sealed class ValueMatch : Dumpable, IMatch
        {
            [EnableDump]
            readonly IMatch _data;
            [EnableDump]
            readonly Func<string, IMatch> _func;

            public ValueMatch(IMatch data, Func<string, IMatch> func)
            {
                _data = data;
                _func = func;
            }

            int? IMatch.Match(SourcePosn sourcePosn)
            {
                var length = _data.Match(sourcePosn);
                if(length == null)
                    return null;
                var value = sourcePosn.SubString(0, length.Value);
                var funcResult = _func(value).Match(sourcePosn + length.Value);
                return funcResult == null ? null : length.Value + funcResult;
            }
        }

        sealed class EndMatch : Dumpable, IMatch
        {
            int? IMatch.Match(SourcePosn sourcePosn) => sourcePosn.IsEnd ? 0 : (int?) null;
        }

        sealed class BreakMatch : IMatch
        {
            public int? Match(SourcePosn sourcePosn)
            {
                Tracer.TraceBreak();
                return 0;
            }
        }

        public interface IError
        {}

        public sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            public readonly IError Error;

            internal Exception(SourcePosn sourcePosn, IError error)
            {
                SourcePosn = sourcePosn;
                Error = error;
            }
        }
    }
}