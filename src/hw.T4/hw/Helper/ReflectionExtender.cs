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
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace hw.Helper
{
    public static class ReflectionExtender
    {
        [NotNull]
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type @this, bool inherit) where TAttribute : Attribute { return @this.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>(); }

        [NotNull]
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo @this, bool inherit) where TAttribute : Attribute { return @this.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>(); }

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


        public sealed class MultipleAttributesException : Exception
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
            for(IEnumerable<Assembly> referencedAssemblies = result; referencedAssemblies.GetEnumerator().MoveNext(); result = result.Concat(referencedAssemblies).ToArray())
            {
                var assemblyNames = referencedAssemblies.SelectMany(assembly => assembly.GetReferencedAssemblies()).ToArray();
                var assemblies = assemblyNames.Select(AssemblyLoad).ToArray();
                var enumerable = assemblies.Distinct().ToArray();
                referencedAssemblies = enumerable.Where(assembly => !result.Contains(assembly)).ToArray();
            }
            return result;
        }

        public static IEnumerable<Type> GetReferencedTypes(this Assembly rootAssembly) { return rootAssembly.GetAssemblies().SelectMany(GetTypes); }

        static Type[] GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException exception)
            {
                throw new Exception(assembly.FullName + "\n" + exception.LoaderExceptions.Stringify("\n"));
            }
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
            if(x is DBNull || x == null)
                return Guid.Empty;
            return new Guid(x.ToString());
        }

        public static T Convert<T>(this object x) { return x is DBNull || x == null ? default(T) : (T) x; }

        static readonly bool[] _boolean = new[] {false, true};

        public static bool ToBoolean(this object x, string[] values)
        {
            for(var i = 0; i < values.Length; i++)
                if(values[i].Equals((string) x, StringComparison.OrdinalIgnoreCase))
                    return _boolean[i];
            throw new InvalidDataException();
        }

        public static DateTime ToDateTime(this object x) { return Convert<DateTime>(x); }
        public static Decimal ToDecimal(this object x) { return Convert<decimal>(x); }
        public static short ToInt16(this object x) { return Convert<short>(x); }
        public static int ToInt32(this object x) { return Convert<int>(x); }
        public static long ToInt64(this object x) { return Convert<long>(x); }
        public static bool ToBoolean(this object x) { return Convert<bool>(x); }
        public static Type ToType(this object x) { return Convert<Type>(x); }

        public static string ToSingular(this object x)
        {
            var plural = x.ToString();
            if(plural.EndsWith("Tables"))
                return plural.Substring(0, plural.Length - 1);
            if(plural.EndsWith("Types"))
                return plural.Substring(0, plural.Length - 1);
            if(plural.EndsWith("es"))
                return plural.Substring(0, plural.Length - 2);
            if(plural.EndsWith("s"))
                return plural.Substring(0, plural.Length - 1);
            return "OneOf" + plural;
        }

        public static bool Is(this Type type, Type otherType)
        {
            if(type == null)
                return false;
            if(type == otherType)
                return true;
            if(type.IsSubclassOf(otherType))
                return true;
            return type.GetInterfaces().Contains(otherType);
        }

        public static bool Is<T>(this Type type) { return Is(type, typeof(T)); }

        public static Type[] GetDirectInterfaces(this Type type) { return type.GetInterfaces().Except((type.BaseType ?? typeof(object)).GetInterfaces()).ToArray(); }

        public static Type GetGenericType(this Type type) { return type.IsGenericType ? type.GetGenericTypeDefinition() : null; }

        public static IEnumerable<Type> ThisAndBias(this Type type)
        {
            var t = type;
            while(t != null)
            {
                yield return t;
                t = t.BaseType;
            }
        }

        internal static IEnumerable<FieldInfo> GetFieldInfos(this Type type) { return type.ThisAndBias().SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)); }

        public static T Parse<T>(this string title) { return (T) Enum.Parse(typeof(T), title); }

        public static void ToStatement<T>(this T any) { }
    }
}