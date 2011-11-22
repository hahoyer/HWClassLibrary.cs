// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    public static class TypeNameExtender
    {
        static readonly SimpleCache<IEnumerable<Type>> _referencedTypesCache =
            new SimpleCache<IEnumerable<Type>>(ObtainReferencedTypes);

        public static void OnModuleLoaded() { _referencedTypesCache.Reset(); }

        public static string PrettyName(this Type type)
        {
            if(type == typeof(int))
                return "int";
            if(type == typeof(string))
                return "string";

            var result = PrettyTypeName(type);
            if(type.IsGenericType)
                result = result + PrettyNameForGeneric(type.GetGenericArguments());
            return result;
        }

        static string PrettyTypeName(Type type)
        {
            var result = type.Name;
            if(result.Contains("`"))
                result = result.Remove(result.IndexOf('`'));

            if(type.IsNested && !type.IsGenericParameter)
                return type.DeclaringType.PrettyName() + "." + result;

            if(type.Namespace == null)
                return result;

            var conflictingTypes = ReferencedTypes
                .Where(definedType => definedType.Name == type.Name && definedType.Namespace != type.Namespace)
                .ToArray();

            var namespaceParts = type.Namespace.Split('.').Reverse().ToArray();
            var namespacePart = "";
            for(var i = 0; i < namespaceParts.Length && conflictingTypes.Length > 0; i++)
            {
                namespacePart = namespaceParts[i] + "." + namespacePart;
                conflictingTypes = conflictingTypes
                    .Where(conflictingType => (conflictingType.Namespace + ".").EndsWith(namespacePart))
                    .ToArray();
            }

            return namespacePart + result;
        }

        static IEnumerable<Type> ObtainReferencedTypes()
        {
            var assembly =
                Assembly.GetEntryAssembly() ??
                Assembly.GetCallingAssembly();
            return assembly.GetReferencedTypes();
        }

        static IEnumerable<Type> ReferencedTypes { get { return _referencedTypesCache.Value; } }

        static string PrettyNameForGeneric(Type[] types)
        {
            var result = "";
            var delim = "<";
            foreach(var t in types)
            {
                result += delim;
                delim = ",";
                result += t.PrettyName();
            }
            return result + ">";
        }
    }
}