using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
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
        private static TAttribute GetRecentAttributeBase<TAttribute>(this Type @this) where TAttribute : Attribute { return @this == null ? null : @this.GetRecentAttribute<TAttribute>(); }

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
            private readonly Type _attributeType;

            [UsedImplicitly]
            private readonly object _this;

            [UsedImplicitly]
            private readonly bool _inherit;

            [UsedImplicitly]
            private readonly Attribute[] _list;

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
                referencedAssemblies = referencedAssemblies
                    .SelectMany(assembly => assembly.GetReferencedAssemblies())
                    .Select(AssemblyLoad)
                    .Distinct()
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
    }
}