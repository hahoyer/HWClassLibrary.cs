#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace HWClassLibrary.Helper
{
    public static class ListExtender
    {
        public static bool AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual) { return InternalAddDistinct(a, b, isEqual); }
        public static bool AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, T> combine) where T : class { return InternalAddDistinct(a, b, combine); }

        static bool InternalAddDistinct<T>(ICollection<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual)
        {
            var result = false;
            foreach(var bi in b)
                if(AddDistinct(a, bi, isEqual))
                    result = true;
            return result;
        }

        static bool InternalAddDistinct<T>(IList<T> a, IEnumerable<T> b, Func<T, T, T> combine) where T : class
        {
            var result = false;
            foreach(var bi in b)
                if(AddDistinct(a, bi, combine))
                    result = true;
            return result;
        }

        static bool AddDistinct<T>(ICollection<T> a, T bi, Func<T, T, bool> isEqual)
        {
            if(a.Any(ai => isEqual(ai, bi)))
                return false;
            a.Add(bi);
            return true;
        }

        static bool AddDistinct<T>(IList<T> a, T bi, Func<T, T, T> combine) where T : class
        {
            for(var i = 0; i < a.Count; i++)
            {
                var ab = combine(a[i], bi);
                if(ab != null)
                {
                    a[i] = ab;
                    return false;
                }
            }
            a.Add(bi);
            return true;
        }

        public static string Dump<T>(this IEnumerable<T> x)
        {
            var xArray = x.ToArray();
            return xArray.Aggregate(xArray.Length.ToString(), (a, xx) => a + " " + xx.ToString());
        }

        public static string DumpLines<T>(this IEnumerable<T> x)
            where T : Dumpable
        {
            var i = 0;
            return x.Aggregate("", (a, xx) => a + "[" + i++ + "] " + xx.Dump() + "\n");
        }

        [Obsolete("use Stringify")]
        public static string Format<T>(this IEnumerable<T> x, string separator) { return Stringify(x, separator); }

        public static string Stringify<T>(this IEnumerable<T> x, string separator)
        {
            var result = new StringBuilder();
            var isNext = false;
            foreach(var element in x)
            {
                if(isNext)
                    result.Append(separator);
                isNext = true;
                result.Append(element);
            }
            return result.ToString();
        }

        public static TimeSpan Sum<T>(this IEnumerable<T> x, Func<T, TimeSpan> selector)
        {
            var result = new TimeSpan();
            return x.Aggregate(result, (current, element) => current + selector(element));
        }

        /// <summary>
        /// Returns index list of all elements, that have no other element, with "isInRelation(element, other)" is true 
        /// For example if relation is "element ;&less; other" will return the maximal element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="isInRelation"></param>
        /// <returns></returns>
        public static IEnumerable<int> FrameIndexList<T>(this IEnumerable<T> list, Func<T, T, bool> isInRelation)
        {
            var listArray = list.ToArray();
            return listArray
                .Select((item, index) => new Tuple<T, int>(item, index))
                .Where(element => !listArray.Any(other => isInRelation(element.Item1, other)))
                .Select(element=>element.Item2);

        }
        /// <summary>
        /// Returns list of all elements, that have no other element, with "isInRelation(element, other)" is true 
        /// For example if relation is "element ;&less; other" will return the maximal element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="isInRelation"></param>
        /// <returns></returns>
        public static IEnumerable<T> FrameElementList<T>(this IEnumerable<T> list, Func<T, T, bool> isInRelation)
        {
            var listArray = list.ToArray();
            return listArray
                .FrameIndexList(isInRelation)
                .Select(index=>listArray[index]);
        }

        public static IEnumerable<int> MaxIndexList<T>(this IEnumerable<T> list)
            where T : IComparable<T> { return list.FrameIndexList((a, b) => a.CompareTo(b) < 0); }
        public static IEnumerable<int> MinIndexList<T>(this IEnumerable<T> list)
            where T : IComparable<T> { return list.FrameIndexList((a, b) => a.CompareTo(b) > 0); }

        /// <summary>
        ///     Checks if object starts with given object.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="x"> The x. </param>
        /// <param name="y"> The y. </param>
        /// <returns> </returns>
        public static bool StartsWith<T>(this IList<T> x, IList<T> y)
        {
            if(x.Count < y.Count)
                return false;
            return !y.Where((t, i) => !Equals(x[i], t)).Any();
        }

        /// <summary>
        ///     Checks if object starts with given object and is longer.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="x"> The x. </param>
        /// <param name="y"> The y. </param>
        /// <returns> </returns>
        public static bool StartsWithAndNotEqual<T>(this IList<T> x, IList<T> y)
        {
            if(x.Count == y.Count)
                return false;
            return x.StartsWith(y);
        }

        public static TResult CheckedApply<T, TResult>(this T target, Func<T, TResult> function)
            where T : class
            where TResult : class { return target == default(T) ? default(TResult) : function(target); }

        [NotNull]
        public static IEnumerable<T> Array<T>(this int count, Func<int, T> getValue) { return new ArrayQuery<T>(count, getValue); }

        sealed class ArrayQuery<T> : IEnumerable<T>
        {
            readonly int _count;
            readonly Func<int, T> _getValue;
            public ArrayQuery(int count, Func<int, T> getValue)
            {
                _count = count;
                _getValue = getValue;
            }

            IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<T>) this).GetEnumerator(); }
            IEnumerator<T> IEnumerable<T>.GetEnumerator() { return new Enumerator(this); }

            sealed class Enumerator : IEnumerator<T>
            {
                readonly ArrayQuery<T> _arrayQuery;
                int? _index;
                public Enumerator(ArrayQuery<T> arrayQuery) { _arrayQuery = arrayQuery; }

                void IDisposable.Dispose() { }
                bool IEnumerator.MoveNext()
                {
                    if(_index == null)
                        _index = 0;
                    else
                        _index++;

                    return _index < _arrayQuery._count;
                }

                void IEnumerator.Reset() { _index = null; }
// ReSharper disable PossibleInvalidOperationException
                T IEnumerator<T>.Current { get { return _arrayQuery._getValue(_index.Value); } } // should throw an exception if _index is null
// ReSharper restore PossibleInvalidOperationException
                object IEnumerator.Current { get { return ((IEnumerator<T>) this).Current; } }
            }
        }

        public static IEnumerable<Tuple<TKey, TLeft, TRight>> Merge<TKey, TLeft, TRight>(this IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, TKey> getLeftKey, Func<TRight, TKey> getRightKey)
            where TLeft : class
            where TRight : class
        {
            var leftCommon = left.Select(l => new Tuple<TKey, TLeft, TRight>(getLeftKey(l), l, null));
            var rightCommon = right.Select(r => new Tuple<TKey, TLeft, TRight>(getRightKey(r), null, r));
            return leftCommon.Union(rightCommon).GroupBy(t => t.Item1).Select(Merge);
        }

        public static Tuple<TKey, TLeft, TRight> Merge<TKey, TLeft, TRight>(IGrouping<TKey, Tuple<TKey, TLeft, TRight>> grouping)
            where TLeft : class
            where TRight : class
        {
            var list = grouping.ToArray();
            switch(list.Length)
            {
                case 1:
                    return list[0];
                case 2:
                    if(list[0].Item2 == null && list[1].Item3 == null)
                        return new Tuple<TKey, TLeft, TRight>(grouping.Key, list[1].Item2, list[0].Item3);
                    if(list[1].Item2 == null && list[0].Item3 == null)
                        return new Tuple<TKey, TLeft, TRight>(grouping.Key, list[0].Item2, list[1].Item3);
                    break;
            }
            throw new DuplicateKeyException();
        }
    }

    sealed class DuplicateKeyException : Exception
    {}
}