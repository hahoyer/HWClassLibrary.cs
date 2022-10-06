using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public static class LinqExtension
{
    public enum SeparatorTreatmentForSplit { Drop, BeginOfSubList, EndOfSubList }

    public static bool AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual)
        => InternalAddDistinct(a, b, isEqual);

    public static bool AddDistinct<T>(this IList<T> a, IEnumerable<T> b, Func<T, T, T> combine)
        where T : class
        => InternalAddDistinct(a, b, combine);

    public static IEnumerable<IEnumerable<T>> Separate<T>(this IEnumerable<T> target, Func<T, bool> isHead)
    {
        var subResult = new List<T>();

        foreach(var xx in target)
        {
            if(isHead(xx))
                if(subResult.Count > 0)
                {
                    yield return subResult.ToArray();
                    subResult = new();
                }

            subResult.Add(xx);
        }

        if(subResult.Count > 0)
            yield return subResult.ToArray();
    }

    [CanBeNull]
    public static T Aggregate<T>(this IEnumerable<T> target, Func<T> getDefault = null)
        where T : class, IAggregateable<T>
    {
        var xx = target.ToArray();
        if(!xx.Any())
            return getDefault?.Invoke();
        var result = xx[0];
        for(var i = 1; i < xx.Length; i++)
            result = result.Aggregate(xx[i]);
        return result;
    }

    public static string Dump<T>(this IEnumerable<T> target) => Tracer.Dump(target);

    public static string DumpLines<T>(this IEnumerable<T> target)
        where T : Dumpable
    {
        var i = 0;
        return target.Aggregate("", (a, xx) => a + "[" + i++ + "] " + xx.Dump() + "\n");
    }

    public static string Stringify<T>(this IEnumerable<T> target, string separator, bool showNumbers = false)
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

    public static TimeSpan Sum<T>(this IEnumerable<T> target, Func<T, TimeSpan> selector)
    {
        var result = new TimeSpan();
        return target.Aggregate(result, (current, element) => current + selector(element));
    }

    /// <summary>
    ///     Returns index list of all elements, that have no other element, with "isInRelation(element, other)" is true
    ///     For example if relation is "element ;&lt; other" will return the maximal element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="isInRelation"></param>
    /// <returns></returns>
    public static IEnumerable<int> FrameIndexList<T>(this IEnumerable<T> list, Func<T, T, bool> isInRelation)
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
    /// <param name="list"></param>
    /// <param name="isInRelation"></param>
    /// <returns></returns>
    public static IEnumerable<T> FrameElementList<T>(this IEnumerable<T> list, Func<T, T, bool> isInRelation)
    {
        var listArray = list.ToArray();
        return listArray.FrameIndexList(isInRelation).Select(index => listArray[index]);
    }

    public static IEnumerable<int> MaxIndexList<T>(this IEnumerable<T> list)
        where T : IComparable<T>
        => list.FrameIndexList((a, b) => a.CompareTo(b) < 0);

    public static IEnumerable<int> MinIndexList<T>(this IEnumerable<T> list)
        where T : IComparable<T>
        => list.FrameIndexList((a, b) => a.CompareTo(b) > 0);

    /// <summary>
    ///     Checks if object starts with given object.
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="target"> The target. </param>
    /// <param name="y"> The y. </param>
    /// <returns> </returns>
    public static bool StartsWith<T>(this IList<T> target, IList<T> y)
    {
        if(target.Count < y.Count)
            return false;
        return !y.Where((t, i) => !Equals(target[i], t)).Any();
    }

    /// <summary>
    ///     Checks if object starts with given object and is longer.
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    /// <param name="target"> The target. </param>
    /// <param name="y"> The y. </param>
    /// <returns> </returns>
    public static bool StartsWithAndNotEqual<T>(this IList<T> target, IList<T> y)
        => target.Count != y.Count && target.StartsWith(y);

    public static TResult CheckedApply<T, TResult>(this T target, Func<T, TResult> function)
        where T : class
        where TResult : class => target == default(T)? default : function(target);

    public static TResult AssertValue<TResult>(this TResult? target)
        where TResult : struct
    {
        (target != null).Assert();
        return target.Value;
    }

    [ContractAnnotation("=> notnull")]
    public static TResult AssertNotNull<TResult>(this TResult target)
        where TResult : class
    {
        (target != null).Assert();
        return target;
    }


    [NotNull]
    public static IEnumerable<int> Select(this int count)
    {
        for(var i = 0; i < count; i++)
            yield return i;
    }

    [NotNull]
    public static IEnumerable<long> Select(this long count)
    {
        for(long i = 0; i < count; i++)
            yield return i;
    }

    [NotNull]
    public static IEnumerable<int> Where(Func<int, bool> getValue)
    {
        for(var i = 0; getValue(i); i++)
            yield return i;
    }


    [NotNull]
    public static IEnumerable<T> Select<T>(this int count, Func<int, T> getValue)
    {
        for(var i = 0; i < count; i++)
            yield return getValue(i);
    }

    [NotNull]
    public static IEnumerable<T> Select<T>(this long count, Func<long, T> getValue)
    {
        for(long i = 0; i < count; i++)
            yield return getValue(i);
    }

    public static IEnumerable<Tuple<TKey, TLeft, TRight>> Merge<TKey, TLeft, TRight>
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
            (l => new Tuple<TKey, TLeft, TRight>(getLeftKey(l), l, null));
        var rightCommon = right.Select
            (r => new Tuple<TKey, TLeft, TRight>(getRightKey(r), null, r));
        return
            leftCommon.Union(rightCommon)
                .GroupBy(t => t.Item1)
                .Select
                    (Merge);
    }

    public static IEnumerable<Tuple<TKey, T, T>> Merge<TKey, T>
        (this IEnumerable<T> left, IEnumerable<T> right, Func<T, TKey> getKey)
        where T : class
        => Merge(left, right, getKey, getKey);

    public static Tuple<TKey, TLeft, TRight> Merge<TKey, TLeft, TRight>
        (IGrouping<TKey, Tuple<TKey, TLeft, TRight>> grouping)
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
        => new(key => list.Where(item => Equals(selector(item), key)));

    public static void AddRange<TKey, TValue>
    (
        this IDictionary<TKey, TValue> target,
        IEnumerable<KeyValuePair<TKey, TValue>> newEntries
    )
    {
        foreach(var item in newEntries.Where(entry => !target.ContainsKey(entry.Key)))
            target.Add(item);
    }

    /// <summary>Finds the index of the first item matching an expression in an enumerable.</summary>
    /// <param name="items">The enumerable to search.</param>
    /// <param name="predicate">The expression to test the items against.</param>
    /// <returns>The index of the first matching item, or null if no items match.</returns>
    public static int? IndexWhere<T>(this IEnumerable<T> items, Func<T, bool> predicate)
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

    public static IEnumerable<T> Chain<T>(this T current, Func<T, T> getNext)
        where T : class
    {
        while(current != null)
        {
            yield return current;
            current = getNext(current);
        }
    }

    public static bool In<T>(this T a, params T[] b) => b.Contains(a);

    public static IEnumerable<TType> Sort<TType>
        (this IEnumerable<TType> target, Func<TType, IEnumerable<TType>> immediateParents)
    {
        var xx = target.ToArray();
        xx.IsCircuitFree(immediateParents).Assert();
        return null;
    }

    public static IEnumerable<TType> Closure<TType>
        (this IEnumerable<TType> target, Func<TType, IEnumerable<TType>> immediateParents)
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

    public static bool IsCircuitFree<TType>(this TType target, Func<TType, IEnumerable<TType>> immediateParents)
        => immediateParents(target).Closure(immediateParents).All(item => !item.Equals(target));

    public static bool IsCircuitFree<TType>
        (this IEnumerable<TType> target, Func<TType, IEnumerable<TType>> immediateParents)
        => target.All(item => item.IsCircuitFree(immediateParents));

    public static IEnumerable<TType> Circuits<TType>
        (this IEnumerable<TType> target, Func<TType, IEnumerable<TType>> immediateParents)
        => target.Where(item => !item.IsCircuitFree(immediateParents));

    public static IEnumerable<T> NullableToArray<T>(this T target)
        where T : class => target == null? new T[0] : new[] { target };

    public static IEnumerable<T> NullableToArray<T>(this T? target)
        where T : struct => target == null? new T[0] : new[] { target.Value };

    public static TTarget Top<TTarget>
    (
        this IEnumerable<TTarget> target,
        Func<TTarget, bool> selector = null,
        Func<Exception> emptyException = null,
        Func<IEnumerable<TTarget>, Exception> multipleException = null,
        bool enableEmpty = true,
        bool enableMultiple = true
    )
    {
        if(selector != null)
            target = target.Where(selector);

        using(var enumerator = target.GetEnumerator())
        {
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

    /// <summary>
    ///     Splits an enumeration at positions where <see cref="isSeparator" /> returns true.
    ///     The resulting enumeration of enumerations may contain the the separator item
    ///     depending of <see cref="separatorTreatment" /> parameter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="isSeparator"></param>
    /// <param name="separatorTreatment"></param>
    /// <returns>
    ///     Enumeration of arrays of <see cref="target" /> items
    ///     split at points where <see cref="isSeparator" /> returns true.
    /// </returns>
    public static IEnumerable<IEnumerable<T>> Split<T>
    (
        this IEnumerable<T> target, Func<T, bool> isSeparator
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
                part = new();

                if(separatorTreatment == SeparatorTreatmentForSplit.BeginOfSubList)
                    part.Add(item);
            }
            else
                part.Add(item);

        if(part.Any())
            yield return part.ToArray();
    }

    public static int? MaxEx(this IEnumerable<int> values)
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

    public static int? MinEx(this IEnumerable<int> values)
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

    public static IEnumerable<T> SelectHierarchical<T>(this T root, Func<T, IEnumerable<T>> getChildren)
    {
        yield return root;
        foreach(var item in getChildren(root).SelectMany(i => i.SelectHierarchical(getChildren)))
            yield return item;
    }

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

    static bool AddDistinct<T>(IList<T> a, T bi, Func<T, T, T> combine)
        where T : class
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

    public static IEnumerable<T> ConcatMany<T>(this IEnumerable<IEnumerable<T>> target)
        => target
            .Where(i => i != null)
            .SelectMany(i => i);
}

// ReSharper disable once IdentifierTypo
public interface IAggregateable<T>
{
    T Aggregate(T other);
}

sealed class DuplicateKeyException : Exception { }