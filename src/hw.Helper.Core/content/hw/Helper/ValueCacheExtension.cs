// ReSharper disable CheckNamespace

namespace hw.Helper;

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

    public static TValueType CachedValue<TValueType>(this ValueCache.IContainer container, Func<TValueType> target)
        => container.CachedItem(target).Value;

    public static ValueCache<TValueType> CachedItem<TValueType>
        (this ValueCache.IContainer container, Func<TValueType> target)
        => (ValueCache<TValueType>)
            GetCache
            (
                container,
                target,
                () => new ValueCache<TValueType>(target)
            );

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