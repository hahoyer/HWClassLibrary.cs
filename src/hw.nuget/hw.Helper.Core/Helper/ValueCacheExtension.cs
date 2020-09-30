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
            => GetCache(container, target).Value;

        static ValueCache<TValueType> GetCache<TValueType>(ValueCache.IContainer container, Func<TValueType> target)
        {
            if(container.Cache.TryGetValue(target, out var storedResult))
                return (ValueCache<TValueType>)storedResult;

            var newResult = new ValueCache<TValueType>(target);
            container.Cache.Add(target, newResult);
            return newResult;
        }
    }
}