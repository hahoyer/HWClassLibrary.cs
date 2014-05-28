using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace hw.ReplaceVariables
{
    public static class ReplaceVariablesExtension
    {
        static BindingFlags AnyBinding { get { return BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic; } }

        public static string ReplaceVariables(this string target, object replaceProvider)
        {
            var result = target.ReplaceVariables(replaceProvider, "");
            return result.Replace("$(=", "$(");
        }

        static string ReplaceVariables(this string target, object replaceProvider, string prefix)
        {
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
            var value = GetValue(definition, replaceProvider);
            return target
                .Replace("$(" + name + ")", value.ToString())
                .ReplaceVariables(value, name + ".");
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