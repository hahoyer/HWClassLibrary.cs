using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Helper
{
    public static class ValueCacheExtension
    {
        public static ValueCache<TValueType> NewValueCache<TValueType>(Func<TValueType> func)
        {
            return new ValueCache<TValueType>(func);
        }

        public static TValueType CachedValue<TValueType>
            (this ValueCache.IContainer container, Func<TValueType> func)
        {
            return GetCache(container, func).Value;
        }

        static ValueCache<TValueType> GetCache<TValueType>
            (ValueCache.IContainer container, Func<TValueType> func)
        {
            object oldResult;
            if(container.Cache.TryGetValue(func, out oldResult))
                return (ValueCache<TValueType>) oldResult;

            var newResult = new ValueCache<TValueType>(func);
            container.Cache.Add(func, newResult);
            return newResult;
        }
    }

}