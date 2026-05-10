using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Like <see cref="EnableDumpExceptAttribute" />, but the "skip" decision is delegated to a
///     predicate method called on the member's current value. The predicate must be a parameterless
///     method returning <see cref="bool" />; the default is <c>IsDumpException</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
[PublicAPI]
public sealed class EnableDumpWithExceptionPredicateAttribute
    : DumpEnabledAttribute
        , IDumpExceptAttribute
{
    readonly string Predicate;

    /// <param name="predicate">
    ///     Name of a parameterless <see cref="bool" />-returning method on the member's value.
    ///     Defaults to <c>IsDumpException</c> when empty.
    /// </param>
    public EnableDumpWithExceptionPredicateAttribute(string predicate = "")
        : base(true)
        => Predicate = predicate == ""? "IsDumpException" : predicate;

    bool IDumpExceptAttribute.IsException(object? target)
    {
        try
        {
            return Equals(target?.GetType().GetMethod(Predicate)?.Invoke(target, null), true);
        }
        catch(Exception e)
        {
            Tracer.AssertionFailed("Exception when calling " + target?.GetType().PrettyName() + Predicate
                , () => e.Message);
            return false;
        }
    }
}