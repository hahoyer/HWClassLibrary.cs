using System.Reflection;

#pragma warning disable IDE0130
namespace hw.Helper;

/// <summary>
///     Curly-brace template engine: scans a string for placeholders like <c>{Name}</c> or
///     <c>{Outer.Inner}</c> and substitutes them with the values of the matching members of an
///     object. The placeholder bracket style is configurable (<c>{X}</c>, <c>{{X}}</c>, …).
/// </summary>
public static class ReplaceVariablesExtension
{
    extension(string target)
    {
        /// <summary>Replaces placeholders in <paramref name="target" /> with the values from <paramref name="values" />.</summary>
        /// <param name="values"> Object whose fields/properties supply replacement values. </param>
        /// <param name="provider"> Optional filter selecting which members are visible. </param>
        public string ReplaceVariables(object values, IReplaceProvider? provider = null)
            => target.ReplaceVariables(values, "", provider);

        /// <summary>
        ///     Variant that only considers members marked with <typeparamref name="TAttribute" />.
        /// </summary>
        /// <param name="values"> Object whose fields/properties supply replacement values. </param>
        /// <param name="brackets"> Number of opening/closing braces around placeholders (1 → <c>{X}</c>, 2 → <c>{{X}}</c>). </param>
        public string ReplaceVariables<TAttribute>(object values, int brackets = 1)
            where TAttribute : Attribute
            => target.ReplaceVariables<TAttribute>(values, "", brackets);

        /// <summary>
        ///     Removes the leading <c>=</c> from "protected" placeholders (e.g. <c>{=X}</c> →
        ///     <c>{X}</c>), used to escape literals that should pass through a first replacement
        ///     pass and only resolve later.
        /// </summary>
        public string ReplaceProtected(int brackets = 1)
            => target.Replace($"{O(brackets)}=", $"{O(brackets)}");

        /// <summary>
        ///     Attribute-filtered replacement with a name prefix, for resolving nested placeholders
        ///     such as <c>{outer.inner}</c>.
        /// </summary>
        /// <param name="values"> Object whose fields/properties supply replacement values. </param>
        /// <param name="prefix"> Prefix prepended to each member name when matching placeholders. </param>
        /// <param name="brackets"> Number of opening/closing braces around placeholders. </param>
        public string ReplaceVariables<TAttribute>(object values, string prefix, int brackets = 1)
            where TAttribute : Attribute
            => target.ReplaceVariables(values, prefix, new Tagged<TAttribute>(brackets));

        /// <summary>
        ///     Replacement with a name prefix; same shape as <see cref="ReplaceVariables(object, IReplaceProvider?)" />
        ///     but used during recursive descent into nested objects.
        /// </summary>
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

    /// <summary>
    ///     Default <see cref="IReplaceProvider" /> implementation: an arbitrary visibility predicate
    ///     and a configurable bracket count.
    /// </summary>
    /// <param name="isVisible"> Predicate selecting visible members; defaults to "all". </param>
    /// <param name="brackets"> Number of opening/closing braces around placeholders. </param>
    [PublicAPI]
    public sealed class Provider(Func<MemberInfo, bool>? isVisible = null, int brackets = 1)
        : IReplaceProvider
    {
        readonly Func<MemberInfo, bool> IsVisible = isVisible ?? (_ => true);

        int IReplaceProvider.Brackets => brackets;
        bool IReplaceProvider.IsVisible(MemberInfo memberInfo) => IsVisible(memberInfo);
    }

    /// <summary>
    ///     Provider that exposes only members marked with <typeparamref name="TAttribute" />.
    ///     Pair with <see cref="VisibleAttribute" /> for an opt-in template surface.
    /// </summary>
    /// <typeparam name="TAttribute"> The marker attribute. </typeparam>
    /// <param name="brackets"> Number of opening/closing braces around placeholders. </param>
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

/// <summary>Configuration of a single replacement pass for <see cref="ReplaceVariablesExtension" />.</summary>
public interface IReplaceProvider
{
    /// <summary>Whether <paramref name="memberInfo" /> should be considered for substitution.</summary>
    bool IsVisible(MemberInfo memberInfo);

    /// <summary>Number of opening/closing braces around placeholders (1 → <c>{X}</c>, 2 → <c>{{X}}</c>).</summary>
    int Brackets { get; }
}

/// <summary>
///     Marks a field or property as participating in <see cref="ReplaceVariablesExtension" />
///     templates when used together with <see cref="ReplaceVariablesExtension.Tagged{TAttribute}" />.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[MeansImplicitUse]
public sealed class VisibleAttribute : Attribute { }