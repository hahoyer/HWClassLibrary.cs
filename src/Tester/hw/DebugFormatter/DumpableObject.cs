using System;
using System.Diagnostics;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

[AdditionalNodeInfo(nameof(NodeDump))]
[DebuggerDisplay("{" + nameof(NodeDump) + "}")]
public abstract class DumpableObject : Dumpable
{
    static int NextObjectId;

    [DisableDump]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [PublicAPI]
    public bool IsStopByObjectIdActive { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly int? ObjectIdValue;

    protected DumpableObject()
        : this(NextObjectId++) { }

    [PublicAPI]
    protected DumpableObject(int? nextObjectId) => ObjectIdValue = nextObjectId ?? NextObjectId++;

    protected virtual string GetNodeDump() => GetType().PrettyName();

    public override string ToString() => base.ToString() + " ObjectId=" + ObjectId;

    public override string DebuggerDump() => base.DebuggerDump() + " ObjectId=" + ObjectId;

    protected override string Dump(bool isRecursion)
    {
        var result = NodeDump;
        if(!isRecursion)
            result += DumpData().Surround("{", "}");
        return result;
    }

    [DisableDump]
    [PublicAPI]
    public int ObjectId
    {
        get
        {
            (ObjectIdValue != null).Assert();
            return ObjectIdValue.Value;
        }
    }

    [DisableDump]
    public string NodeDump
    {
        get
        {
            var result = GetNodeDump();
            if(ObjectIdValue == null)
                return result;
            return result + "." + ObjectId + "i";
        }
    }

    [PublicAPI]
    protected static string CallingMethodName
        => Debugger.IsAttached? Tracer.CallingMethodName(2) : "";

    [PublicAPI]
    public string NodeDumpForDebug() => Debugger.IsAttached? GetNodeDump() : "";

    [DebuggerHidden]
    [PublicAPI]
    public void StopByObjectIds(params int[] objectIds)
    {
        foreach(var objectId in objectIds)
            StopByObjectId(1, objectId);
    }

    [DebuggerHidden]
    void StopByObjectId(int stackFrameDepth, int objectId)
    {
        var isStopByObjectIdActive = IsStopByObjectIdActive;
        IsStopByObjectIdActive = true;
        if(ObjectId == objectId)
            Tracer.ConditionalBreak
                ("", () => @"ObjectId==" + ObjectId + "\n" + Dump(), stackFrameDepth + 1);
        IsStopByObjectIdActive = isStopByObjectIdActive;
    }
}

/// <summary>
///     Class attribute to define additional node info property, displayed after node title
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class AdditionalNodeInfoAttribute : Attribute
{
    /// <summary>
    ///     Property to obtain additional node info
    /// </summary>
    public string Property { get; }

    /// <summary>
    ///     Initializes a new instance of the AdditionalNodeInfoAttribute class.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// created 07.02.2007 00:47
    public AdditionalNodeInfoAttribute(string property) => Property = property;
}