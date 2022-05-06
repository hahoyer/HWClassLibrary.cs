using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public abstract class EnumEx : Dumpable
{
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

    public override string ToString() => Tag;

    protected override string Dump(bool isRecursion) => GetType().PrettyName() + "." + Tag;
    protected static IEnumerable<T> AllInstances<T>() => AllInstances(typeof(T)).Cast<T>();

    static IEnumerable<MemberInfo> AllMemberInfos(Type type)
    {
        var memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        return memberInfos.Where(memberInfo => IsValue(memberInfo, type));
    }

    static IEnumerable<EnumEx> AllInstances(Type type) => AllMemberInfos(type).Select(Instance);
    static EnumEx Instance(MemberInfo memberInfo) => (EnumEx)((FieldInfo)memberInfo).GetValue(null);

    static bool IsValue(MemberInfo memberInfo, Type type)
    {
        if(type == null)
            return false;
        if(memberInfo.DeclaringType != type)
            return false;
        if(memberInfo.MemberType != MemberTypes.Field)
            return false;
        if(memberInfo.GetAttribute<NotAnEnumInstanceAttribute>(false) != null)
            return false;
        var fieldInfo = memberInfo as FieldInfo;
        if(fieldInfo == null)
            return false;
        if(fieldInfo.FieldType != type && !fieldInfo.FieldType.IsSubclassOf(type))
            return false;
        if((fieldInfo.Attributes & FieldAttributes.Static) == 0)
            return false;
        return true;
    }
}

sealed class NotAnEnumInstanceAttribute : Attribute { }