using System.Reflection;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.Helper
{
    public static class TypeNameExtender
    {
        public static string PrettyName(this Type type) { return type.PrettyName(Assembly.GetCallingAssembly()); }

        public static string PrettyName(this Type type, Assembly assembly)
        {
            if(type == typeof(int))
                return "int";
            if(type == typeof(string))
                return "string";

            var result = PrettyTypeName(type, assembly);
            if(type.IsGenericType)
                result = result + PrettyNameForGeneric(type.GetGenericArguments(), assembly);
            return result;
        }

        private static string PrettyTypeName(Type type, Assembly assembly)
        {
            var result = type.Name;
            if(type.IsGenericType)
                result = result.Remove(result.IndexOf('`'));

            if (type.Namespace == null)
                return result;

            if (assembly == null)
                return type.Namespace + "." + result;

            var conflictingTypes = assembly
                .GetReferencedTypes()
                .Where(definedType => definedType.Name == type.Name && definedType.Namespace != type.Namespace)
                .ToArray();

            var namespaceParts = type.Namespace.Split('.').Reverse().ToArray();
            var namespacePart = "";
            for(var i= 0; i < namespaceParts.Length && conflictingTypes.Length > 0; i++)
            {
                namespacePart = namespaceParts[i] + "." + namespacePart;
                conflictingTypes = conflictingTypes
                    .Where(conflictingType => (conflictingType.Namespace + ".").EndsWith(namespacePart))
                    .ToArray();
            }
                
            return namespacePart + result;
        }

        private static string PrettyNameForGeneric(Type[] types, Assembly assembly)
        {
            var result = "";
            var delim = "<";
            foreach(var t in types)
            {
                result += delim;
                delim = ",";
                result += t.PrettyName(assembly);
            }
            return result + ">";
        }
    }
}