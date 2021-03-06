﻿using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
// ReSharper disable UnusedMember.Local

namespace hw.Tests.Helper.Core
{
    [UnitTest]
    public sealed class TestEnumEx
    {
        class MyEnum : EnumEx
        {
            public static readonly MyEnum X1 = new MyEnum();
            public static readonly MyEnum X2 = new MyEnum();
            public static readonly MyEnum X3 = new MyEnum();
            public static readonly MyEnumDerived X4 = new MyEnumDerived();

            [NotAnEnumInstance]
            public static readonly MyEnum Xclude = new MyEnum();

            static readonly MyEnum XPrivate = new MyEnum();
            public static IEnumerable<MyEnum> All => AllInstances<MyEnum>();
        }

        class MyEnumDerived : MyEnum { }

        [UnitTest]
        public void Test()
        {
            var all = MyEnum.All.ToArray();
            Tracer.Assert(all.Length == 4);

            Tracer.Assert(MyEnum.X4.Tag == "X4");
            Tracer.Assert(new MyEnum().Tag == null);
            Tracer.Assert(MyEnum.X1.Tag == "X1");
        }
    }
}