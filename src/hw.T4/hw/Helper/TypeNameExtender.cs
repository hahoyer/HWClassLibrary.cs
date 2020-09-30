using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hw.Helper
{
    public static class TypeNameExtender
    {
        static readonly ValueCache<TypeLibrary> _referencedTypesCache = new ValueCache<TypeLibrary>
            (ObtainReferencedTypes);

        public static void OnModuleLoaded() { _referencedTypesCache.IsValid = false; }

        public static Type[] Types => ReferencedTypes.Types;

        public static Type[] ResolveType(this string typeName)
        {
            return ReferencedTypes.ByNamePartMulti[typeName];
        }

        public static Type ResolveUniqueType(this string typeName)
        {
            return ReferencedTypes.ByNamePart[typeName];
        }

        public static string PrettyName(this Type type) { return ReferencedTypes.PrettyName[type]; }

        public static string CompleteName(this Type type)
        {
            return ReferencedTypes.CompleteName[type];
        }

        static TypeLibrary ObtainReferencedTypes()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            return new TypeLibrary(assembly.GetReferencedTypes());
        }

        static TypeLibrary ReferencedTypes
        {
            get
            {
                lock(_referencedTypesCache)
                    return _referencedTypesCache.Value;
            }
        }

        public static string NullableName(this Type type)
        {
            if(type.IsClass)
                return type.PrettyName();
            return type.PrettyName() + "?";
        }
    }
}