using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Type-safe enum pattern: derive a sealed class with <c>public static readonly</c> fields of
///     the type, and each field becomes a "value" identifiable by its declaring name. Useful when
///     enum members need to carry behaviour or extra data that a regular <see langword="enum" />
///     cannot express. Decorate fields that are not part of the enum with
///     <see cref="NotAnEnumInstanceAttribute" />.
/// </summary>
[PublicAPI]
public abstract class EnumEx : Dumpable
{
    /// <summary>
    ///     Name of the static field that this instance was declared as, walking the inheritance
    ///     chain. Returns <c>null</c> when the instance was constructed but never assigned to a
    ///     static field.
    /// </summary>
    public virtual string? Tag
    {
        get
        {
            for(var type = GetType(); type != null; type = type.BaseType)
            {
                var instance = AllMemberInfos(type).SingleOrDefault(t => Instance(t) == this);
                if(instance != null)
                    return instance.Name;
            }

            return null;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Tag ?? "";

    /// <inheritdoc />
    protected override string Dump(bool isRecursion) => GetType().PrettyName() + "." + Tag;

    /// <summary>Discovers every <c>public static readonly</c> field of <typeparamref name="T" /> declared in <typeparamref name="T" /> or any base type.</summary>
    protected static IEnumerable<T> AllInstances<T>() => AllInstances(typeof(T)).Cast<T>();

    static IEnumerable<MemberInfo> AllMemberInfos(Type type)
    {
        var memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        return memberInfos.Where(memberInfo => IsValue(memberInfo, type));
    }

    static IEnumerable<EnumEx?> AllInstances(Type type) => AllMemberInfos(type).Select(Instance);
    static EnumEx? Instance(MemberInfo memberInfo) => (EnumEx?)((FieldInfo)memberInfo).GetValue(null);

    static bool IsValue(MemberInfo memberInfo, Type? type)
    {
        if(type == null)
            return false;
        if(memberInfo.DeclaringType != type)
            return false;
        if(memberInfo.MemberType != MemberTypes.Field)
            return false;
        if(memberInfo.GetAttribute<NotAnEnumInstanceAttribute>(false) != null)
            return false;
        var fieldInfo = memberInfo as FieldInfo;
        if(fieldInfo == null)
            return false;
        if(fieldInfo.FieldType != type && !fieldInfo.FieldType.IsSubclassOf(type))
            return false;
        if((fieldInfo.Attributes & FieldAttributes.Static) == 0)
            return false;
        return true;
    }
}

/// <summary>
///     Excludes a <c>public static readonly</c> field from the <see cref="EnumEx.AllInstances{T}" />
///     enumeration when it should not count as one of the enum's values.
/// </summary>
public sealed class NotAnEnumInstanceAttribute : Attribute { }