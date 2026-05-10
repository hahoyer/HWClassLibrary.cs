using hw.Helper;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>Convenience wrappers around <see cref="System.Reflection" /> APIs.</summary>
[PublicAPI]
public static class ReflectionExtender
{
    /// <summary>
    ///     Thrown by <see cref="GetAttribute{TAttribute}(Type, bool)" /> and friends when more than
    ///     one matching attribute is found and a single result was expected.
    /// </summary>
    public sealed class MultipleAttributesException : Exception
    {
        [UsedImplicitly]
        readonly Type AttributeType;

        [UsedImplicitly]
        readonly bool Inherit;

        [UsedImplicitly]
        readonly Attribute[] List;

        [UsedImplicitly]
        readonly object This;

        /// <param name="attributeType"> Attribute type that was queried. </param>
        /// <param name="target"> Reflection target on which the attributes were found. </param>
        /// <param name="inherit"> Whether the lookup walked the inheritance chain. </param>
        /// <param name="list"> The attributes that were found. </param>
        public MultipleAttributesException(Type attributeType, object target, bool inherit, Attribute[] list)
        {
            AttributeType = attributeType;
            This = target;
            Inherit = inherit;
            List = list;
        }
    }

    static readonly bool[] Boolean = [false, true];

    /// <summary>All attributes of type <typeparamref name="TAttribute" /> on <paramref name="target" />.</summary>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type target, bool inherit)
        => target.GetCustomAttributes(inherit).OfType<TAttribute>();

    /// <summary>All attributes of type <typeparamref name="TAttribute" /> on the <see cref="Enum" /> field declaring <paramref name="target" />.</summary>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Enum target)
        where TAttribute : Attribute
        => target
            .GetType()
            .GetMember(target.ToString())[0]
            .GetCustomAttributes(false)
            .OfType<TAttribute>();

    /// <summary>All attributes of type <typeparamref name="TAttribute" /> on <paramref name="target" />.</summary>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo target, bool inherit)
        => target.GetCustomAttributes(inherit).OfType<TAttribute>();

    /// <summary>
    ///     Returns the single attribute of type <typeparamref name="TAttribute" /> on
    ///     <paramref name="target" />, or <c>null</c> if absent.
    /// </summary>
    /// <exception cref="MultipleAttributesException"> More than one matching attribute was found. </exception>
    public static TAttribute? GetAttribute<TAttribute>(this Type target, bool inherit)
        where TAttribute : Attribute
    {
        var list = target.GetAttributes<TAttribute>(inherit).ToArray();
        switch(list.Length)
        {
            case 0:
                return default;
            case 1:
                return list[0];
        }

        throw new MultipleAttributesException(typeof(TAttribute), target, inherit
            , list.Cast<Attribute>().ToArray());
    }

    /// <summary>
    ///     Returns the single attribute of type <typeparamref name="TAttribute" /> on the enum
    ///     value's declaring field, or <c>null</c> if absent or ambiguous.
    /// </summary>
    public static TAttribute? GetAttribute<TAttribute>(this Enum target)
        where TAttribute : Attribute
        => target.GetAttributes<TAttribute>().SingleOrDefault();

    /// <summary>
    ///     Looks for an attribute first on <paramref name="target" /> itself and then up the base-type
    ///     chain, returning the first match. Used when subtypes may override an attribute but only
    ///     the most-derived one should apply.
    /// </summary>
    public static TAttribute? GetRecentAttribute<TAttribute>(this Type target)
        where TAttribute : Attribute
        => target.GetAttribute<TAttribute>(false) ?? target.BaseType?.GetRecentAttributeBase<TAttribute>();

    /// <inheritdoc cref="GetAttribute{TAttribute}(Type, bool)" />
    public static TAttribute? GetAttribute<TAttribute>(this MemberInfo target, bool inherit)
        where TAttribute : Attribute
    {
        var list = target.GetAttributes<TAttribute>(inherit).ToArray();
        switch(list.Length)
        {
            case 0:
                return default;
            case 1:
                return list[0];
        }

        throw new MultipleAttributesException(typeof(TAttribute), target, inherit
            , list.Cast<Attribute>().ToArray());
    }

