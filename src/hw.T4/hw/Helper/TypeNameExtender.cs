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

namespace hw.Helper
{
    public static class TypeNameExtender
    {
        static readonly ValueCache<TypeLibrary> _referencedTypesCache = new ValueCache<TypeLibrary>(ObtainReferencedTypes);

        public static void OnModuleLoaded() { _referencedTypesCache.IsValid = false; }

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

            var conflictingTypes = ReferencedTypes.ConflictingTypes(type);

            var namespaceParts = type.Namespace.Split('.').Reverse().ToArray();
            var namespacePart = "";
            for(var i = 0; i < namespaceParts.Length && conflictingTypes.Length > 0; i++)
            {
                namespacePart = namespaceParts[i] + "." + namespacePart;
                conflictingTypes = conflictingTypes.Where(conflictingType => ("." + conflictingType.Namespace + ".").EndsWith("." + namespacePart)).ToArray();
            }

            return namespacePart + result;
        }

        static TypeLibrary ObtainReferencedTypes()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            return new TypeLibrary(assembly.GetReferencedTypes());
        }

        static TypeLibrary ReferencedTypes { get { return _referencedTypesCache.Value; } }

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

    sealed class TypeLibrary
    {
        readonly Dictionary<string, IGrouping<string, Type>> _data;

        public TypeLibrary(IEnumerable<Type> types) { _data = types.GroupBy(t => t.Name, t => t).ToDictionary(t => t.Key, t => t); }

        internal Type[] ConflictingTypes(Type type)
        {
            IGrouping<string, Type> result;
            if(!_data.TryGetValue(type.Name, out result))
                return new Type[0];
            return result.Where(definedType => definedType.Namespace != type.Namespace).ToArray();
        }
    }
}