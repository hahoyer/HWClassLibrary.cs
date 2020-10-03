using System;
using JetBrains.Annotations;

namespace hw.Helper
{
    [PublicAPI]
    public static class ValueCacheExtension
    {
        public static ValueCache<TValueType> NewValueCache<TValueType>(Func<TValueType> target)
            => new ValueCache<TValueType>(target);

        public static TValueType CachedValue<TValueType>(this ValueCache.IContainer container, Func<TValueType> target)
            => ((ValueCache<TValueType>)
                    GetCache
                    (
                        container,
                        target,
                        () => new ValueCache<TValueType>(target)
                    )
                ).Value;

        public static TResult CachedFunction<TValueType, TResult>
            (this ValueCache.IContainer container, TValueType arg, Func<TValueType, TResult> target)
            => ((FunctionCache<TValueType, TResult>)
                GetCache
                (
                    container,
                    target,
                    () => new FunctionCache<TValueType, TResult>(target)
                ))[arg];

        static object GetCache(ValueCache.IContainer container, object target, Func<object> create)
        {
            if(container.Cache.TryGetValue(target, out var storedResult))
                return storedResult;

            var newResult = create();
            container.Cache.Add(target, newResult);
            return newResult;
        }
    }
}