    /// <summary>
    ///     Walks the transitive closure of assemblies reachable from <paramref name="rootAssembly" />
    ///     via <see cref="Assembly.GetReferencedAssemblies" />. Assemblies that fail to load are
    ///     replaced by the executing assembly so the walk can continue.
    /// </summary>
    public static IEnumerable<Assembly> GetAssemblies(this Assembly rootAssembly)
    {
        var result = new[] { rootAssembly };

        for(var referencedAssemblies = result; referencedAssemblies.Any();)
        {
            referencedAssemblies = referencedAssemblies
                .SelectMany(assembly => assembly.GetReferencedAssemblies())
                .Select(AssemblyLoad)
                .Distinct()
                .Where(assembly => !result.Contains(assembly))
                .ToArray();
            result = result.Concat(referencedAssemblies).ToArray();
        }

        return result;
    }

    /// <summary>All types declared in any assembly reachable from <paramref name="rootAssembly" />.</summary>
    public static IEnumerable<Type> GetReferencedTypes
        (this Assembly rootAssembly) => rootAssembly.GetAssemblies().SelectMany(GetTypes);


    /// <summary>
    ///     Loads <paramref name="yy" /> from the current <see cref="AppDomain" />, falling back to
    ///     the executing assembly when the load fails (e.g. because the dependency is missing on
    ///     this platform).
    /// </summary>
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

    /// <summary>
    ///     Parses a database value as a <see cref="Guid" />. Maps <see cref="DBNull" /> to
    ///     <see cref="Guid.Empty" /> rather than throwing.
    /// </summary>
    // ReSharper disable once RedundantSuppressNullableWarningExpression
    // ToString-method changed in new releases th string?
    public static Guid ToGuid(this object target) => target is DBNull? Guid.Empty : new(target.ToString()!);

    /// <summary>
    ///     Casts a database value to <typeparamref name="T" />. <see cref="DBNull" /> is converted to
    ///     <c>default(T)</c> rather than thrown on.
    /// </summary>
    public static T? Convert<T>(this object target) => target is DBNull? default : (T)target;

    /// <summary>
    ///     Decodes a string-coded boolean using the lookup pair <paramref name="values" />:
    ///     index 0 → <c>false</c>, index 1 → <c>true</c>. Comparison is case-insensitive.
    /// </summary>
    /// <exception cref="InvalidDataException"> <paramref name="target" /> matches neither entry. </exception>
    public static bool ToBoolean(this object target, string[] values)
    {
        for(var i = 0; i < values.Length; i++)
            if(values[i].Equals((string)target, StringComparison.OrdinalIgnoreCase))
                return Boolean[i];
        throw new InvalidDataException();
    }

    /// <summary>Converts a database value to <see cref="DateTime" />, mapping <see cref="DBNull" /> to <c>default</c>.</summary>
    public static DateTime ToDateTime(this object target) => target.Convert<DateTime>();

    /// <summary>Converts a database value to <see cref="decimal" />, mapping <see cref="DBNull" /> to <c>default</c>.</summary>
    public static decimal ToDecimal(this object target) => target.Convert<decimal>();

    /// <summary>Converts a database value to <see cref="short" />, mapping <see cref="DBNull" /> to <c>default</c>.</summary>
    public static short ToInt16(this object target) => target.Convert<short>();

    /// <summary>Converts a database value to <see cref="int" />, mapping <see cref="DBNull" /> to <c>default</c>.</summary>
    public static int ToInt32(this object target) => target.Convert<int>();

    /// <summary>Converts a database value to <see cref="long" />, mapping <see cref="DBNull" /> to <c>default</c>.</summary>
    public static long ToInt64(this object target) => target.Convert<long>();

