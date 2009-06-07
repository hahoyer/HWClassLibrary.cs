using System;
using System.Collections.Generic;
using System.Linq;

namespace HWClassLibrary.Helper
{
    public static class HWListExtender
    {
        public static bool AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual) { return InternalAddDistinct(a, b, isEqual); }
        public static bool AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, T> combine) where T : class { return InternalAddDistinct(a, b, combine); }

        private static bool InternalAddDistinct<T>(ICollection<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual)
        {
            var result = false;
            foreach (var bi in b)
                if(AddDistinct(a, bi, isEqual))
                    result = true;
            return result;
        }

        private static bool InternalAddDistinct<T>(IList<T> a, IEnumerable<T> b, Func<T, T, T> combine) where T : class
        {
            var result = false;
            foreach (var bi in b)
                if(AddDistinct(a, bi, combine))
                    result = true;
            return result;
        }

        private static bool AddDistinct<T>(ICollection<T> a, T bi, Func<T, T, bool> isEqual)
        {
            foreach (var ai in a)
                if (isEqual(ai, bi))
                    return false;
            a.Add(bi);
            return true;
        }

        private static bool AddDistinct<T>(IList<T> a, T bi, Func<T, T, T> combine) where T : class
        {
            for (var i = 0; i < a.Count; i++)
            {
                var ab = combine(a[i], bi);
                if (ab != null)
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
            return x.Aggregate(x.ToArray().Length.ToString(), (a, xx) => a + " " + xx.ToString());
        }
        public static string Format<T>(this IEnumerable<T> x, string separator)
        {
            var result = "";
            foreach (var element in x)
            {
                if (result != "")
                    result += separator;
                result += element.ToString();
            }
            return result;
        }

        public static TimeSpan Sum<T>(this IEnumerable<T> x, Func<T,TimeSpan> selector)
        {
            var result = new TimeSpan();
            foreach(var element in x)
                result += selector(element);
            return result;
        }
    }
}