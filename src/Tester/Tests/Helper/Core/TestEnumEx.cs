using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

// ReSharper disable UnusedMember.Local
// ReSharper disable CheckNamespace
// ReSharper disable ClassCanBeSealed.Local

namespace hw.Tests.Helper.Core;

[UnitTest]
public sealed class TestEnumEx
{
    class MyEnum : EnumEx
    {
        public static readonly MyEnum X1 = new();
        public static readonly MyEnum X2 = new();
        public static readonly MyEnum X3 = new();
        public static readonly MyEnumDerived X4 = new();

        [NotAnEnumInstance]
        public static readonly MyEnum Xclude = new();

        static readonly MyEnum XPrivate = new();
        public static IEnumerable<MyEnum> All => AllInstances<MyEnum>();
    }

    class MyEnumDerived : MyEnum { }

    [UnitTest]
    public void Test()
    {
        var all = MyEnum.All.ToArray();
        (all.Length == 4).Assert();

        (MyEnum.X4.Tag == "X4").Assert();
        (new MyEnum().Tag == null).Assert();
        (MyEnum.X1.Tag == "X1").Assert();
    }
}