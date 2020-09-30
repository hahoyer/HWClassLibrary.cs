using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Proof
{
    [PublicAPI]
    public sealed class Set<T> : IEnumerable<T>
        where T : IComparable<T>
    {
        [EnableDump]
        readonly List<T> Data;

        public Set()
            : this(new T[0]) { }

        Set(T[] ts) => Data = new List<T>(ts);

        /// <summary>
        ///     Returns true if the instance is empty.
        /// </summary>
        /// <value> <c>true</c> if this instance is empty; otherwise, <c>false</c> . </value>
        /// created 14.07.2007 16:43 on HAHOYER-DELL by hh
        [DisableDump]
        public bool IsEmpty => Count == 0;

        public static Set<T> Empty => new Set<T>();

        int Count => Data.Count;

        T this[int i] => Data[i];
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        /// <summary>
        ///     Adds an element.
        /// </summary>
        /// <param name="t"> The t. </param>
        /// created 14.07.2007 16:44 on HAHOYER-DELL by hh
        public void Add(T t)
        {
            if(Contains(t))
                return;
            Data.Add(t);
        }

        public bool Contains(T t) => Data.Any(t1 => t1.CompareTo(t) == 0);

        public static Set<T> Create(IEnumerable<T> data)
        {
            var result = new Set<T>();
            foreach(var t in data)
                result.Add(t);
            return result;
        }

        public static Set<T> operator &(Set<T> a, Set<T> b) => a.And(b);

        public static Set<T> operator |(Set<T> a, IEnumerable<T> b) => a.Or(b);
        public static Set<T> operator |(Set<T> a, T b) => a.Or(b.ToSet());
        public static Set<T> operator -(Set<T> a, T b) => a.Without(b.ToSet());

        Set<T> And(Set<T> other) => Data.Where(other.Contains).ToSet();

        Set<T> Or(IEnumerable<T> other)
        {
            var result = new Set<T>(Data.ToArray());
            foreach(var value in other)
                result.Add(value);
            return result;
        }

        Set<T> Without(Set<T> other) => Data.Where(target => !other.Contains(target)).ToSet();
    }

    public static class SetExtender
    {
        public static Set<T> ToSet<T>(this IEnumerable<T> target)
            where T : IComparable<T> => Set<T>.Create(target);

        public static Set<T> ToSet<T>(this T target)
            where T : IComparable<T> => Set<T>.Create(new[] {target});
    }
}