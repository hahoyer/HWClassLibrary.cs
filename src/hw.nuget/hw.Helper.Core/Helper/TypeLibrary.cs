using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable CheckNamespace

namespace hw.Helper;

sealed class TypeLibrary
{
    public readonly FunctionCache<string, Type> ByNamePart;
    public readonly FunctionCache<string, Type[]> ByNamePartMulti;
    public readonly FunctionCache<Type, string> CompleteName;
    public readonly FunctionCache<Type, string> PrettyName;

    public Type[] Types { get; }
    readonly Dictionary<string, IGrouping<string, Type>> ByName;

    public TypeLibrary(IEnumerable<Type> types)
    {
        Types = types.ToArray();
        ByNamePart = new(GetTypeByNamePart);
        ByNamePartMulti = new(GetTypesByNamePart);
        ByName = Types.GroupBy(t => t.Name, t => t).ToDictionary(t => t.Key, t => t);
        PrettyName = new(type => ObtainTypeName(type, true));
        CompleteName = new(type => ObtainTypeName(type, false));
    }

    Type GetTypeByNamePart(string arg) => Types.Single(t => t.CompleteName().EndsWith(arg));
    Type[] GetTypesByNamePart(string arg) => Types.Where(t => t.CompleteName().EndsWith(arg)).ToArray();

    Type[] ConflictingTypes(Type type)
        => ByName.TryGetValue(type.Name, out var result)
            ? result.Where(definedType => definedType.Namespace != type.Namespace).ToArray()
            : new Type[0];

    string ObtainTypeName(Type type, bool shortenNamespace)
    {
        if(type == typeof(int))
            return "int";
        if(type == typeof(string))
            return "string";

        var namePart = ObtainNamePart(type, shortenNamespace);
        var namespacePart = ObtainNameSpacePart(type, shortenNamespace);

        return namespacePart + namePart;
    }

    string ObtainNameSpacePart(Type type, bool shortenNamespace)
    {
        if(type.IsNested && !type.IsGenericParameter)
            return ObtainTypeName(type.DeclaringType, shortenNamespace) + ".";

        if(type.Namespace == null)
            return "";

        if(!shortenNamespace)
            return type.Namespace + ".";

        var conflictingTypes = ConflictingTypes(type);

        var namespaceParts = type.Namespace.Split('.').Reverse().ToArray();
        var namespacePart = "";
        for(var i = 0; i < namespaceParts.Length && conflictingTypes.Length > 0; i++)
        {
            namespacePart = namespaceParts[i] + "." + namespacePart;
            conflictingTypes = conflictingTypes.Where(conflictingType
                => ("." + conflictingType.Namespace + ".").EndsWith("." + namespacePart)).ToArray();
        }

        return namespacePart;
    }

    string ObtainNamePart(Type type, bool shortenNamespace)
    {
        var result = type.Name;

        if(result.Contains("`"))
            result = result.Remove(result.IndexOf('`'));

        if(type.IsGenericType)
            return result + ObtainNameForGeneric(type.GetGenericArguments(), shortenNamespace);

        return result;
    }

    string ObtainNameForGeneric(Type[] types, bool shortenNamespace)
    {
        var result = "";
        var delimiter = "<";
        foreach(var t in types)
        {
            result += delimiter;
            delimiter = ",";
            result += ObtainTypeName(t, shortenNamespace);
        }

        return result + ">";
    }
}