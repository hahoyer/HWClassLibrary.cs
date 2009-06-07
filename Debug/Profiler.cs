using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Helper;

namespace HWClassLibrary.Debug
{
    public class Profiler
    {
        private class Dumper
        {
            private readonly int? _count;
            private readonly ProfileItem[] _data;
            private readonly TimeSpan _sumMax;
            private readonly TimeSpan _sumAll;
            private int _index;
            private TimeSpan _sum;

            public Dumper(Profiler profiler, int? count, double hidden)
            {
                _count = count;
                _data = profiler._profileItems.Values.OrderByDescending(x => x.Duration).ToArray();
                _sumAll = _data.Sum(x => x.Duration);
                _sumMax = new TimeSpan((long) (_sumAll.Ticks*(1.0 - hidden)));
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

        public static T Measure<T>(Func<T> expression)
        {
            return _instance.InternalMeasure("", expression, 1);
        }

        public static void Measure(Action action)
        {
            _instance.InternalMeasure("", action, 1);
        }

        public static T Measure<T>(string flag, Func<T> expression)
        {
            return _instance.InternalMeasure(flag, expression, 1);
        }

        public static void Measure(string flag, Action action)
        {
            _instance.InternalMeasure(flag, action, 1);
        }

        public static void Reset()
        {
            lock(_instance)
                _instance.InternalReset();
            _instance = new Profiler();
        }

        public static string Dump()
        {
            lock(_instance)
                return new Dumper(_instance, null, 0.1).Format();
        }

        private static Profiler _instance = new Profiler();

        private readonly Dictionary<string, ProfileItem> _profileItems = new Dictionary<string, ProfileItem>();
        private readonly Stopwatch _stopwatch;
        private readonly Stack<ProfileItem> _stack = new Stack<ProfileItem>();
        private ProfileItem _current;


        private Profiler()
        {
            _current = new ProfileItem("");
            _stopwatch = new Stopwatch();
            _current.Start(_stopwatch.Elapsed);
            _stopwatch.Start();
        }

        private void InternalMeasure(string flag, Action action, int depth)
        {
            BeforeAction(flag, depth + 1);
            action();
            AfterAction();
        }

        private T InternalMeasure<T>(string flag, Func<T> expression, int depth)
        {
            BeforeAction(flag, depth + 1);
            var result = expression();
            AfterAction();
            return result;
        }

        private void BeforeAction(string flag, int depth)
        {
            lock(this)
            {
                _stopwatch.Stop();
                var start = _stopwatch.Elapsed;
                _current.Suspend(start);
                _stack.Push(_current);
                var position = Tracer.MethodHeader(depth + 1)+flag;
                if(!_profileItems.TryGetValue(position, out _current))
                {
                    _current = new ProfileItem(position);
                    _profileItems.Add(position, _current);
                }
                _current.Start(start);
                _stopwatch.Start();
            }
        }

        private void AfterAction()
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

        private void InternalReset()
        {
            Tracer.Assert(_stack.Count == 0);
        }
    }

    internal class ProfileItem
    {
        private readonly string _position;
        private TimeSpan _duration;
        private long _countStart;
        private long _countEnd;
        private long _suspendCount;

        public ProfileItem(string position)
        {
            _position = position;
        }

        public TimeSpan Duration { get { return _duration; } }
        public TimeSpan AverageDuration { get { return new TimeSpan(_duration.Ticks/_countEnd); } }

        public bool IsValid { get { return _countStart == _countEnd && _suspendCount == 0; } }

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