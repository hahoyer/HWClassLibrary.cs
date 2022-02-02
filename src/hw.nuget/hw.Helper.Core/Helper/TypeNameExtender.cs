using System;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public static class TypeNameExtender
{
    static readonly ValueCache<TypeLibrary> ReferencedTypesCache = new(ObtainReferencedTypes);

    public static Type[] Types => ReferencedTypes.Types;

    static TypeLibrary ReferencedTypes
    {
        get
        {
            lock(ReferencedTypesCache)
                return ReferencedTypesCache.Value;
        }
    }

    public static void OnModuleLoaded()
    {
        lock(ReferencedTypesCache)
            ReferencedTypesCache.IsValid = false;
    }

    public static Type[] ResolveType(this string typeName) => ReferencedTypes.ByNamePartMulti[typeName];

    public static Type ResolveUniqueType(this string typeName) => ReferencedTypes.ByNamePart[typeName];

    public static string PrettyName(this Type type) => ReferencedTypes.PrettyName[type];

    public static string CompleteName(this Type type) => ReferencedTypes.CompleteName[type];

    public static string NullableName(this Type type)
    {
        if(type.IsClass)
            return type.PrettyName();
        return type.PrettyName() + "?";
    }

    static TypeLibrary ObtainReferencedTypes()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        return new(assembly.GetReferencedTypes());
    }
}