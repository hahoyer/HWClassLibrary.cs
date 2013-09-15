// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.Debug
{
    public sealed class Profiler
    {
        sealed class Dumper
        {
            readonly int? _count;
            readonly ProfileItem[] _data;
            readonly TimeSpan _sumMax;
            readonly TimeSpan _sumAll;
            int _index;
            TimeSpan _sum;

            public Dumper(Profiler profiler, int? count, double hidden)
            {
                _count = count;
                _data = profiler._profileItems.Values.OrderByDescending(x => x.Duration).ToArray();
                _sumAll = _data.Sum(x => x.Duration);
                _sumMax = new TimeSpan((long) (_sumAll.Ticks * (1.0 - hidden)));
                _index = 0;
                _sum = new TimeSpan();
            }

            public string Format()
            {
                if(_data.Length == 0)
                    return "\n=========== Profile empty ============\n";

                var result = "";
                for(; _index < _data.Length && _index != _count && _sum <= _sumMax; _index++)
                {
                    var item = _data[_index];
                    result += item.Format(_index.ToString());
                    _sum += item.Duration;
                }

                var stringAligner = new StringAligner();
                stringAligner.AddFloatingColumn("#");
                stringAligner.AddFloatingColumn("  ");
                stringAligner.AddFloatingColumn("  ");
                stringAligner.AddFloatingColumn("  ");

                result = "\n=========== Profile ==================\n" + stringAligner.Format(result);
                result += "Total:\t" + _sumAll.Format3Digits();
                if(_index < _data.Length)
                    result +=
                        " ("
                        + (_data.Length - _index)
                        + " not-shown-items "
                        + (_sumAll - _sum).Format3Digits()
                        + ")";
                result += "\n======================================\n";
                return result;
            }
        }

        /// <summary>
        ///     Measures the specified expression.
        /// </summary>
        /// <typeparam name="T"> The type the specitied expression returns </typeparam>
        /// <param name="expression"> a function without parameters returning something. </param>
        /// <returns> the result of the invokation of the specified expression </returns>
        public static T Measure<T>(Func<T> expression) { return _instance.InternalMeasure("", expression, 1); }

        /// <summary>
        ///     Measures the specified action.
        /// </summary>
        /// <param name="action"> The action. </param>
        public static void Measure(Action action) { _instance.InternalMeasure("", action, 1); }

        /// <summary>
        ///     Measures the specified expression.
        /// </summary>
        /// <typeparam name="T"> The type the specitied expression returns </typeparam>
        /// <param name="flag"> A flag that is used in dump. </param>
        /// <param name="expression"> a function without parameters returning something. </param>
        /// <returns> the result of the invokation of the specified expression </returns>
        public static T Measure<T>(string flag, Func<T> expression) { return _instance.InternalMeasure(flag, expression, 1); }

        /// <summary>
        ///     Measures the specified action.
        /// </summary>
        /// <param name="flag"> A flag that is used in dump. </param>
        /// <param name="action"> The action. </param>
        public static void Measure(string flag, Action action) { _instance.InternalMeasure(flag, action, 1); }

        /// <summary>
        ///     Resets the profiler data.
        /// </summary>
        public static void Reset()
        {
            lock(_instance)
                _instance.InternalReset();
            _instance = new Profiler();
        }

        /// <summary>
        ///     Formats the data accumulated so far.
        /// </summary>
        /// <param name="count"> The number of measured expressions in result. </param>
        /// <param name="hidden"> The relative amount of time that will be hidden in result. </param>
        /// <returns> The formatted data. </returns>
        /// <remarks>
        ///     The result contains one line for each measured expression, that is not ignored. Each line contains <para>- the file path, the line and the start column of the measuered expression in the source file,
        ///                                                                                                            (The information is formatted in a way, that within VisualStudio doubleclicking on such a line will open it.)</para> <para>- the flag, if provided,</para> <para>- the ranking,</para> <para>- the execution count,</para> <para>- the average duration of one execution,</para> <para>- the duration,</para> of the expression. The the lines are sorted by descending duration. by use of <paramref
        ///      name="count" /> and <paramref name="hidden" /> the number of lines can be restricted.
        /// </remarks>
        public static string Format(int? count = null, double hidden = 0.1)
        {
            lock(_instance)
                return new Dumper(_instance, count, hidden).Format();
        }

        static Profiler _instance = new Profiler();

        readonly Dictionary<string, ProfileItem> _profileItems = new Dictionary<string, ProfileItem>();
        readonly Stopwatch _stopwatch;
        readonly Stack<ProfileItem> _stack = new Stack<ProfileItem>();
        ProfileItem _current;


        Profiler()
        {
            _current = new ProfileItem("");
            _stopwatch = new Stopwatch();
            _current.Start(_stopwatch.Elapsed);
            _stopwatch.Start();
        }

        void InternalMeasure(string flag, Action action, int depth)
        {
            BeforeAction(flag, depth + 1);
            action();
            AfterAction();
        }

        T InternalMeasure<T>(string flag, Func<T> expression, int depth)
        {
            BeforeAction(flag, depth + 1);
            var result = expression();
            AfterAction();
            return result;
        }

        void BeforeAction(string flag, int depth)
        {
            lock(this)
            {
                _stopwatch.Stop();
                var start = _stopwatch.Elapsed;
                _current.Suspend(start);
                _stack.Push(_current);
                var position = Tracer.MethodHeader(depth + 1, FilePositionTag.Profiler) + flag;
                if(!_profileItems.TryGetValue(position, out _current))
                {
                    _current = new ProfileItem(position);
                    _profileItems.Add(position, _current);
                }
                _current.Start(start);
                _stopwatch.Start();
            }
        }

        void AfterAction()
        {
            lock(this)
            {
                _stopwatch.Stop();
                var end = _stopwatch.Elapsed;
                _current.End(end);
                _current = _stack.Pop();
                _current.Resume(end);
                _stopwatch.Start();
            }
        }

        void InternalReset() { Tracer.Assert(_stack.Count == 0); }
    }

    sealed class ProfileItem
    {
        readonly string _position;
        TimeSpan _duration;
        long _countStart;
        long _countEnd;
        long _suspendCount;

        public ProfileItem(string position) { _position = position; }

        public TimeSpan Duration { get { return _duration; } }
        TimeSpan AverageDuration { get { return new TimeSpan(_duration.Ticks / _countEnd); } }

        bool IsValid { get { return _countStart == _countEnd && _suspendCount == 0; } }

        public void Start(TimeSpan duration)
        {
            _countStart++;
            _duration -= duration;
            if(IsValid)
                Tracer.Assert(_duration.Ticks >= 0);
        }

        public void End(TimeSpan duration)
        {
            _countEnd++;
            _duration += duration;
            if(IsValid)
                Tracer.Assert(_duration.Ticks >= 0);
        }

        public string Format(string tag)
        {
            Tracer.Assert(IsValid);
            return _position
                   + " #"
                   + tag
                   + ":  "
                   + _countEnd.Format3Digits()
                   + "x  "
                   + AverageDuration.Format3Digits()
                   + "  "
                   + _duration.Format3Digits()
                   + "\n";
        }

        public void Suspend(TimeSpan start)
        {
            _suspendCount++;
            _duration += start;
            if(IsValid)
                Tracer.Assert(_duration.Ticks >= 0);
        }

        public void Resume(TimeSpan end)
        {
            _suspendCount--;
            _duration -= end;
            if(IsValid)
                Tracer.Assert(_duration.Ticks >= 0);
        }
    }
}