// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Typed accessors over a <see cref="ValueCache" />. The producing delegate itself is used as the
///     cache key, so passing the same lambda location returns the same cached entry.
/// </summary>
[PublicAPI]
public static class ValueCacheExtension
{
    class FunctionCacheWithNullKey<TKey, TResult>(Func<TKey?, TResult> target)
        where TKey : notnull
    {
        readonly FunctionCache<TKey, TResult> NotNullValues = new(target);
        readonly ValueCache<TResult> NullValues = new(() => target(default));

        internal TResult this[TKey? arg] => ReferenceEquals(arg, default(TKey))? NullValues.Value : NotNullValues[arg!];
    }

    /// <summary>
    ///     Returns the cached value for <paramref name="target" />, computing it on first use.
    /// </summary>
    /// <param name="container"> The cache owner. </param>
    /// <param name="target"> Producer; also serves as the cache key. </param>
    public static TValueType CachedValue<TValueType>(this ValueCache.IContainer container, Func<TValueType> target)
        => container.CachedItem(target).Value;

    /// <summary>
    ///     Returns the <see cref="ValueCache{TValueType}" /> instance backing
    ///     <see cref="CachedValue{TValueType}" />, allowing the caller to invalidate it explicitly.
    /// </summary>
    /// <param name="container"> The cache owner. </param>
    /// <param name="target"> Producer; also serves as the cache key. </param>
    public static ValueCache<TValueType> CachedItem<TValueType>
        (this ValueCache.IContainer container, Func<TValueType> target)
        => (ValueCache<TValueType>)
            GetCache
            (
                container,
                target,
                () => new ValueCache<TValueType>(target)
            );

    /// <summary>
    ///     Memoised function call: the result of <paramref name="target" /> evaluated at
    ///     <paramref name="arg" /> is cached per argument. Null keys are handled.
    /// </summary>
    /// <param name="container"> The cache owner. </param>
    /// <param name="arg"> Function argument used as the per-call key. </param>
    /// <param name="target"> Function to memoise; the delegate identity is the cache key. </param>
    public static TResult CachedFunction<TKey, TResult>
        (this ValueCache.IContainer container, TKey? arg, Func<TKey?, TResult> target)
        where TKey : notnull
        => ((FunctionCacheWithNullKey<TKey, TResult>)
                GetCache
                (
                    container,
                    target,
                    () => new FunctionCacheWithNullKey<TKey, TResult>(target))
            )[arg];

    static object GetCache(ValueCache.IContainer container, object target, Func<object> create)
    {
        if(container.Cache.TryGetValue(target, out var storedResult))
            return storedResult;

        var newResult = create();
        container.Cache.Add(target, newResult);
        return newResult;
    }
}