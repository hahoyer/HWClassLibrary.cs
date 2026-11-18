using System.Diagnostics;
using System.Text;
using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Library of LINQ-style helpers covering enumeration, grouping, splitting, hierarchical
///     traversal, custom aggregation and small assertion shortcuts. Most members are extension
///     methods so they read naturally at the call site.
/// </summary>
[PublicAPI]
public static class LinqExtension
{
    /// <summary>How <see cref="LinqExtension.Split{T}" /> places the separator element relative to the produced sub-lists.</summary>
    public enum SeparatorTreatmentForSplit
    {
        Drop,
        BeginOfSubList,
        EndOfSubList
    }

    extension<T>(IList<T> a)
    {
        /// <summary>
        ///     Adds entries from <paramref name="b" /> that have no equivalent (per
        ///     <paramref name="isEqual" />) in <paramref name="a" />. Returns whether anything was
        ///     actually added.
        /// </summary>
        public bool AddDistinct(IEnumerable<T> b, Func<T, T, bool> isEqual)
            => InternalAddDistinct(a, b, isEqual);
    }

    extension<T>(IList<T> a)
        where T : class
    {
        /// <summary>
        ///     Adds entries from <paramref name="b" /> using a combiner instead of an equality test:
        ///     when <paramref name="combine" /> returns non-<c>null</c> for an existing element, that
        ///     element is replaced with the combined value; otherwise the new element is appended.
        ///     Returns whether at least one item was appended.
        /// </summary>
        public bool AddDistinct(IEnumerable<T> b, Func<T, T, T> combine)
            => InternalAddDistinct(a, b, combine);
    }

    extension<T>(IEnumerable<T> target)
    {
        /// <summary>
        ///     Deprecated; use <see cref="Split(IEnumerable{T},Func{T,bool},SeparatorTreatmentForSplit)" /> with
        ///     <see cref="SeparatorTreatmentForSplit.BeginOfSubList" />.
        /// </summary>
        [Obsolete("Use Split(isHead, SeparatorTreatmentForSplit.BeginOfSubList) instead.")]
        public IEnumerable<IEnumerable<T>> Separate(Func<T, bool> isHead)
            => target.Split(isHead, SeparatorTreatmentForSplit.BeginOfSubList);

        /// <summary>Reflection-based dump of the sequence, useful in debugger watches.</summary>
        public string Dump() => Tracer.Dump(target);

        /// <summary>
        ///     Concatenates the string representations of <paramref name="target" /> separated by
        ///     <paramref name="separator" />. With <paramref name="showNumbers" /> set, each item is
        ///     prefixed with its zero-based index.
        /// </summary>
        public string Stringify(string separator = "", bool showNumbers = false)
        {
            var result = new StringBuilder();
            var i = 0;
            var isNext = false;
            foreach (var element in target)
            {
                if (isNext)
                    result.Append(separator);
                if (showNumbers)
                    result.Append("[" + i + "] ");
                isNext = true;
                result.Append(element);
                i++;
            }

            return result.ToString();
        }

        /// <summary>Sums up <see cref="TimeSpan" /> values projected from <paramref name="target" />.</summary>
        public TimeSpan Sum(Func<T, TimeSpan> selector)
        {
            var result = new TimeSpan();
            return target.Aggregate(result, (current, element) => current + selector(element));
        }

        /// <summary>
        ///     Splits the sequence at every element matching <paramref name="isSeparator" />. The
        ///     fate of the separator itself is decided by <paramref name="separatorTreatment" />.
        ///     Empty sub-lists are not yielded.
        /// </summary>
        /// <param name="isSeparator"> Predicate identifying separator elements. </param>
        /// <param name="separatorTreatment"> Where the separator goes in the output. </param>
        public IEnumerable<IEnumerable<T>> Split
        (
            Func<T, bool> isSeparator
            , SeparatorTreatmentForSplit separatorTreatment = SeparatorTreatmentForSplit.Drop
        )
        {
            var part = new List<T>();
            foreach (var item in target)
                if (isSeparator(item))
                {
                    if (separatorTreatment == SeparatorTreatmentForSplit.EndOfSubList)
                        part.Add(item);

                    if (part.Any())
                        yield return part.ToArray();
                    part = [];

                    if (separatorTreatment == SeparatorTreatmentForSplit.BeginOfSubList)
                        part.Add(item);
                }
                else
                {
                    part.Add(item);
                }

            if (part.Any())
                yield return part.ToArray();
        }
    }

