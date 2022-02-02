using System;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Used to control dump of data element
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
public sealed class EnableDumpExceptAttribute
    : DumpExceptAttribute
        , IDumpExceptAttribute
{
    /// <summary>
    ///     Set exception for value tha will not be dumped
    /// </summary>
    /// <param name="exceptionValue"> dump this property or not </param>
    public EnableDumpExceptAttribute(object exceptionValue)
        : base(exceptionValue) { }

    bool IDumpExceptAttribute.IsException(object v) => IsException(v);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
public abstract class DumpExceptAttribute : DumpAttributeBase
{
    readonly object ExceptionValue;

    protected DumpExceptAttribute(object exceptionValue) => ExceptionValue = exceptionValue;

    protected bool IsException(object targetValue) => Equals(targetValue, ExceptionValue);
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
public sealed class DisableDumpExceptAttribute
    : DumpExceptAttribute
        , IDumpExceptAttribute
{
    /// <summary>
    ///     Set exception for value tha will not be dumped
    /// </summary>
    /// <param name="exceptionValue"> dump this property or not </param>
    public DisableDumpExceptAttribute(object exceptionValue)
        : base(exceptionValue) { }

    bool IDumpExceptAttribute.IsException(object targetValue) => !IsException(targetValue);
}