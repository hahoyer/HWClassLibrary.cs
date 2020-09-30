using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Helper;

namespace hw.DebugFormatter
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
        ///     Provides a standard frame for accumulating and reporting result of measurements, that are contained in the given
        ///     action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="count"> The number of measured expressions in result, default is "null" for "no restricton. </param>
        /// <param name="hidden"> The relative amount of time that will be hidden in result, default is 0.1. </param>
        public static void Frame(Action action, int? count = null, double hidden = 0.1)
        {
            Reset();
            _instance.InternalMeasure(action, "", 1);
            Tracer.FlaggedLine(Format(count, hidden));
        }

        /// <summary>
        ///     Provides a standard frame for accumulating and reporting result of measurements, that are contained in the given
        ///     function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="count"> The number of measured expressions in result, default is "null" for "no restricton. </param>
        /// <param name="hidden"> The relative amount of time that will be hidden in result, default is 0.1. </param>
        public static TResult Frame<TResult>(Func<TResult> function, int? count = null, double hidden = 0.1)
        {
            Reset();
            var result = _instance.InternalMeasure(function, "", 1);
            Tracer.FlaggedLine(Format(count, hidden));
            return result;
        }

        /// <summary>
        ///     Start measurement of the following code parts until next item.End() statement.
        /// </summary>
        /// <param name="flag"> </param>
        /// <returns> an item, that represents the measurement.</returns>
        public static Item Start(string flag = "") { return new Item(_instance, flag, 1); }

        /// <summary>
        ///     Measures the specified expression.
        /// </summary>
        /// <typeparam name="T"> The type the specitied expression returns </typeparam>
        /// <param name="expression"> a function without parameters returning something. </param>
        /// <param name="flag"> A flag that is used in dump. </param>
        /// <returns> the result of the invokation of the specified expression </returns>
        public static T Measure<T>(Func<T> expression, string flag = "") { return _instance.InternalMeasure(expression, flag, 1); }

        /// <summary>
        ///     Measures the specified action.
        /// </summary>
        /// <param name="action"> The action. </param>
        /// <param name="flag"> A flag that is used in dump. </param>
        public static void Measure(Action action, string flag = "") { _instance.InternalMeasure(action, flag, 1); }

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
        /// <param name="count"> The number of measured expressions in result, default is "null" for "no restricton. </param>
        /// <param name="hidden"> The relative amount of time that will be hidden in result, default is 0.1. </param>
        /// <returns> The formatted data. </returns>
        /// <remarks>
        ///     The result contains one line for each measured expression, that is not ignored.
        ///     Each line contains
        ///     <para>
        ///         - the file path, the line and the start column of the measuered expression in the source file, (The
        ///         information is formatted in a way, that within VisualStudio doubleclicking on such a line will open it.)
        ///     </para>
        ///     <para> - the flag, if provided, </para>
        ///     <para> - the ranking, </para>
        ///     <para> - the execution count, </para>
        ///     <para> - the average duration of one execution, </para>
        ///     <para> - the duration, </para>
        ///     of the expression.
        ///     The the lines are sorted by descending duration.
        ///     by use of <paramref name="count" /> and <paramref name="hidden" /> the number of lines can be restricted.
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

        void InternalMeasure(Action action, string flag, int stackFrameDepth)
        {
            BeforeAction(flag, stackFrameDepth + 1);
            action();
            AfterAction();
        }

        T InternalMeasure<T>(Func<T> expression, string flag, int stackFrameDepth)
        {
            BeforeAction(flag, stackFrameDepth + 1);
            var result = expression();
            AfterAction();
            return result;
        }

        void BeforeAction(string flag, int stackFrameDepth)
        {
            lock(this)
            {
                _stopwatch.Stop();
                var start = _stopwatch.Elapsed;
                _current.Suspend(start);
                _stack.Push(_current);
                var position = Tracer.MethodHeader(stackFrameDepth:stackFrameDepth + 1) + flag;
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


        /// <summary>
        ///     Measuement Item
        /// </summary>
        public sealed class Item
        {
            readonly Profiler _profiler;
            ProfileItem _item;
            internal Item(Profiler profiler, string flag, int stackFrameDepth)
            {
                _profiler = profiler;
                Start(flag, stackFrameDepth + 1);
            }
            void Start(string flag, int stackFrameDepth)
            {
                _instance.BeforeAction(flag, stackFrameDepth + 1);
                _item = _profiler._current;
            }

            /// <summary>
            ///     End of measurement for this item
            /// </summary>
            public void End()
            {
                Tracer.Assert(_profiler._current == _item);
                _profiler.AfterAction();
            }
            /// <summary>
            ///     End of measurement for this item and start of new measurement
            /// </summary>
            public void Next(string flag = "")
            {
                End();
                Start(flag, 1);
            }
        }
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