    extension<T>(IEnumerable<T?> target)
        where T : class, IAggregateable<T>
    {
        /// <summary>
        ///     Folds the sequence using <see cref="IAggregateable{T}.Aggregate" />, treating
        ///     <c>null</c> elements as skip. Returns <paramref name="getDefault" />'s result for an
        ///     empty sequence (or <c>null</c> when no default is supplied).
        /// </summary>
        public T? Aggregate(Func<T>? getDefault = null)
        {
            var targetArray = target.ToArray();
            if (!targetArray.Any())
                return getDefault?.Invoke();
            var result = targetArray[0];
            for (var i = 1; i < targetArray.Length; i++)
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
        /// <summary>One <see cref="Dumpable.Dump()" /> per line, prefixed with the element's zero-based index.</summary>
        public string DumpLines()
        {
            var i = 0;
            return target.Aggregate("", (a, xx) => a + "[" + i++ + "] " + xx.Dump() + "\n");
        }
    }

    extension<T>(IEnumerable<T> list)
    {
        /// <summary>
        ///     Indices of elements that are "frontier" items: no other element <c>other</c> satisfies
        ///     <c>isInRelation(element, other)</c>. With <c>isInRelation = (a, b) =&gt; a &lt; b</c>
        ///     this returns the maxima.
        /// </summary>
        public IEnumerable<int> FrameIndexList(Func<T, T, bool> isInRelation)
        {
            var listArray = list.ToArray();
            return
                listArray.Select((item, index) => new Tuple<T, int>(item, index))
                    .Where(element => !listArray.Any(other => isInRelation(element.Item1, other)))
                    .Select(element => element.Item2);
        }

        /// <summary>Element-form of <see cref="FrameIndexList" />: returns the items themselves rather than their positions.</summary>
        public IEnumerable<T> FrameElementList(Func<T, T, bool> isInRelation)
        {
            var listArray = list.ToArray();
            return listArray.FrameIndexList(isInRelation).Select(index => listArray[index]);
        }
    }

    extension<T>(IEnumerable<T> list)
        where T : IComparable<T>
    {
        /// <summary>Indices of the maximum elements of <paramref name="list" /> using natural order.</summary>
        public IEnumerable<int> GetMaxIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) < 0);

