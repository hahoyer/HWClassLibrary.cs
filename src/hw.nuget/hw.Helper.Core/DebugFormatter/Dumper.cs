using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

public sealed class Dumper
{
    public readonly Configuration Configuration = new();
    readonly Dictionary<object, long> ActiveObjects = new();
    long NextObjectId;

    static BindingFlags AnyBinding => BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

    internal string Dump(object target)
    {
        if(target == null)
            return "null";

        if(ActiveObjects.TryGetValue(target, out var key))
        {
            if(key == -1)
            {
                key = NextObjectId++;
                ActiveObjects[target] = key;
            }

            return "[see{" + key + "#}]";
        }

        ActiveObjects.Add(target, -1);

        var result = Dump(target.GetType(), target);

        key = ActiveObjects[target];
        if(key != -1)
            result += "{" + key + "#}";
        ActiveObjects.Remove(target);

        return result;
    }

    internal string DumpData(object target) => DumpData(target.GetType(), target);

    string Dump(Type t, object target)
    {
        var dea = DumpClassAttribute(t);
        if(dea != null)
            return dea.Dump(t, target);

        var handler = Configuration.GetDump(t);
        if(handler != null)
            return handler(t, target);

        var result = ",\n".SaveConcat(BaseDump(t, target), DumpData(t, target));
        if(result != "")
            result = result.Surround("{", "}");

        if(t == target.GetType() || result != "")
            result = t + result;

        return result;
    }

    string DumpData(Type type, object data)
    {
        var dumpData = type.GetAttribute<DumpDataClassAttribute>(false);
        if(dumpData != null)
            return dumpData.Dump(type, data);

        var memberCheck = Configuration.GetMemberCheck(type);
        var results = type
            .GetFields(AnyBinding)
            .Cast<MemberInfo>()
            .Concat(type.GetProperties(AnyBinding))
            .Where(memberInfo => IsRelevant(memberInfo, type, data))
            .Where(memberInfo => memberCheck(memberInfo, data))
            .OrderBy(GetOrderPriority)
            .Select(memberInfo => Format(memberInfo, data))
            .ToArray();
        return FormatMemberDump(results);
    }

    static double GetOrderPriority(MemberInfo memberInfo)
        => memberInfo.GetAttribute<EnableDumpAttribute>(true)?.Order ?? default;

    static string FormatMemberDump(string[] results)
    {
        var result = results;
        if(result.Length > 10)
            result = result
                .Select((s, i) => i + ":" + s)
                .ToArray();
        return result.Stringify(",\n");
    }

    string BaseDump(Type type, object target)
    {
        var baseDump = "";
        if(type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
            baseDump = Dump(type.BaseType, target);
        if(baseDump != "")
            baseDump = "Base:" + baseDump;
        return baseDump;
    }

    static DumpClassAttribute DumpClassAttribute(Type type)
        => type.GetRecentAttribute<DumpClassAttribute>() ?? DumpClassAttributeInterfaces(type);

    static DumpClassAttribute DumpClassAttributeInterfaces(Type type) => type
        .SelectHierarchical(interfaceType => interfaceType.GetInterfaces())
        .SelectMany(interfaceType => interfaceType.GetAttributes<DumpClassAttribute>(false))
        .SingleOrDefault();

    static bool IsRelevant(MemberInfo memberInfo, Type type, object target)
    {
        if(memberInfo.DeclaringType != type)
            return false;
        if(memberInfo is PropertyInfo propertyInfo && propertyInfo.GetIndexParameters().Length > 0)
            return false;
        return CheckDumpDataAttribute(memberInfo) && CheckDumpExceptAttribute(memberInfo, target);
    }

    static bool CheckDumpDataAttribute(MemberInfo memberInfo)
    {
        var attribute = memberInfo.GetAttribute<DumpEnabledAttribute>(true);
        if(attribute != null)
            return attribute.IsEnabled;

        return !IsPrivateOrDump(memberInfo);
    }

    static bool IsPrivateOrDump(MemberInfo memberInfo)
    {
        if(memberInfo.Name.Contains("Dump") || memberInfo.Name.Contains("dump"))
            return true;

        if(memberInfo is FieldInfo fieldInfo)
            return fieldInfo.IsPrivate;

        return !((PropertyInfo)memberInfo).CanRead || ((PropertyInfo)memberInfo).GetGetMethod(true).IsPrivate;
    }

    static string Format(MemberInfo memberInfo, object target)
    {
        try
        {
            return memberInfo.Name.IsSetTo(target.InvokeValue(memberInfo));
        }
        catch(Exception)
        {
            return "<not implemented>";
        }
    }

    static bool CheckDumpExceptAttribute(MemberInfo memberInfo, object target)
    {
        foreach(
            var attribute in
            Attribute.GetCustomAttributes(memberInfo, typeof(DumpAttributeBase))
                .Select(attribute => attribute as IDumpExceptAttribute)
                .Where(attribute => attribute != null))
        {
            var value = target.InvokeValue(memberInfo);
            return !attribute.IsException(value);
        }

        return true;
    }
}