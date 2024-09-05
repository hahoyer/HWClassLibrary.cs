using System;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[MeansImplicitUse]
[PublicAPI]
public sealed class EnableDumpWithExceptionPredicateAttribute
    : DumpEnabledAttribute
        , IDumpExceptAttribute
{
    readonly string Predicate;

    public EnableDumpWithExceptionPredicateAttribute(string predicate = "")
        : base(true)
        => Predicate = predicate == ""? "IsDumpException" : predicate;

    bool IDumpExceptAttribute.IsException(object target)
    {
        try
        {
            return Equals(target.GetType().GetMethod(Predicate)?.Invoke(target, null), true);
        }
        catch(Exception e)
        {
            Tracer.AssertionFailed("Exception when calling " + target.GetType().PrettyName() + Predicate
                , () => e.Message);
            return false;
        }
    }
}