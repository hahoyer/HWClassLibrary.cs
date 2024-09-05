using System;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Used to control dump. Use ToString function
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DumpAttribute : DumpClassAttribute
{
    readonly string Name;

    public DumpAttribute(string name) => Name = name;

    /// <summary>
    ///     set "ToString" as dump behaviour of class
    /// </summary>
    /// <param name="type"> the type to dump. Is the type of any base class of "target" </param>
    /// <param name="target"> the object to dump </param>
    /// <returns> </returns>
    public override string Dump(Type type, object target)
    {
        try
        {
            var methodInfo = type.GetMethod(Name);
            return (string)methodInfo?.Invoke(target, null);
        }
        catch(Exception)
        {
            return type.ToString();
        }
    }
}