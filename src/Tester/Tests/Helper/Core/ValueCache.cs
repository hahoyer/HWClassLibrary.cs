using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
// ReSharper disable UnusedVariable

namespace hw.Tests.Helper.Core
{
    [UnitTest]
    public static class ValueCacheTest
    {
        sealed class ClassWithCache : ValueCache.IContainer
        {
            public int Get12Called;
            readonly ValueCache Cache;
            public ClassWithCache() => Cache = new ValueCache();

            ValueCache ValueCache.IContainer.Cache => Cache;

            public int Get12() => this.CachedValue
            (
                () =>
                {
                    Get12Called++;
                    return 12;
                }
            );
        }

        [UnitTest]
        public static void Inline()
        {
            var target = new ClassWithCache();
            (target.Get12Called == 0).Assert();
            var y12 = target.Get12();
            (target.Get12Called == 1).Assert();
            var y12Again = target.Get12();
            (target.Get12Called == 1).Assert();
        }
    }
}