
// ReSharper disable UnusedVariable

namespace Net5Tester.Tests.Helper.Core
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
            Tracer.Assert(target.Get12Called == 0);
            var y12 = target.Get12();
            Tracer.Assert(target.Get12Called == 1);
            var y12Again = target.Get12();
            Tracer.Assert(target.Get12Called == 1);
        }
    }
}