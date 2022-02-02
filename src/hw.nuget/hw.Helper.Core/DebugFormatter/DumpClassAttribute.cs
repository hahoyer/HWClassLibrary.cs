using System;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Used to control dump.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public abstract class DumpClassAttribute : Attribute
{
    /// <summary>
    ///     override this function to define special dump behaviour of class
    /// </summary>
    /// <param name="type"> the type to dump. Is the type of any base class of "target" </param>
    /// <param name="target"> the object to dump </param>
    /// <returns> </returns>
    public abstract string Dump(Type type, object target);
}