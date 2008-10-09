using System;
using System.Collections;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    [Serializable]
    public class Sequence<T>: Dumpable, IEnumerable<T>
    {
        [DumpData(true)]
        private readonly T[] _data;

        private Sequence(Sequence<T> a, Sequence<T> b)
        {
            var x = new List<T>(a._data);
            x.AddRange(b._data);
            _data = x.ToArray();
        }

        private Sequence(List<T> a)
        {
            _data = a.ToArray();
        }

        private Sequence(T a, Sequence<T> b)
        {
            var x = new List<T> {a};
            x.AddRange(b._data);
            _data = x.ToArray();
        }

        private Sequence(Sequence<T> a, T b)
        {
            _data = new List<T>(a._data) {b}.ToArray();
        }

        internal Sequence()
        {
            _data = new T[]{};
        }

        internal Sequence(T a)
        {
            _data = new[] { a };
        }

        public static Sequence<T> operator +(Sequence<T> a, Sequence<T> b)
        {
            return new Sequence<T>(a,b);
        }

        public static Sequence<T> operator +(Sequence<T> a, T b)
        {
            return new Sequence<T>(a, b);
        }

        public static Sequence<T> operator +(T a, Sequence<T> b)
        {
            return new Sequence<T>(a, b);
        }

        public bool StartsWith(Sequence<T> value)
        {
            if(_data.Length < value._data.Length)
                return false;
            for(var i = 0; i < value._data.Length; i++)
                if(!Equals(_data[i], value._data[i]))
                    return false;
            return true;
        }
        public bool StartsWithAndNotEqual(Sequence<T> value)
        {
            if (_data.Length == value._data.Length)
                return false;
            return StartsWith(value);
        }

        public Sequence<ResultType> Apply<ResultType>(Func<T, IEnumerable<ResultType>> applyDelegate)
        {
            var result = new List<ResultType>();
            for (var i = 0; i < _data.Length; i++)
            {
                var t = _data[i];
                result.AddRange(applyDelegate(t));
            }
            return new Sequence<ResultType>(result);
        }

        public Result Serialize<Result>() 
            where Result : ICombiner<Result>, new()
        {
            var result = new Result();
            for (var i = 0; i < _data.Length; i++)
                result = result.CreateSequence(_data[i]);
            return result;
        }

        public Result Serialize<Result>(Result empty)
            where Result : ICombiner<Result>
        {
            var result = empty;
            for (var i = 0; i < _data.Length; i++)
                result = result.CreateSequence(_data[i]);
            return result;
        }

        public Result Serialize<Result>(Result empty, Func<Result, T, Result> combiner)
        {
            var result = empty;
            for (var i = 0; i < _data.Length; i++)
                result = combiner(result,_data[i]);
            return result;
        }

        public interface ICombiner<Result>
        {
            Result CreateSequence(T t);
        }

        public IEnumerator<T> GetEnumerator() { return new Enumerator(this, 0); }

        public class Enumerator : IEnumerator<T>
        {
            private readonly Sequence<T> _sequence;
            private int _index;

            public Enumerator(Sequence<T> sequence, int index)
            {
                _sequence = sequence;
                _index = index;
            }

            public void Dispose() {  }
            public bool MoveNext() { _index++; return _index < _sequence._data.Length; }
            public void Reset() { _index = 0; }
            public T Current { get { return _sequence._data[_index]; } }
            object IEnumerator.Current { get { return Current; } }
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}