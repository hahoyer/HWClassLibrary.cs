using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    public sealed class Set<T> : IEnumerable<T>
        where T : IComparable<T>
    {
        [IsDumpEnabled]
        private readonly List<T> _data;

        public Set()
            : this(new T[0]) { }

        private Set(T[] ts) { _data = new List<T>(ts); }

        private int Count { get { return _data.Count; } }

        /// <summary>
        ///     Returns true if the instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// created 14.07.2007 16:43 on HAHOYER-DELL by hh
        public bool IsEmpty { get { return Count == 0; } }

        public static Set<T> Empty { get { return new Set<T>(); } }

        /// <summary>
        ///     Adds an element.
        /// </summary>
        /// <param name = "t">The t.</param>
        /// created 14.07.2007 16:44 on HAHOYER-DELL by hh
        public void Add(T t)
        {
            if(Contains(t))
                return;
            _data.Add(t);
        }

        public bool Contains(T t)
        {
            for(var i = 0; i < _data.Count; i++)
            {
                if(_data[i].CompareTo(t) == 0)
                    return true;
            }
            return false;
        }

        private Set<T> And(Set<T> other) { return _data.Where(other.Contains).ToSet(); }

        private Set<T> Or(IEnumerable<T> other)
        {
            var result = new Set<T>(_data.ToArray());
            foreach(var value in other)
                result.Add(value);
            return result;
        }

        public static Set<T> Create(IEnumerable<T> data)
        {
            var result = new Set<T>();
            foreach(var t in data)
                result.Add(t);
            return result;
        }

        private Set<T> Without(Set<T> other) { return _data.Where(x => !other.Contains(x)).ToSet(); }

        private T this[int i] { get { return _data[i]; } }

        public static Set<T> operator &(Set<T> a, Set<T> b) { return a.And(b); }

        public static Set<T> operator |(Set<T> a, IEnumerable<T> b) { return a.Or(b); }
        public static Set<T> operator |(Set<T> a, T b) { return a.Or(b.ToSet()); }
        public static Set<T> operator -(Set<T> a, T b) { return a.Without(b.ToSet()); }

        public IEnumerator<T> GetEnumerator() { return _data.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }

    public static class SetExtender
    {
        public static Set<T> ToSet<T>(this IEnumerable<T> x) where T : IComparable<T> { return Set<T>.Create(x); }
        public static Set<T> ToSet<T>(this T x) where T : IComparable<T> { return Set<T>.Create(new[] {x}); }
    }
}