    /// <summary>Converts a database value to <see cref="bool" />, mapping <see cref="DBNull" /> to <c>default</c>.</summary>
    public static bool ToBoolean(this object target) => target.Convert<bool>();

    /// <summary>Converts a database value to <see cref="Type" />, mapping <see cref="DBNull" /> to <c>null</c>.</summary>
    public static Type? ToType(this object target) => target.Convert<Type>();

    /// <summary>
    ///     Naive English singularisation of <paramref name="target" />'s string form. Handles a few
    ///     ad-hoc cases (<c>Tables</c>, <c>Types</c>, generic <c>es</c>/<c>s</c> endings); otherwise
    ///     prefixes with <c>OneOf</c>. Intended for code generation, not for prose.
    /// </summary>
    public static string ToSingular(this object target)
    {
        // ReSharper disable once RedundantSuppressNullableWarningExpression
        // ToString-method changed in new releases th string?
        var plural = target.ToString()!;
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

    /// <summary>
    ///     <c>true</c> if <paramref name="type" /> equals, derives from, or implements
    ///     <paramref name="otherType" />.
    /// </summary>
    public static bool Is(this Type? type, Type otherType)
    {
        if(type == null)
            return false;
        if(type == otherType)
            return true;
        if(type.IsSubclassOf(otherType))
            return true;
        return type.GetInterfaces().Contains(otherType);
    }

    /// <inheritdoc cref="Is(Type?, Type)" />
    public static bool Is<T>(this Type type) => type.Is(typeof(T));

    /// <summary>Interfaces declared directly on <paramref name="type" />, excluding those inherited from its base class.</summary>
    public static Type[] GetDirectInterfaces
        (this Type type)
        => type.GetInterfaces().Except((type.BaseType ?? typeof(object)).GetInterfaces()).ToArray();

    /// <summary>The generic type definition of <paramref name="type" />, or <c>null</c> if it is not a closed generic.</summary>
    public static Type? GetGenericType(this Type type) => type.IsGenericType? type.GetGenericTypeDefinition() : null;

    /// <summary>Walks the inheritance chain starting at <paramref name="type" /> through every base type up to (but excluding) <c>null</c>.</summary>
    public static IEnumerable<Type> ThisAndBias(this Type type)
    {
        var t = type;
        while(t != null)
        {
            yield return t;
            t = t.BaseType;
        }
    }

    /// <summary>Parses <paramref name="title" /> as an enum value of type <typeparamref name="T" />.</summary>
    public static T Parse<T>(this string title) => (T)Enum.Parse(typeof(T), title);

    /// <summary>
    ///     No-op that consumes <paramref name="any" /> as a statement. Lets expressions like
    ///     <c>foo.Bar.ToStatement()</c> compile where the expression value is not otherwise needed.
    /// </summary>
    public static void ToStatement<T>(this T any) { }

    /// <summary>Compiles <paramref name="target" /> into a delegate, invokes it, and casts the result to <typeparamref name="T" />.</summary>
    public static T Eval<T>(this Expression target) => (T)Expression.Lambda(target)
        .Compile()
        .DynamicInvoke()!;

    /// <summary>Invokes the named instance method via reflection and casts the result to <typeparamref name="T" />.</summary>
    /// <typeparam name="T"> Expected return type. </typeparam>
    /// <param name="target"> Receiver of the call. </param>
    /// <param name="method"> Method name. </param>
    /// <param name="args"> Arguments. </param>
    public static T Invoke<T>(this object target, string method, params object[] args) => (T)target.GetType()
        .InvokeMember(method, BindingFlags.InvokeMethod, null, target, args)!;

    /// <summary>
    ///     Invokes the named static method via reflection and casts the result to
    ///     <typeparamref name="T" />. Wrapped in <see cref="ExceptionGuard{T}" /> so a failed lookup
    ///     yields <c>default</c> rather than throwing.
    /// </summary>
    /// <typeparam name="T"> Expected return type. </typeparam>
    /// <param name="type"> Type that declares the static method. </param>
    /// <param name="method"> Method name. </param>
    /// <param name="args"> Arguments. </param>
    public static T? Invoke<T>(this Type type, string method, params object[] args)
        => ExceptionGuard(
            ()
                => (T)type.InvokeMember(method, BindingFlags.InvokeMethod, null, null, args)!
        );

    /// <summary>
    ///     Invokes <paramref name="function" /> and turns any exception into either a default value or
    ///     the result of <paramref name="onError" /> (when supplied).
    /// </summary>
    /// <typeparam name="T"> Function result type. </typeparam>
    /// <param name="function"> Function to invoke. </param>
    /// <param name="onError"> Optional fallback called with the thrown exception. </param>
    public static T? ExceptionGuard<T>(this Func<T> function, Func<Exception, T>? onError = null)
    {
        try
        {
            return function();
        }
        catch(Exception exception)
        {
            return onError == null? default : onError(exception);
        }
    }

    /// <summary>
    ///     Invokes <paramref name="action" /> and forwards any exception to <paramref name="onError" />
    ///     (or swallows it when no handler is supplied).
    /// </summary>
    /// <param name="action"> Action to invoke. </param>
    /// <param name="onError"> Optional fallback called with the thrown exception. </param>
    public static void ExceptionGuard(this Action action, Action<Exception>? onError = null)
    {
        try
        {
            action();
        }
        catch(Exception exception)
        {
            onError?.Invoke(exception);
        }
    }

    /// <summary>
    ///     Runs <paramref name="func" /> on a fresh STA thread (Windows only) and returns a
    ///     <see cref="Task{T}" /> that completes when it finishes. Required for COM interop with
    ///     apartment-affine APIs.
    /// </summary>
    public static Task<T> StaStart<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        var thread = new Thread(() =>
        {
            try
            {
                tcs.SetResult(func());
            }
            catch(Exception e)
            {
                tcs.SetException(e);
            }
        });
#pragma warning disable CA1416 //This call site is reachable on all platforms. 'Thread.SetApartmentState(ApartmentState)' is only supported on: 'windows'.
        thread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416
        thread.Start();
        return tcs.Task;
    }

