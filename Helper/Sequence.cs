using System;
using System.Collections;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    /// <summary>
    /// Arrays that support concatenation
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        internal Sequence(IList<T> a)
        {
            _data = new T[a.Count];
            for(int i = 0; i < a.Count; i++)
                _data[i] = a[i];
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Sequence<T> operator +(Sequence<T> a, Sequence<T> b)
        {
            return new Sequence<T>(a,b);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Sequence<T> operator +(Sequence<T> a, T b)
        {
            return new Sequence<T>(a, b);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Sequence<T> operator +(T a, Sequence<T> b)
        {
            return new Sequence<T>(a, b);
        }

        /// <summary>
        /// Checks if object starts with given object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool StartsWith(Sequence<T> value)
        {
            if(_data.Length < value._data.Length)
                return false;
            for(var i = 0; i < value._data.Length; i++)
                if(!Equals(_data[i], value._data[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Checks if object starts with given object and is longer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool StartsWithAndNotEqual(Sequence<T> value)
        {
            if (_data.Length == value._data.Length)
                return false;
            return StartsWith(value);
        }

        public Sequence<ResultType> Apply<ResultType>(Func<T, IEnumerable<ResultType>> applyDelegate)
        {
            var result = new List<ResultType>();
            for(var i = 0; i < _data.Length; i++)
            {
                var d = applyDelegate(_data[i]);
                if(d != null)
                    result.AddRange(d);
            }
            return new Sequence<ResultType>(result);
        }

        public Sequence<ResultType> Apply1<ResultType>(Func<T, ResultType> applyDelegate) where ResultType : class
        {
            var result = new List<ResultType>();
            for(var i = 0; i < _data.Length; i++)
            {
                var d = applyDelegate(_data[i]);
                if (d != null)
                    result.Add(d);
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

        public bool IsEmpty { get { return _data.Length == 0; } }

        public T[] ToArray() { return (T[]) _data.Clone(); }

        public interface ICombiner<Result>
        {
            Result CreateSequence(T t);
        }

        public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }

        public class Enumerator : IEnumerator<T>
        {
            private readonly Sequence<T> _parent;
            private int _index;

            public Enumerator(Sequence<T> sequence)
            {
                _parent = sequence;
                Reset();
            }

            public void Dispose() {  }
            public bool MoveNext() { _index++; return _index < _parent._data.Length; }
            public void Reset() { _index = -1; }
            public T Current { get { return _parent._data[_index]; } }
            object IEnumerator.Current { get { return Current; } }
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}