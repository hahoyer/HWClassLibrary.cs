using System;
using System.Collections.Generic;

namespace HWClassLibrary.Helper
{
    public static class HWListExtender
    {
        public static void AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual) { InternalAddDistinct(a, b, isEqual); }
        public static void AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, T> combine) where T : class { InternalAddDistinct(a, b, combine); }

        private static void InternalAddDistinct<T>(ICollection<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual)
        {
            foreach (var bi in b)
                AddDistinct(a, bi, isEqual);
        }

        private static void InternalAddDistinct<T>(IList<T> a, IEnumerable<T> b, Func<T, T, T> combine) where T : class
        {
            foreach (var bi in b)
                AddDistinct(a, bi, combine);
        }

        private static void AddDistinct<T>(ICollection<T> a, T bi, Func<T, T, bool> isEqual)
        {
            foreach (var ai in a)
            {
                if (isEqual(ai, bi))
                    return;
            }
            a.Add(bi);
        }

        private static void AddDistinct<T>(IList<T> a, T bi, Func<T, T, T> combine) where T : class
        {
            for (var i = 0; i < a.Count; i++)
            {
                var ab = combine(a[i], bi);
                if (ab != null)
                {
                    a[i] = ab;
                    return;
                }
            }
            a.Add(bi);
        }
    }
}