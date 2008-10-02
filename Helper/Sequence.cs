using System;
using System.Collections.Generic;

namespace HWClassLibrary.Helper
{
    [Serializable]
    public class Sequence<T>
    {
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

        public Sequence<ResultType> Apply<ResultType>(Func<T, ResultType> applyDelegate)
        {
            var result = new List<ResultType>();
            for (var i = 0; i < _data.Length; i++)
            {
                var t = _data[i];
                result.Add(applyDelegate(t));
            }
            return new Sequence<ResultType>(result);
        }

        public CombinedResultType Apply<CombinedResultType, ResultType>
            (
            Func<T, ResultType> applyDelegate,
            Func<CombinedResultType, ResultType, CombinedResultType> combineDelegate
                )
            where CombinedResultType : new()
        {
            var result = new CombinedResultType();
            for (var i = 0; i < _data.Length; i++)
            {
                var t = _data[i];
                result = combineDelegate(result, applyDelegate(t));
            }
            return result;
        }
    }
}