    /// <inheritdoc cref="StaStart{T}(Func{T})" />
    public static Task StaStart(Action func)
    {
        var result = new Task(func);
        var thread = new Thread(result.Start);
#pragma warning disable CA1416 //This call site is reachable on all platforms. 'Thread.SetApartmentState(ApartmentState)' is only supported on: 'windows'.
        thread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416


        thread.Start();
        return result;
    }

    /// <summary>
    ///     Reads the value of a field or property on <paramref name="target" /> via reflection.
    /// </summary>
    /// <exception cref="FieldOrPropertyExpected"> <paramref name="info" /> describes neither. </exception>
    public static object? InvokeValue(this object target, MemberInfo info)
    {
        var fi = info as FieldInfo;
        if(fi != null)
            return fi.GetValue(target);
        var pi = info as PropertyInfo;
        if(pi != null)
            return pi.GetValue(target, null);

        throw new FieldOrPropertyExpected(target, info);
    }

    internal static IEnumerable<FieldInfo> GetFieldInfos(this Type type) => type.ThisAndBias().SelectMany(t
        => t.GetFields(BindingFlags.Public
            | BindingFlags.Instance
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly));

    static TAttribute? GetRecentAttributeBase<TAttribute>(this Type target)
        where TAttribute : Attribute
        => target.GetRecentAttribute<TAttribute>();

    static Type[] GetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch(ReflectionTypeLoadException exception)
        {
            throw new(assembly.FullName + "\n" + exception.LoaderExceptions.Stringify("\n"));
        }
    }
}

sealed class FieldOrPropertyExpected : Exception
{
    [UsedImplicitly]
    public new readonly object Data;

    [UsedImplicitly]
    public readonly MemberInfo MemberInfo;

    internal FieldOrPropertyExpected(object data, MemberInfo memberInfo)
    {
        Data = data;
        MemberInfo = memberInfo;
    }
}