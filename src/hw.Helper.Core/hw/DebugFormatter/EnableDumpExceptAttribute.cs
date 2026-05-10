// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Skips dumping the decorated member when its current value equals the configured exception
///     value. Typical use: hide default flag values to keep the dump compact.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
public sealed class EnableDumpExceptAttribute
    : DumpExceptAttribute
        , IDumpExceptAttribute
{
    /// <param name="exceptionValue"> The value at which the member is omitted from the dump. </param>
    public EnableDumpExceptAttribute(object? exceptionValue)
        : base(exceptionValue) { }

    bool IDumpExceptAttribute.IsException(object? value) => IsException(value);
}

/// <summary>
///     Common base for value-based dump filters. Subclasses decide whether equality with
///     <see cref="ExceptionValue" /> means "skip" (<see cref="EnableDumpExceptAttribute" />) or
///     "render" (<see cref="DisableDumpExceptAttribute" />).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
public abstract class DumpExceptAttribute : DumpAttributeBase
{
    readonly object? ExceptionValue;

    /// <param name="exceptionValue"> The reference value that the current member value is compared against. </param>
    protected DumpExceptAttribute(object? exceptionValue) => ExceptionValue = exceptionValue;

    /// <summary>Tests whether <paramref name="targetValue" /> equals the configured exception value.</summary>
    protected bool IsException(object? targetValue) => Equals(targetValue, ExceptionValue);
}

/// <summary>
///     Dumps the decorated member only when its current value equals the configured exception value;
///     all other values are suppressed. The mirror image of <see cref="EnableDumpExceptAttribute" />.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
public sealed class DisableDumpExceptAttribute
    : DumpExceptAttribute
        , IDumpExceptAttribute
{
    /// <param name="exceptionValue"> The only value for which the member is rendered. </param>
    public DisableDumpExceptAttribute(object? exceptionValue)
        : base(exceptionValue) { }

    bool IDumpExceptAttribute.IsException(object? targetValue) => !IsException(targetValue);
}