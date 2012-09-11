using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;

namespace HWClassLibrary.Helper
{
    abstract class EnumEx
    {
        static IEnumerable<MemberInfo> AllMemberInfos(Type type) { return type.GetMembers().Where(memberInfo => IsValue(memberInfo, type)); }
        static IEnumerable<EnumEx> AllInstances(Type type) { return AllMemberInfos(type).Select(Instance); }
        static EnumEx Instance(MemberInfo memberInfo) { return (EnumEx) ((FieldInfo) memberInfo).GetValue(null); }
        protected static IEnumerable<T> AllInstances<T>() { return AllInstances(typeof(T)).Cast<T>(); }

        static bool IsValue(MemberInfo memberInfo, Type type)
        {
            if(memberInfo.DeclaringType != type)
                return false;
            if(memberInfo.MemberType != MemberTypes.Field)
                return false;
            if (memberInfo.GetAttribute<NotAEnumInstanceAttribute>(false) != null)
                return false;
            var fieldInfo = memberInfo as FieldInfo;
            if(fieldInfo == null)
                return false;
            if(fieldInfo.FieldType != type)
                return false;
            if((fieldInfo.Attributes & FieldAttributes.Static) == 0)
                return false;
            return true;
        }
        public override string ToString() { return Tag; }
        public string Tag { get { return AllMemberInfos(GetType()).Single(t => Instance(t) == this).Name; } }
    }

    [TestFixture]
    public sealed class TestEnumEx
    {
        sealed class MyEnum : EnumEx
        {
            public static readonly MyEnum X1 = new MyEnum();
            public static readonly MyEnum X2 = new MyEnum();
            public static readonly MyEnum X3 = new MyEnum();
            public static readonly MyEnum X4 = new MyEnum();
            [NotAEnumInstance]
            public static readonly MyEnum Xclude = new MyEnum();
            public static IEnumerable<MyEnum> All { get { return AllInstances<MyEnum>(); } }
        }

        [Test]
        public void Test()
        {
            var all = MyEnum.All.ToArray();
            Tracer.Assert(all.Length == 4);

            Tracer.Assert(MyEnum.X1.Tag == "X1");
        }
    }

    sealed class NotAEnumInstanceAttribute : Attribute
    {}
}