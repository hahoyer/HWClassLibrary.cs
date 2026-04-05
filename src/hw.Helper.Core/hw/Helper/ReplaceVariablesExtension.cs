using System.Reflection;

namespace hw.Helper;

public static class ReplaceVariablesExtension
{
    extension(string target)
    {
        public string ReplaceVariables(object values, IReplaceProvider? provider = null)
            => target.ReplaceVariables(values, "", provider);

        public string ReplaceVariables<TAttribute>(object values, int brackets = 1)
            where TAttribute : Attribute
            => target.ReplaceVariables<TAttribute>(values, "", brackets);

        public string ReplaceProtected(int brackets = 1)
            => target.Replace($"{O(brackets)}=", $"{O(brackets)}");

        public string ReplaceVariables<TAttribute>(object values, string prefix, int brackets = 1)
            where TAttribute : Attribute
            => target.ReplaceVariables(values, prefix, new Tagged<TAttribute>(brackets));

        public string ReplaceVariables(object values, string prefix, IReplaceProvider? provider = null)
        {
            provider ??= new Provider();
            var type = values.GetType();

            return type
                .GetFields(AnyBinding)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(AnyBinding))
                .Where(provider.IsVisible)
                .Aggregate(target, (s, info) => Replace(s, info, values, prefix, provider));
        }
    }

    [PublicAPI]
    public sealed class Provider(Func<MemberInfo, bool>? isVisible = null, int brackets = 1)
        : IReplaceProvider
    {
        readonly Func<MemberInfo, bool> IsVisible = isVisible ?? (_ => true);

        int IReplaceProvider.Brackets => brackets;
        bool IReplaceProvider.IsVisible(MemberInfo memberInfo) => IsVisible(memberInfo);
    }

    [PublicAPI]
    public sealed class Tagged<TAttribute>(int brackets = 1)
        : IReplaceProvider
        where TAttribute : Attribute
    {
        int IReplaceProvider.Brackets => brackets;

        bool IReplaceProvider.IsVisible(MemberInfo memberInfo)
            => memberInfo.GetCustomAttribute<TAttribute>(true) != null;
    }

    static BindingFlags AnyBinding => BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

    static string O(int brackets) => "{".Repeat(brackets);
    static string C(int brackets) => "}".Repeat(brackets);

    static string Replace(string target, MemberInfo definition, object values, string prefix, IReplaceProvider provider)
    {
        var brackets = provider.Brackets;
        var name = prefix + definition.Name;
        var hasDirectReference = target.Contains($"{O(brackets)}{name}{C(brackets)}");
        var mayHaveIndirectReference = target.Contains($"{O(brackets)}{name}.");

        if(!hasDirectReference && !mayHaveIndirectReference)
            return target;

        var value = GetValue(definition, values);

        if(hasDirectReference)
            target = target.Replace($"{O(brackets)}{name}{C(brackets)}", value?.ToString());
        if(mayHaveIndirectReference && value != null)
            target = target.ReplaceVariables(value, $"{name}.");
        return target;
    }

    static object? GetValue(MemberInfo definition, object values)
    {
        var fieldInfo = definition as FieldInfo;
        if(fieldInfo != null)
            return fieldInfo.GetValue(values);
        var propertyInfo = definition as PropertyInfo;
        if(propertyInfo != null)
            return propertyInfo.GetValue(values);
        return null;
    }
}

public interface IReplaceProvider
{
    bool IsVisible(MemberInfo memberInfo);
    int Brackets { get; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[MeansImplicitUse]
public sealed class VisibleAttribute : Attribute { }