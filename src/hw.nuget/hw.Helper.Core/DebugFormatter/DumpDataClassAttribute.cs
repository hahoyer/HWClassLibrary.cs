using System;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public abstract class DumpDataClassAttribute : Attribute
{
    public abstract string Dump(Type type, object target);
}