using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

        public static T Eval<T>(this Expression x)
        {
            return (T) Expression.Lambda(x)
                .Compile()
                .DynamicInvoke();
        }

        /// <summary>
        ///     Invoke a member method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T Invoke<T>(this object target, string method, params object[] args) { return (T) target.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, target, args); }
        /// <summary>
        ///     Invoke a static method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T Invoke<T>(this Type type, string method, params object[] args) { return ExceptionGuard(() => (T) type.InvokeMember(method, BindingFlags.InvokeMethod, null, null, args)); }

        /// <summary>
        ///     Calls a function. In case of exceptions, onError is called, if provided. Otherwise default value is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static T ExceptionGuard<T>(this Func<T> function, Func<Exception, T> onError = null)
        {
            try
            {
                return function();
            }
            catch(Exception exception)
            {
                return onError == null ? default(T) : onError(exception);
            }
        }

        /// <summary>
        ///     Calls an action. In case of exceptions, onError is called, if provided.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onError"></param>
        public static void ExceptionGuard(this Action action, Action<Exception> onError = null)
        {
            try
            {
                action();
            }
            catch(Exception exception)
            {
                if(onError != null)
                    onError(exception);
            }
        }

        public static Task<T> STAStart<T>(Func<T> func)
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
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static Task STAStart(Action func)
        {
            var result = new Task(func);
            var thread = new Thread(result.Start);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return result;
        }

        public static object InvokeValue(this object x, MemberInfo info)
        {
            var fi = info as FieldInfo;
            if(fi != null)
                return fi.GetValue(x);
            var pi = info as PropertyInfo;
            if(pi != null)
                return pi.GetValue(x, null);

            throw new FieldOrPropertyExpected(x, info);
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
}