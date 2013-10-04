#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Debug;
using hw.UnitTest;

namespace hw.Helper
{
    public abstract class EnumEx : Dumpable
    {
        static IEnumerable<MemberInfo> AllMemberInfos(Type type)
        {
            var memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            return memberInfos.Where(memberInfo => IsValue(memberInfo, type));
        }
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

        public virtual string Tag
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

        protected override string Dump(bool isRecursion) { return GetType().PrettyName() + "." + Tag; }
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