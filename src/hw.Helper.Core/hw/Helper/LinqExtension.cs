using System.Diagnostics;
using System.Text;
using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public static class LinqExtension
{
    public enum SeparatorTreatmentForSplit { Drop, BeginOfSubList, EndOfSubList }

    extension<T>(IList<T> a)
    {
        public bool AddDistinct(IEnumerable<T> b, Func<T, T, bool> isEqual)
            => InternalAddDistinct(a, b, isEqual);
    }

    extension<T>(IList<T> a)
        where T : class
    {
        public bool AddDistinct(IEnumerable<T> b, Func<T, T, T> combine)
            => InternalAddDistinct(a, b, combine);
    }

    extension<T>(IEnumerable<T> target)
    {
        public IEnumerable<IEnumerable<T>> Separate(Func<T, bool> isHead)
        {
            var subResult = new List<T>();

            foreach(var xx in target)
            {
                if(isHead(xx))
                    if(subResult.Count > 0)
                    {
                        yield return subResult.ToArray();
                        subResult = [];
                    }

                subResult.Add(xx);
            }

            if(subResult.Count > 0)
                yield return subResult.ToArray();
        }

        public string Dump() => Tracer.Dump(target);

        public string Stringify(string separator, bool showNumbers = false)
        {
            var result = new StringBuilder();
            var i = 0;
            var isNext = false;
            foreach(var element in target)
            {
                if(isNext)
                    result.Append(separator);
                if(showNumbers)
                    result.Append("[" + i + "] ");
                isNext = true;
                result.Append(element);
                i++;
            }

            return result.ToString();
        }

        public TimeSpan Sum(Func<T, TimeSpan> selector)
        {
            var result = new TimeSpan();
            return target.Aggregate(result, (current, element) => current + selector(element));
        }

        /// <summary>
        ///     Splits an enumeration at positions where <see cref="isSeparator" /> returns true.
        ///     The resulting enumeration of enumerations may contain the separator item
        ///     depending on <see cref="separatorTreatment" /> parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isSeparator"></param>
        /// <param name="separatorTreatment"></param>
        /// <returns>
        ///     Enumeration of arrays of <see cref="target" /> items
        ///     split at points where <see cref="isSeparator" /> returns true.
        /// </returns>
        public IEnumerable<IEnumerable<T>> Split
        (
            Func<T, bool> isSeparator
            , SeparatorTreatmentForSplit separatorTreatment = SeparatorTreatmentForSplit.Drop
        )
        {
            var part = new List<T>();
            foreach(var item in target)
                if(isSeparator(item))
                {
                    if(separatorTreatment == SeparatorTreatmentForSplit.EndOfSubList)
                        part.Add(item);

                    if(part.Any())
                        yield return part.ToArray();
                    part = [];

                    if(separatorTreatment == SeparatorTreatmentForSplit.BeginOfSubList)
                        part.Add(item);
                }
                else
                    part.Add(item);

            if(part.Any())
                yield return part.ToArray();
        }
    }

    extension<T>(IEnumerable<T?> target)
        where T : class, IAggregateable<T>
    {
        public T? Aggregate(Func<T>? getDefault = null)
        {
            var targetArray = target.ToArray();
            if(!targetArray.Any())
                return getDefault?.Invoke();
            var result = targetArray[0];
            for(var i = 1; i < targetArray.Length; i++)
            {
                var item = targetArray[i];
                result = result?.Aggregate(item) ?? item;
            }

            return result;
        }
    }

    extension<T>(IEnumerable<T> target)
        where T : Dumpable
    {
        public string DumpLines()
        {
            var i = 0;
            return target.Aggregate("", (a, xx) => a + "[" + i++ + "] " + xx.Dump() + "\n");
        }
    }

    extension<T>(IEnumerable<T> list)
    {
        /// <summary>
        ///     Returns index list of all elements, that have no other element, with "isInRelation(element, other)" is true
        ///     For example if relation is "element ;&lt; other" will return the maximal element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isInRelation"></param>
        /// <returns></returns>
        public IEnumerable<int> FrameIndexList(Func<T, T, bool> isInRelation)
        {
            var listArray = list.ToArray();
            return
                listArray.Select((item, index) => new Tuple<T, int>(item, index))
                    .Where(element => !listArray.Any(other => isInRelation(element.Item1, other)))
                    .Select(element => element.Item2);
        }

        /// <summary>
        ///     Returns list of all elements, that have no other element, with "isInRelation(element, other)" is true
        ///     For example if relation is "element &lt; other" will return the maximal element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isInRelation"></param>
        /// <returns></returns>
        public IEnumerable<T> FrameElementList(Func<T, T, bool> isInRelation)
        {
            var listArray = list.ToArray();
            return listArray.FrameIndexList(isInRelation).Select(index => listArray[index]);
        }
    }                                                                   

    extension<T>(IEnumerable<T> list)
        where T : IComparable<T>
    {
        public IEnumerable<int> GetMaxIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) < 0);

        public IEnumerable<int> GetMinIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) > 0);

        [Obsolete("use GetMaxIndexList",false)]
        public IEnumerable<int> MaxIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) < 0);

        [Obsolete("use GetMinIndexList",false)]
        public IEnumerable<int> MinIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) > 0);
    }

    extension<T>(IList<T> target)
    {
        /// <summary>
        ///     Checks if object starts with given object.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="y"> The y. </param>
        /// <returns> </returns>
        public bool StartsWith(IList<T> y)
        {
            if(target.Count < y.Count)
                return false;
            return !y.Where((t, i) => !Equals(target[i], t)).Any();
        }

        /// <summary>
        ///     Checks if object starts with given object and is longer.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="y"> The y. </param>
        /// <returns> </returns>
        public bool StartsWithAndNotEqual(IList<T> y)
            => target.Count != y.Count && target.StartsWith(y);
    }

    extension<T>(T? target)
        where T : class
    {
        public TResult? CheckedApply<TResult>(Func<T, TResult> function)
            where TResult : class
            => target == default(T)? default : function(target);

        public IEnumerable<T> NullableToArray()
            => target == null? [] : new[] { target };
    }

    extension<TResult>(TResult? target)
        where TResult : struct
    {
        public TResult AssertValue()
        {
            (target != null).Assert(stackFrameDepth: 1);
            return target!.Value;
        }
    }

    extension<TResult>(TResult? target)
        where TResult : class
    {
        [DebuggerHidden]
        [ContractAnnotation("target: null => halt")]
        public TResult AssertNotNull(int stackFrameDepth = 0)
        {
            (target != null).Assert(stackFrameDepth: stackFrameDepth + 1);
            return target!;
        }
    }

    extension(int count)
    {
        public IEnumerable<int> Select()
        {
            for(var i = 0; i < count; i++)
                yield return i;
        }


        public IEnumerable<T> Select<T>(Func<int, T> getValue)
        {
            for(var i = 0; i < count; i++)
                yield return getValue(i);
        }
    }

    extension(long count)
    {
        public IEnumerable<long> Select()
        {
            for(long i = 0; i < count; i++)
                yield return i;
        }

        public IEnumerable<T> Select<T>(Func<long, T> getValue)
        {
            for(long i = 0; i < count; i++)
                yield return getValue(i);
        }
    }

    extension<TKey, TValue>(IDictionary<TKey, TValue> target)
    {
        public void AddRange
        (
            IEnumerable<KeyValuePair<TKey, TValue>> newEntries
        )
        {
            foreach(var item in newEntries.Where(entry => !target.ContainsKey(entry.Key)))
                target.Add(item);
        }
    }

    extension<T>(IEnumerable<T> items)
    {
        /// <summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        /// <param name="predicate">The expression to test the items against.</param>
        /// <returns>The index of the first matching item, or null if no items match.</returns>
        public int? IndexWhere(Func<T, bool> predicate)
        {
            if(items == null)
                throw new ArgumentNullException(nameof(items));
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var result = 0;
            foreach(var item in items)
            {
                if(predicate(item))
                    return result;
                result++;
            }

            return null;
        }
    }

    extension<T>(T? current)
        where T : class
    {
        public IEnumerable<T> Chain(Func<T, T?> getNext)
        {
            while(current != null)
            {
                yield return current;
                current = getNext(current);
            }
        }
    }

    extension<T>(T a)
    {
        public bool In(params T[] b) => b.Contains(a);
    }

    extension<TType>(IEnumerable<TType> target)
    {
        public IEnumerable<TType>? Sort(Func<TType, IEnumerable<TType>> immediateParents)
        {
            var xx = target.ToArray();
            xx.IsCircuitFree(immediateParents).Assert();
            return null;
        }

        public IEnumerable<TType> Closure(Func<TType, IEnumerable<TType>> immediateParents)
        {
            var types = target.ToArray();
            var targets = types;
            while(true)
            {
                targets = targets.SelectMany(immediateParents).Except(types).ToArray();
                if(!targets.Any())
                    return types;
                types = types.Union(targets).ToArray();
            }
        }

        public bool IsCircuitFree(Func<TType, IEnumerable<TType>> immediateParents)
            => target.All(item => item.IsCircuitFree(immediateParents));

        public IEnumerable<TType> Circuits(Func<TType, IEnumerable<TType>> immediateParents)
            => target.Where(item => !item.IsCircuitFree(immediateParents));
    }

    extension<TType>(TType target)
    {
        public bool IsCircuitFree(Func<TType, IEnumerable<TType>> immediateParents)
            => immediateParents(target).Closure(immediateParents).All(item => !Equals(item, target));
    }

    extension<T>(T? target)
        where T : struct
    {
        public IEnumerable<T> NullableToArray()
            => target == null? [] : new[] { target.Value };
    }

    extension<TTarget>(IEnumerable<TTarget> target)
    {
        public TTarget? Top
        (
            Func<TTarget, bool>? selector = null,
            Func<Exception>? emptyException = null,
            Func<IEnumerable<TTarget>, Exception>? multipleException = null,
            bool enableEmpty = true,
            bool enableMultiple = true
        )
        {
            if(selector != null)
                target = target.Where(selector);

            using var enumerator = target.GetEnumerator();

            if(!enumerator.MoveNext())
            {
                if(emptyException != null)
                    throw emptyException();
                return enableEmpty? default : target.Single();
            }

            var result = enumerator.Current;
            if(!enumerator.MoveNext())
                return result;

            if(multipleException != null)
                throw multipleException(target);
            return enableMultiple? result : target.Single();
        }
    }

    extension(IEnumerable<int> values)
    {
        public int? Maxx
        {
            get
            {
                if(values == null)
                    throw new ArgumentNullException(nameof(values));
                int? result = null;
                foreach(var value in values)
                    if(result == null)
                        result = value;
                    else if(value > result)
                        result = value;
                return result;
            }
        }

        public int? Minn
        {
            get
            {
                if(values == null)
                    throw new ArgumentNullException(nameof(values));
                int? result = null;
                foreach(var value in values)
                    if(result == null)
                        result = value;
                    else if(value < result)
                        result = value;
                return result;
            }
        }
    }

    extension<T>(T root)
    {
        public IEnumerable<T> SelectHierarchical(Func<T, IEnumerable<T>> getChildren)
        {
            yield return root;
            foreach(var item in getChildren(root).SelectMany(i => i.SelectHierarchical(getChildren)))
                yield return item;
        }
    }

    public static IEnumerable<int> Where(Func<int, bool> getValue)
    {
        for(var i = 0; getValue(i); i++)
            yield return i;
    }

    public static IEnumerable<Tuple<TKey, TLeft?, TRight?>> Merge<TKey, TLeft, TRight>
    (
        this IEnumerable<TLeft> left,
        IEnumerable<TRight> right,
        Func<TLeft, TKey> getLeftKey,
        Func<TRight, TKey> getRightKey
    )
        where TLeft : class
        where TRight : class
    {
        var leftCommon = left.Select
            (l => new Tuple<TKey, TLeft?, TRight?>(getLeftKey(l), l, null));
        var rightCommon = right.Select
            (r => new Tuple<TKey, TLeft?, TRight?>(getRightKey(r), null, r));
        return
            leftCommon.Union(rightCommon)
                .GroupBy(t => t.Item1)
                .Select(Merge);
    }

    public static IEnumerable<Tuple<TKey, T?, T?>> Merge<TKey, T>
        (this IEnumerable<T> left, IEnumerable<T> right, Func<T, TKey> getKey)
        where T : class
        => left.Merge(right, getKey, getKey);

    public static Tuple<TKey, TLeft?, TRight?> Merge<TKey, TLeft, TRight>
        (IGrouping<TKey, Tuple<TKey, TLeft?, TRight?>> grouping)
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
                    return new(grouping.Key, list[1].Item2, list[0].Item3);
                if(list[1].Item2 == null && list[0].Item3 == null)
                    return new(grouping.Key, list[0].Item2, list[1].Item3);
                break;
        }

        throw new DuplicateKeyException();
    }

    public static FunctionCache<TKey, IEnumerable<T>> ToDictionaryEx<TKey, T>
        (this IEnumerable<T> list, Func<T, TKey> selector)
        where TKey : notnull
        => new(key => list.Where(item => Equals(selector(item), key)));

    static bool InternalAddDistinct<T>(ICollection<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual)
    {
        var result = false;
        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach(var bi in b)
            if(AddDistinct(a, bi, isEqual))
                result = true;
        return result;
    }

    static bool InternalAddDistinct<T>(IList<T> a, IEnumerable<T> b, Func<T, T, T> combine)
        where T : class
    {
        var result = false;
        // ReSharper disable once LoopCanBePartlyConvertedToQuery
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

    static bool AddDistinct<T>(IList<T> a, T bi, Func<T, T, T?> combine)
        where T : class
    {
        for(var i = 0; i < a.Count; i++)
        {
            var ab = combine(a[i], bi);
            if(ab == null)
                continue;
            a[i] = ab;
            return false;
        }

        a.Add(bi);
        return true;
    }

    public static IEnumerable<T> ConcatMany<T>(this IEnumerable<IEnumerable<T>?> target)
        => target
            .Where(i => i != null)
            .SelectMany(i => i!);
}

// ReSharper disable once IdentifierTypo
public interface IAggregateable<T>
{
    T Aggregate(T? other);
}

sealed class DuplicateKeyException : Exception;