        /// <summary>Indices of the minimum elements of <paramref name="list" /> using natural order.</summary>
        public IEnumerable<int> GetMinIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) > 0);

        /// <summary>Deprecated; use <see cref="GetMaxIndexList" />.</summary>
        [Obsolete("use GetMaxIndexList", false)]
        public IEnumerable<int> MaxIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) < 0);

        /// <summary>Deprecated; use <see cref="GetMinIndexList" />.</summary>
        [Obsolete("use GetMinIndexList", false)]
        public IEnumerable<int> MinIndexList()
            => list.FrameIndexList((a, b) => a.CompareTo(b) > 0);
    }

    extension<T>(IList<T> target)
    {
        /// <summary>Whether <paramref name="target" /> begins with <paramref name="y" /> element-wise.</summary>
        public bool StartsWith(IList<T> y)
        {
            if (target.Count < y.Count)
                return false;
            return !y.Where((t, i) => !Equals(target[i], t)).Any();
        }

        /// <summary>Like <see cref="StartsWith(IList{T},IList{T})" />, but also requires <paramref name="target" /> to be strictly longer.</summary>
        public bool StartsWithAndNotEqual(IList<T> y)
            => target.Count != y.Count && target.StartsWith(y);
    }

    extension<T>(T? target)
        where T : class
    {
        /// <summary>
        ///     Applies <paramref name="function" /> when <paramref name="target" /> is non-<c>null</c>;
        ///     otherwise returns <c>default</c>. Equivalent to <c>target?.Apply(function)</c> but
        ///     usable on any reference type.
        /// </summary>
        public TResult? CheckedApply<TResult>(Func<T, TResult> function)
            where TResult : class
            => target == default(T) ? default : function(target);

        /// <summary>Wraps a single value as a one-element enumerable, or returns an empty one when <c>null</c>.</summary>
        public IEnumerable<T> NullableToArray()
            => target == null ? [] : new[] { target };
    }

    extension<TResult>(TResult? target)
        where TResult : struct
    {
        /// <summary>Asserts that the nullable value is set and unwraps it.</summary>
        public TResult AssertValue()
        {
            (target != null).Assert(stackFrameDepth: 1);
            return target!.Value;
        }
    }

    extension<TResult>(TResult? target)
        where TResult : class
    {
        /// <summary>Asserts that <paramref name="target" /> is not <c>null</c> and returns it as a non-nullable reference.</summary>
        /// <param name="stackFrameDepth"> Frames to skip when reporting the call site. </param>
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
        /// <summary>Yields the integers <c>0, 1, …, count-1</c>.</summary>
        public IEnumerable<int> Select()
        {
            for (var i = 0; i < count; i++)
                yield return i;
        }

        /// <summary>Yields <paramref name="getValue" /> applied to each integer in <c>0..count-1</c>.</summary>
        public IEnumerable<T> Select<T>(Func<int, T> getValue)
        {
            for (var i = 0; i < count; i++)
                yield return getValue(i);
        }
    }

    extension(long count)
    {
        /// <summary>Yields the long integers <c>0, 1, …, count-1</c>.</summary>
        public IEnumerable<long> Select()
        {
            for (long i = 0; i < count; i++)
                yield return i;
        }

        /// <summary>Yields <paramref name="getValue" /> applied to each long integer in <c>0..count-1</c>.</summary>
        public IEnumerable<T> Select<T>(Func<long, T> getValue)
        {
            for (long i = 0; i < count; i++)
                yield return getValue(i);
        }
    }

    extension<TKey, TValue>(IDictionary<TKey, TValue> target)
    {
        /// <summary>
        ///     Copies entries from <paramref name="newEntries" /> into <paramref name="target" />,
        ///     skipping keys that already exist (so existing values win, no overwrite).
        /// </summary>
        public void AddRange
        (
            IEnumerable<KeyValuePair<TKey, TValue>> newEntries
        )
        {
            foreach (var item in newEntries.Where(entry => !target.ContainsKey(entry.Key)))
                target.Add(item);
        }
    }

    extension<T>(IEnumerable<T> items)
    {
        /// <summary>
        ///     Returns the zero-based index of the first element satisfying <paramref name="predicate" />,
        ///     or <c>null</c> when no such element exists. Iteration stops at the first hit.
        /// </summary>
        /// <param name="predicate"> Filter applied to each element. </param>
        public int? IndexWhere(Func<T, bool> predicate)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var result = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                    return result;
                result++;
            }

            return null;
        }
    }

    extension<T>(T? current)
        where T : class
    {
        /// <summary>
        ///     Yields <paramref name="current" /> and every successor produced by <paramref name="getNext" />
        ///     until it returns <c>null</c> — i.e. a linked-list walk.
        /// </summary>
        public IEnumerable<T> Chain(Func<T, T?> getNext)
        {
            while (current != null)
            {
                yield return current;
                current = getNext(current);
            }
        }
    }

    extension<T>(T a)
    {
        /// <summary>
        ///     Whether <paramref name="a" /> equals any element of <paramref name="b" />. The reverse of
        ///     <see cref="Enumerable.Contains{T}(IEnumerable{T}, T)" />.
        /// </summary>
        public bool In(params T[] b) => b.Contains(a);
    }

    extension<TType>(IEnumerable<TType> target)
    {
        /// <summary>
        ///     Asserts that the parent relation has no cycles. Always returns <c>null</c>; the call exists for its side
        ///     effect.
        /// </summary>
        public IEnumerable<TType>? Sort(Func<TType, IEnumerable<TType>> immediateParents)
        {
            var xx = target.ToArray();
            xx.IsCircuitFree(immediateParents).Assert();
            return null;
        }

        /// <summary>Transitive closure of <paramref name="target" /> under <paramref name="immediateParents" />.</summary>
        public IEnumerable<TType> Closure(Func<TType, IEnumerable<TType>> immediateParents)
        {
            var types = target.ToArray();
            var targets = types;
            while (true)
            {
                targets = targets.SelectMany(immediateParents).Except(types).ToArray();
                if (!targets.Any())
                    return types;
                types = types.Union(targets).ToArray();
            }
        }

        /// <summary>Whether none of the elements participates in a cycle under <paramref name="immediateParents" />.</summary>
        public bool IsCircuitFree(Func<TType, IEnumerable<TType>> immediateParents)
            => target.All(item => item.IsCircuitFree(immediateParents));

        /// <summary>The subset of <paramref name="target" /> participating in a cycle under <paramref name="immediateParents" />.</summary>
        public IEnumerable<TType> Circuits(Func<TType, IEnumerable<TType>> immediateParents)
            => target.Where(item => !item.IsCircuitFree(immediateParents));
    }

    extension<TType>(TType target)
    {
        /// <summary>Whether <paramref name="target" /> is not reachable from itself via <paramref name="immediateParents" />.</summary>
        public bool IsCircuitFree(Func<TType, IEnumerable<TType>> immediateParents)
            => immediateParents(target).Closure(immediateParents).All(item => !Equals(item, target));
    }

    extension<T>(T? target)
        where T : struct
    {
        /// <summary>Wraps a nullable value type as a one-element enumerable (or empty when null).</summary>
        public IEnumerable<T> NullableToArray()
            => target == null ? [] : new[] { target.Value };
    }

    extension<TTarget>(IEnumerable<TTarget> target)
    {
        /// <summary>
        ///     Returns the single element of <paramref name="target" /> (after optional filtering),
        ///     with explicit control over what should happen for empty or multi-result cases.
        /// </summary>
        /// <param name="selector"> Optional pre-filter applied to the sequence. </param>
        /// <param name="emptyException"> Producer for the exception thrown when no element matches. </param>
        /// <param name="multipleException"> Producer for the exception thrown when several elements match. </param>
        /// <param name="enableEmpty"> If <c>true</c>, an empty result is allowed (returns <c>default</c>). </param>
        /// <param name="enableMultiple"> If <c>true</c>, multiple matches are allowed (returns the first). </param>
        public TTarget? Top
        (
            Func<TTarget, bool>? selector = null,
            Func<Exception>? emptyException = null,
            Func<IEnumerable<TTarget>, Exception>? multipleException = null,
            bool enableEmpty = true,
            bool enableMultiple = true
        )
        {
            if (selector != null)
                target = target.Where(selector);

            using var enumerator = target.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                if (emptyException != null)
                    throw emptyException();
                return enableEmpty ? default : target.Single();
            }

            var result = enumerator.Current;
            if (!enumerator.MoveNext())
                return result;

            if (multipleException != null)
                throw multipleException(target);
            return enableMultiple ? result : target.Single();
        }
    }

    extension(IEnumerable<int> values)
    {
        /// <summary>
        ///     Maximum element of <paramref name="values" />, or <c>null</c> when empty (unlike
        ///     <see cref="Enumerable.Max{TSource}(IEnumerable{TSource})" />, which throws).
        /// </summary>
        public int? Maxx
        {
            get
            {
                if (values == null)
                    throw new ArgumentNullException(nameof(values));
                int? result = null;
                foreach (var value in values)
                    if (result == null)
                        result = value;
                    else if (value > result)
                        result = value;
                return result;
            }
        }

        /// <summary>Minimum element of <paramref name="values" />, or <c>null</c> when empty.</summary>
        public int? Minn
        {
            get
            {
                if (values == null)
                    throw new ArgumentNullException(nameof(values));
                int? result = null;
                foreach (var value in values)
                    if (result == null)
                        result = value;
                    else if (value < result)
                        result = value;
                return result;
            }
        }
    }

    extension<T>(T root)
    {
        /// <summary>
        ///     Pre-order traversal of <paramref name="root" /> followed by every descendant under
        ///     <paramref name="getChildren" />.
        /// </summary>
        public IEnumerable<T> SelectHierarchical(Func<T, IEnumerable<T>> getChildren)
        {
            yield return root;
            foreach (var item in getChildren(root).SelectMany(i => i.SelectHierarchical(getChildren)))
                yield return item;
        }
    }

    /// <summary>
    ///     Yields ascending integers starting at 0 while <paramref name="getValue" /> returns
    ///     <c>true</c>. Useful for "<c>while-yield</c>" idioms over indexed sources.
    /// </summary>
    public static IEnumerable<int> Where(Func<int, bool> getValue)
    {
        for (var i = 0; getValue(i); i++)
            yield return i;
    }

    extension<TKey, TLeft, TRight>(IEnumerable<TLeft> left)
    {
        /// <summary>
        ///     Outer-merges <paramref name="left" /> and <paramref name="right" /> by their respective
        ///     keys, returning one tuple per key with the left and/or right element (whichever is
        ///     present). Throws <see cref="DuplicateKeyException" /> when a key occurs more than once on
        ///     either side.
        /// </summary>
        public IEnumerable<Tuple<TKey, TLeft?, TRight?>>
            Merge(IEnumerable<TRight> right,
                Func<TLeft, TKey> getLeftKey,
                Func<TRight, TKey> getRightKey
            )
        {
            var leftCommon = left.Select
                (l => new Tuple<TKey, TLeft?, TRight?>(getLeftKey(l), l, default));
            var rightCommon = right.Select
                (r => new Tuple<TKey, TLeft?, TRight?>(getRightKey(r), default, r));
            return
                leftCommon.Union(rightCommon)
                    .GroupBy(t => t.Item1)
                    .Select(Merge);
        }
    }

    extension<TKey, T>(IEnumerable<T> left)
    {
        /// <summary>
        ///     Symmetric variant of
        ///     <see>
        ///         <cref>Merge{TKey, TLeft, TRight}</cref>
        ///     </see>
        ///     for when both sides have the same element type.
        /// </summary>
        public IEnumerable<Tuple<TKey, T?, T?>>
            Merge(IEnumerable<T> right, Func<T, TKey> getKey)
            => left.Merge(right, getKey, getKey);
    }

    /// <summary>
    ///     Combines a key-grouping into a single triple. Public so callers can supply this as the
    ///     last step in a custom merge pipeline; throws <see cref="DuplicateKeyException" /> when
    ///     the grouping is malformed.
    /// </summary>
    public static Tuple<TKey, TLeft?, TRight?>
        Merge<TKey, TLeft, TRight>(IGrouping<TKey, Tuple<TKey, TLeft?, TRight?>> grouping)
    {
        var list = grouping.ToArray();
        switch (list.Length)
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

    extension<TKey, T>(IEnumerable<T> list) where TKey : notnull
    {
        /// <summary>
        ///     Lazily groups <paramref name="list" /> by <paramref name="selector" /> into a
        ///     <see cref="FunctionCache{TKey, TValue}" />: missing keys yield the empty group on first
        ///     access. Unlike <see cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />,
        ///     the source is re-scanned on every miss instead of materialised eagerly.
        /// </summary>
        public FunctionCache<TKey, IEnumerable<T>> 
            ToDictionaryEx(Func<T, TKey> selector) => new(key => list.Where(item => Equals(selector(item), key)));
    }

    static bool InternalAddDistinct<T>(ICollection<T> a, IEnumerable<T> b, Func<T, T, bool> isEqual)
    {
        var result = false;
        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach (var bi in b)
            if (AddDistinct(a, bi, isEqual))
                result = true;
        return result;
    }

    static bool InternalAddDistinct<T>(IList<T> a, IEnumerable<T> b, Func<T, T, T> combine)
        where T : class
    {
        var result = false;
        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach (var bi in b)
            if (AddDistinct(a, bi, combine))
                result = true;
        return result;
    }

    static bool AddDistinct<T>(ICollection<T> a, T bi, Func<T, T, bool> isEqual)
    {
        if (a.Any(ai => isEqual(ai, bi)))
            return false;
        a.Add(bi);
        return true;
    }

    static bool AddDistinct<T>(IList<T> a, T bi, Func<T, T, T?> combine)
        where T : class
    {
        for (var i = 0; i < a.Count; i++)
        {
            var ab = combine(a[i], bi);
            if (ab == null)
                continue;
            a[i] = ab;
            return false;
        }

        a.Add(bi);
        return true;
    }

    /// <summary>Flattens a sequence of sequences, treating <c>null</c> inner sequences as empty.</summary>
    public static IEnumerable<T> ConcatMany<T>(this IEnumerable<IEnumerable<T>?> target)
        => target
            .Where(i => i != null)
            .SelectMany(i => i!);
}

/// <summary>
///     Implemented by types that know how to fold one of themselves into another, used by the
///     null-tolerant <see cref="LinqExtension.Aggregate{T}" /> overload.
/// </summary>
// ReSharper disable once IdentifierTypo
public interface IAggregateable<T>
{
    /// <summary>Combines this instance with <paramref name="other" /> (which may be <c>null</c>).</summary>
    T Aggregate(T? other);
}

sealed class DuplicateKeyException : Exception;