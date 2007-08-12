using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    public class Set<T>
    {
        private List<T> _data;

        public delegate bool IsEqualDelegate(T a, T b);
        public delegate ResultType ApplyDelegate<ResultType>(T x);
        public delegate CombinedResultType 
            CombineDelegate<CombinedResultType, ResultType>
            (CombinedResultType combinedResultType, ResultType resultType);

        private IsEqualDelegate _isEqual = new IsEqualDelegate(delegate(T a, T b) { return a.Equals(b); });

        [DumpData(false)]
        public IsEqualDelegate IsEqual
        {
            get { return _isEqual; } 
            set
            {
                Tracer.Assert(Count <= 1);
                _isEqual = value;
            }
        }

        public Set()
        {
            _data = new List<T>();
        }

        private Set(T[] ts)
        {
            _data = new List<T>(ts);
        }

        private int Count { get { return _data.Count; } }

        /// <summary>
        /// Returns true if the instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// created 14.07.2007 16:43 on HAHOYER-DELL by hh
        public bool IsEmpty { get { return Count == 0; } }

        /// <summary>
        /// Adds an element. 
        /// </summary>
        /// <param name="t">The t.</param>
        /// created 14.07.2007 16:44 on HAHOYER-DELL by hh
        public void Add(T t)
        {
            if(Contains(t))
                return;
            _data.Add(t);
        }

        private bool Contains(T t)
        {
            for (int i = 0; i < _data.Count; i++)
                if (_isEqual(_data[i], t))
                    return true;
            return false;
        }

        private Set<T> And(Set<T> other)
        {
            Set<T> result = new Set<T>();
            for (int i = 0; i < _data.Count; i++)
                if (other.Contains(_data[i]))
                    result._data.Add(_data[i]);
            return result;
        }

        private Set<T> Or(Set<T> other)
        {
            Set<T> result = new Set<T>(_data.ToArray());
            for (int i = 0; i < other.Count; i++)
                result.Add(other[i]);
            return result;
        }

        /// <summary>
        /// Creates the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// created 09.08.2007 23:50 on HAHOYER-DELL by hh
        static public Set<T> Create(T[] data)
        {
            Set<T> result = new Set<T>();
            for (int i = 0; i < data.Length; i++)
                result.Add(data[i]);
            return result;
        }

        private T this[int i] { get { return _data[i]; } }

        static public Set<T> operator &(Set<T> a, Set<T> b) 
        {
            return a.And(b);
        }

        static public Set<T> operator |(Set<T> a, Set<T> b)
        {
            return a.Or(b);
        }

        public List<ResultType> Apply<ResultType>(ApplyDelegate<ResultType> applyDelegate)
        {
            List<ResultType> result = new List<ResultType>();
            for (int i = 0; i < _data.Count; i++)
            {
                T t = _data[i];
                result.Add(applyDelegate(t));
            }
            return result;
        }
        public CombinedResultType Apply<CombinedResultType, ResultType>(ApplyDelegate<ResultType> applyDelegate, CombineDelegate<CombinedResultType, ResultType> combineDelegate) where CombinedResultType : new()
        {
            CombinedResultType result = new CombinedResultType();
            for (int i = 0; i < _data.Count; i++)
            {
                T t = _data[i];
                result = combineDelegate(result, applyDelegate(t));
            }
            return result;
        }
    }
}