// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Delegates dump rendering of the decorated class or struct to a named parameterless instance method
///     that returns <see cref="string" /> (typically a custom Dump helper, but any method is accepted).
///     If the call throws, the type's name is used as a fallback.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DumpAttribute : DumpClassAttribute
{
    readonly string Name;

    /// <param name="name"> Name of the parameterless string-returning method to invoke on the target. </param>
    public DumpAttribute(string name) => Name = name;

    /// <inheritdoc />
    public override string Dump(Type type, object target)
    {
        try
        {
            var methodInfo = type.GetMethod(Name);
            return (string)methodInfo?.Invoke(target, null)!;
        }
        catch(Exception)
        {
            return type.ToString();
        }
    }
}