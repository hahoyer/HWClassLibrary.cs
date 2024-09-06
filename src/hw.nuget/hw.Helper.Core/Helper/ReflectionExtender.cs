﻿#nullable enable
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public static class ReflectionExtender
{
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

        public MultipleAttributesException(Type attributeType, object target, bool inherit, Attribute[] list)
        {
            AttributeType = attributeType;
            This = target;
            Inherit = inherit;
            List = list;
        }
    }

    static readonly bool[] Boolean = [false, true];

    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type target, bool inherit)
        => target.GetCustomAttributes(inherit).OfType<TAttribute>();

    public static IEnumerable<T> GetAttributes<T>(this Enum target)
        => target
            .GetType()
            .GetMember(target.ToString())[0]
            .GetCustomAttributes(false)
            .OfType<T>();

    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo target, bool inherit)
        => target.GetCustomAttributes(inherit).OfType<TAttribute>();

    public static TAttribute? GetAttribute<TAttribute>(this Type target, bool inherit)
    {
        var list = GetAttributes<TAttribute>(target, inherit).ToArray();
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

    public static TAttribute? GetAttribute<TAttribute>(this Enum target)
        => GetAttributes<TAttribute>(target).SingleOrDefault();

    public static TAttribute? GetRecentAttribute<TAttribute>(this Type target)
        => GetAttribute<TAttribute>(target, false) ?? GetRecentAttributeBase<TAttribute>(target.BaseType!);

    public static TAttribute? GetAttribute<TAttribute>(this MemberInfo target, bool inherit)

    {
        var list = GetAttributes<TAttribute>(target, inherit).ToArray();
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

    public static IEnumerable<Type> GetReferencedTypes
        (this Assembly rootAssembly) => rootAssembly.GetAssemblies().SelectMany(GetTypes);


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

    public static Guid ToGuid(this object target) => target is DBNull? Guid.Empty : new(target.ToString()!);

    public static T? Convert<T>(this object target) => target is DBNull? default : (T)target;

    public static bool ToBoolean(this object target, string[] values)
    {
        for(var i = 0; i < values.Length; i++)
            if(values[i].Equals((string)target, StringComparison.OrdinalIgnoreCase))
                return Boolean[i];
        throw new InvalidDataException();
    }

    public static DateTime ToDateTime(this object target) => Convert<DateTime>(target);
    public static decimal ToDecimal(this object target) => Convert<decimal>(target);
    public static short ToInt16(this object target) => Convert<short>(target);
    public static int ToInt32(this object target) => Convert<int>(target);
    public static long ToInt64(this object target) => Convert<long>(target);
    public static bool ToBoolean(this object target) => Convert<bool>(target);
    public static Type? ToType(this object target) => Convert<Type>(target);

    public static string ToSingular(this object target)
    {
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

    public static bool Is<T>(this Type type) => Is(type, typeof(T));

    public static Type[] GetDirectInterfaces
        (this Type type)
        => type.GetInterfaces().Except((type.BaseType ?? typeof(object)).GetInterfaces()).ToArray();

    public static Type? GetGenericType(this Type type) => type.IsGenericType? type.GetGenericTypeDefinition() : null;

    public static IEnumerable<Type> ThisAndBias(this Type type)
    {
        var t = type;
        while(t != null)
        {
            yield return t;
            t = t.BaseType;
        }
    }

    public static T Parse<T>(this string title) => (T)Enum.Parse(typeof(T), title);

    public static void ToStatement<T>(this T any) { }

    public static T Eval<T>(this Expression target) => (T)Expression.Lambda(target)
        .Compile()
        .DynamicInvoke()!;

    /// <summary>
    ///     Invoke a member method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static T Invoke<T>(this object target, string method, params object[] args) => (T)target.GetType()
        .InvokeMember(method, BindingFlags.InvokeMethod, null, target, args)!;

    /// <summary>
    ///     Invoke a static method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static T? Invoke<T>(this Type type, string method, params object[] args)
        => ExceptionGuard(
            ()
                => (T)type.InvokeMember(method, BindingFlags.InvokeMethod, null, null, args)!
        );

    /// <summary>
    ///     Calls a function. In case of exceptions, onError is called, if provided. Otherwise, default value is returned
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="function"></param>
    /// <param name="onError"></param>
    /// <returns></returns>
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
    ///     Calls an action. In case of exceptions, onError is called, if provided.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="onError"></param>
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