using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.ReplaceVariables
{
    public static class ReplaceVariablesExtension
    {
        static BindingFlags AnyBinding { get { return BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic; } }

        public static string ReplaceVariables(this string target, object replaceProvider) { return target.ReplaceVariables(replaceProvider, ""); }
        public static string ReplaceProtected(this string target) { return target == null ? null : target.Replace("$(=", "$("); }

        static string ReplaceVariables(this string target, object replaceProvider, string prefix)
        {
            if(target == null)
                return null;
            var type = replaceProvider.GetType();
            return type
                .GetFields(AnyBinding)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(AnyBinding))
                .Where(m => m.GetCustomAttribute<VisibleAttribute>(true) != null)
                .Aggregate(target, (s, info) => Replace(s, info, replaceProvider, prefix));
        }

        static string Replace(string target, MemberInfo definition, object replaceProvider, string prefix)
        {
            var name = prefix + definition.Name;
            var hasDirectReference = target.Contains("$(" + name + ")");
            var mayHaveIndirectReference = target.Contains("$(" + name + ".");

            if(!hasDirectReference && !mayHaveIndirectReference)
                return target;

            var value = GetValue(definition, replaceProvider);

            if(hasDirectReference)
                target = target.Replace("$(" + name + ")", value.ToString());
            if(mayHaveIndirectReference)
                target = target.ReplaceVariables(value, name + ".");
            return target;
        }

        static object GetValue(MemberInfo definition, object replaceProvider)
        {
            var fieldInfo = definition as FieldInfo;
            if(fieldInfo != null)
                return fieldInfo.GetValue(replaceProvider);
            var propertyInfo = definition as PropertyInfo;
            if(propertyInfo != null)
                return propertyInfo.GetValue(replaceProvider);
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    [MeansImplicitUse]
    public sealed class VisibleAttribute : Attribute
    {}
}