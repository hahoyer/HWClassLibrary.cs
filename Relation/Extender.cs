using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Relation
{
    public static class Extender
    {
        public static IEnumerable<TType> Sort<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents)
        {
            var xx = x.ToArray();
            Tracer.Assert(xx.IsCircuidFree(immediateParents));
            return null;
        }

        public static IEnumerable<TType> Closure<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents)
        {
            var types = x.ToArray();
            var targets = types;
            while(true)
            {
                targets = targets.SelectMany(immediateParents).Except(types).ToArray();
                if(targets.Count() == 0)
                    return types;
                types = types.Union(targets).ToArray();
            }
        }

        public static bool IsCircuidFree<TType>(this TType x, Func<TType, IEnumerable<TType>> immediateParents) { return immediateParents(x).Closure(immediateParents).All(xx => !xx.Equals(x)); }
        public static bool IsCircuidFree<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents) { return x.All(xx => xx.IsCircuidFree(immediateParents)); }

        public static IEnumerable<TType> Circuids<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents) { return x.Where(xx => !xx.IsCircuidFree(immediateParents)); }
    }
}