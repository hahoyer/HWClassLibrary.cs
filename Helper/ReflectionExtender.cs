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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace HWClassLibrary.Helper
{
    public static class ReflectionExtender
    {
        [NotNull]
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type @this, bool inherit) where TAttribute : Attribute
        {
            return @this
                .GetCustomAttributes(typeof(TAttribute), inherit)
                .Cast<TAttribute>();
        }

        [NotNull]
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo @this, bool inherit)
            where TAttribute : Attribute
        {
            return @this
                .GetCustomAttributes(typeof(TAttribute), inherit)
                .Cast<TAttribute>();
        }

        [CanBeNull]
        public static TAttribute GetAttribute<TAttribute>(this Type @this, bool inherit) where TAttribute : Attribute
        {
            var list = GetAttributes<TAttribute>(@this, inherit).ToArray();
            switch(list.Length)
            {
                case 0:
                    return null;
                case 1:
                    return list[0];
            }
            throw new MultipleAttributesException(typeof(TAttribute), @this, inherit, list.ToArray());
        }

        [CanBeNull]
        public static TAttribute GetRecentAttribute<TAttribute>(this Type @this) where TAttribute : Attribute { return GetAttribute<TAttribute>(@this, false) ?? GetRecentAttributeBase<TAttribute>(@this.BaseType); }

        [CanBeNull]
        static TAttribute GetRecentAttributeBase<TAttribute>(this Type @this) where TAttribute : Attribute { return @this == null ? null : @this.GetRecentAttribute<TAttribute>(); }

        [CanBeNull]
        public static TAttribute GetAttribute<TAttribute>(this MemberInfo @this, bool inherit) where TAttribute : Attribute
        {
            var list = GetAttributes<TAttribute>(@this, inherit).ToArray();
            switch(list.Length)
            {
                case 0:
                    return null;
                case 1:
                    return list[0];
            }
            throw new MultipleAttributesException(typeof(TAttribute), @this, inherit, list.ToArray());
        }

        public class MultipleAttributesException : Exception
        {
            [UsedImplicitly]
            readonly Type _attributeType;

            [UsedImplicitly]
            readonly object _this;

            [UsedImplicitly]
            readonly bool _inherit;

            [UsedImplicitly]
            readonly Attribute[] _list;

            public MultipleAttributesException(Type attributeType, object @this, bool inherit, Attribute[] list)
            {
                _attributeType = attributeType;
                _this = @this;
                _inherit = inherit;
                _list = list;
            }
        }

        public static IEnumerable<Assembly> GetAssemblies(this Assembly rootAssembly)
        {
            var result = new[] {rootAssembly};
            for(IEnumerable<Assembly> referencedAssemblies = result;
                referencedAssemblies.GetEnumerator().MoveNext();
                result = result.Concat(referencedAssemblies).ToArray())
            {
                var assemblyNames = referencedAssemblies
                    .SelectMany(assembly => assembly.GetReferencedAssemblies()).ToArray();
                var assemblies = assemblyNames
                    .Select(AssemblyLoad).ToArray();
                var enumerable = assemblies
                    .Distinct().ToArray();
                referencedAssemblies = enumerable
                    .Where(assembly => !result.Contains(assembly)).ToArray();
            }
            return result;
        }

        public static IEnumerable<Type> GetReferencedTypes(this Assembly rootAssembly)
        {
            return rootAssembly
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());
        }


        public static Assembly AssemblyLoad(AssemblyName yy)
        {
            try
            {
                return AppDomain.CurrentDomain.Load(yy);
            }
            catch(Exception)
            {
                return Assembly.GetExecutingAssembly();
            }
        }

        public static Guid ToGuid(this object x)
        {
            if (x is DBNull || x == null)
                return Guid.Empty;
            return new Guid(x.ToString());
        }
        public static int ToInt32(this object x) { return x is DBNull ? 0 : x == null ? 0 : (int)x; }
        public static long ToInt64(this object x) { return x is DBNull ? 0 : x == null ? 0 : (long) x; }
        public static bool ToBoolean(this object x)
        {
            if(x is DBNull || x == null)
                return false;
            return (bool) x;
        }
        public static string ToSingular(this object x)
        {
            var plural = x.ToString();
            if (plural.EndsWith("Tables"))
                return plural.Substring(0, plural.Length - 1);
            if (plural.EndsWith("es"))
                return plural.Substring(0, plural.Length - 2);
            if (plural.EndsWith("s"))
                return plural.Substring(0, plural.Length - 1);
            Debugger.Break();
            return plural;
        }
    }
}