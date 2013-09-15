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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    public sealed class Set<T> : IEnumerable<T>
        where T : IComparable<T>
    {
        [EnableDump]
        readonly List<T> _data;

        public Set()
            : this(new T[0]) { }

        Set(T[] ts) { _data = new List<T>(ts); }

        int Count { get { return _data.Count; } }

        /// <summary>
        ///     Returns true if the instance is empty.
        /// </summary>
        /// <value> <c>true</c> if this instance is empty; otherwise, <c>false</c> . </value>
        /// created 14.07.2007 16:43 on HAHOYER-DELL by hh
        [DisableDump]
        public bool IsEmpty { get { return Count == 0; } }

        public static Set<T> Empty { get { return new Set<T>(); } }

        /// <summary>
        ///     Adds an element.
        /// </summary>
        /// <param name="t"> The t. </param>
        /// created 14.07.2007 16:44 on HAHOYER-DELL by hh
        public void Add(T t)
        {
            if(Contains(t))
                return;
            _data.Add(t);
        }

        public bool Contains(T t) { return _data.Any(t1 => t1.CompareTo(t) == 0); }

        Set<T> And(Set<T> other) { return _data.Where(other.Contains).ToSet(); }

        Set<T> Or(IEnumerable<T> other)
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

        Set<T> Without(Set<T> other) { return _data.Where(x => !other.Contains(x)).ToSet(); }

        T this[int i] { get { return _data[i]; } }

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