#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace hw.Scanner
{
    static class MatchExtension
    {
        internal static IMatch UnBox(this IMatch data)
        {
            var box = data as Match;
            return box == null ? data : box.UnBox;
        }

        internal static Match AnyChar(this string data) { return new Match(new AnyCharMatch(data)); }
        internal static Match Box(this Match.IError error) { return new Match(new ErrorMatch(error)); }
        internal static Match Box(this string data) { return new Match(new CharMatch(data)); }
        internal static Match Box(this IMatch data) { return data as Match ?? new Match(data); }
        internal static Match Repeat(this IMatch data, int minCount = 0, int? maxCount = null) { return new Match(new Repeater(data.UnBox(), minCount, maxCount)); }

        internal static Match Else(this string data, IMatch other) { return data.Box().Else(other); }
        internal static Match Else(this IMatch data, string other) { return data.Else(other.Box()); }
        internal static Match Else(this Match.IError data, IMatch other) { return data.Box().Else(other); }
        internal static Match Else(this IMatch data, Match.IError other) { return data.Else(other.Box()); }
        internal static Match Else(this IMatch data, IMatch other) { return new Match(new ElseMatch(data.UnBox(), other.UnBox())); }

        sealed class ErrorMatch : Dumpable, IMatch
        {
            readonly Match.IError _error;
            public ErrorMatch(Match.IError error) { _error = error; }
            int? IMatch.Match(SourcePosn sourcePosn) { throw new Match.Exception(sourcePosn, _error); }
        }

        sealed class CharMatch : Dumpable, IMatch
        {
            [EnableDump]
            readonly string _data;
            public CharMatch(string data) { _data = data; }
            int? IMatch.Match(SourcePosn sourcePosn)
            {
                var result = _data.Length;
                return sourcePosn.StartsWith(_data) ? (int?) result : null;
            }
        }

        sealed class AnyCharMatch : Dumpable, IMatch
        {
            [EnableDump]
            readonly string _data;
            public AnyCharMatch(string data) { _data = data; }
            int? IMatch.Match(SourcePosn sourcePosn) { return _data.Contains(sourcePosn.Current) ? (int?) 1 : null; }
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

            int? IMatch.Match(SourcePosn sourcePosn) { return _data.Match(sourcePosn) ?? _other.Match(sourcePosn); }
        }

        sealed class Repeater : Dumpable, IMatch
        {
            [EnableDump]
            readonly IMatch _data;
            [EnableDump]
            readonly int _minCount;
            [EnableDump]
            readonly int? _maxCount;

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
    }
}