using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

namespace hw.Tests.Helper.Core
{
    [UnitTest]
    public static class ValueCacheTest
    {
        sealed class ClassWithCache : ValueCache.IContainer
        {
            readonly ValueCache Cache;
            public ClassWithCache() { Cache = new ValueCache(); }

            ValueCache ValueCache.IContainer.Cache { get { return Cache; } }

            public int Get12Called;

            public int Get12()
            {
                return this.CachedValue
                    (
                        () =>
                        {
                            Get12Called ++;
                            return 12;
                        }
                    );
            }
        }

        [UnitTest]
        public static void Inline()
        {
            var x = new ClassWithCache();
            Tracer.Assert(x.Get12Called == 0);
            var y12 = x.Get12();
            Tracer.Assert(x.Get12Called == 1);
            var y12Again = x.Get12();
            Tracer.Assert(x.Get12Called == 1);
        }
    }
}