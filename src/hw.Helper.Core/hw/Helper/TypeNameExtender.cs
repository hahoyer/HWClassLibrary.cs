using System.Reflection;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Renders <see cref="Type" /> names in a C#-like, human-readable form, with namespaces
///     shortened just enough to disambiguate from other reachable types. Resolution and rendering
///     are cached; call <see cref="OnModuleLoaded" /> after dynamic assembly loads to refresh.
/// </summary>
[PublicAPI]
public static class TypeNameExtender
{
    static readonly ValueCache<TypeLibrary> ReferencedTypesCache = new(ObtainReferencedTypes);

    /// <summary>All types known to the resolver — i.e. types declared in the entry assembly and any assembly it transitively references.</summary>
    public static Type[] Types => ReferencedTypes.Types;

    static TypeLibrary ReferencedTypes
    {
        get
        {
            lock(ReferencedTypesCache)
                return ReferencedTypesCache.Value;
        }
    }

    /// <summary>Drops the resolver cache so the next access rebuilds it from the current assemblies.</summary>
    public static void OnModuleLoaded()
    {
        lock(ReferencedTypesCache)
            ReferencedTypesCache.IsValid = false;
    }

    /// <summary>All types whose fully-qualified name ends with <paramref name="typeName" />.</summary>
    public static Type[] ResolveType(this string typeName) => ReferencedTypes.ByNamePartMulti[typeName];

    /// <summary>The single type whose fully-qualified name ends with <paramref name="typeName" />; throws if zero or several match.</summary>
    public static Type ResolveUniqueType(this string typeName) => ReferencedTypes.ByNamePart[typeName];

    /// <summary>Short, C#-like name of <paramref name="type" /> (primitives spelled out, namespace shortened to disambiguate).</summary>
    public static string PrettyName(this Type type) => ReferencedTypes.PrettyName[type];

    /// <summary>Fully-qualified C#-like name of <paramref name="type" /> with the entire namespace.</summary>
    public static string CompleteName(this Type type) => ReferencedTypes.CompleteName[type];

    /// <summary>
    ///     <see cref="PrettyName" /> appended with <c>?</c> for non-class types so that value types
    ///     read as nullable. Class types are returned unchanged because they are reference-nullable
    ///     by default.
    /// </summary>
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