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

        static bool IsValue(MemberInfo m, Type type)
        {
            if(m.DeclaringType != type)
                return false;
            if(m.MemberType != MemberTypes.Field)
                return false;
            if(m.GetAttribute<NotAnEnumInstanceAttribute>(false) != null)
                return false;
            var fieldInfo = m as FieldInfo;
            if(fieldInfo == null)
                return false;
            if(fieldInfo.FieldType != type && !fieldInfo.FieldType.IsSubclassOf(type))
                return false;
            if((fieldInfo.Attributes & FieldAttributes.Static) == 0)
                return false;
            return true;
        }
        public override string ToString() { return Tag; }
        public string Tag
        {
            get
            {
                for(var type = GetType(); type != null; type = type.BaseType)
                {
                    var instance = AllMemberInfos(type).SingleOrDefault(t => Instance(t) == this);
                    if(instance != null)
                        return instance.Name;
                }
                return null;
            }
        }
    }

    [TestFixture]
    public sealed class TestEnumEx
    {
        class MyEnum : EnumEx
        {
            public static readonly MyEnum X1 = new MyEnum();
            public static readonly MyEnum X2 = new MyEnum();
            public static readonly MyEnum X3 = new MyEnum();
            public static readonly MyEnumDerived X4 = new MyEnumDerived();
            static readonly MyEnum XPrivate = new MyEnum();
            [NotAnEnumInstance]
            public static readonly MyEnum Xclude = new MyEnum();
            public static IEnumerable<MyEnum> All { get { return AllInstances<MyEnum>(); } }
        }

        class MyEnumDerived : MyEnum
        {}

        [Test]
        public void Test()
        {
            var all = MyEnum.All.ToArray();
            Tracer.Assert(all.Length == 4);

            Tracer.Assert(MyEnum.X4.Tag == "X4");
            Tracer.Assert(new MyEnum().Tag == null);
            Tracer.Assert(MyEnum.X1.Tag == "X1");
        }
    }

    sealed class NotAnEnumInstanceAttribute : Attribute
    {}
}