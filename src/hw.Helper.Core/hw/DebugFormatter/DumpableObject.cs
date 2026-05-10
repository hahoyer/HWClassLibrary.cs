using System.Diagnostics;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     <see cref="Dumpable" /> with a per-instance numeric identity. Each instance gets a unique
///     <see cref="ObjectId" /> at construction time, which makes individual objects identifiable in
///     dumps and supports targeted debugger breaks via <see cref="StopByObjectIds" />.
/// </summary>
[AdditionalNodeInfo(nameof(NodeDump))]
[DebuggerDisplay("{" + nameof(NodeDump) + "}")]
public abstract class DumpableObject : Dumpable
{
    static int NextObjectId;

    /// <summary>
    ///     <c>true</c> while this instance is inside a <see cref="StopByObjectIds" /> evaluation, used
    ///     to keep the break logic re-entrancy-safe.
    /// </summary>
    [DisableDump]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [PublicAPI]
    public bool IsStopByObjectIdActive { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly int? ObjectIdValue;

    /// <summary>Stable identity assigned to this instance on construction.</summary>
    [DisableDump]
    [PublicAPI]
    public int ObjectId => ObjectIdValue.AssertValue();

    /// <summary>
    ///     Short label for this instance — type name plus <see cref="ObjectId" /> — used as the node
    ///     title in tree dumps and the debugger display.
    /// </summary>
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

    /// <summary>
    ///     Name of the method two stack frames above the caller (i.e. the method that called the
    ///     property accessor). Returns the empty string when no debugger is attached.
    /// </summary>
    [PublicAPI]
    protected static string CallingMethodName
        => Debugger.IsAttached? Tracer.CallingMethodName(2) : "";

    /// <summary>Allocates the next available <see cref="ObjectId" />.</summary>
    protected DumpableObject()
        : this(NextObjectId++) { }

    /// <summary>
    ///     Allows derived types to share an existing identity (pass <c>null</c> to allocate a fresh
    ///     <see cref="ObjectId" />).
    /// </summary>
    /// <param name="nextObjectId"> Identity to assume; <c>null</c> means allocate a new one. </param>
    [PublicAPI]
    protected DumpableObject(int? nextObjectId) => ObjectIdValue = nextObjectId ?? NextObjectId++;

    /// <summary>
    ///     Override to customise the node title shown in tree dumps. Defaults to the pretty type name.
    /// </summary>
    protected virtual string GetNodeDump() => GetType().PrettyName();

    /// <inheritdoc />
    public override string ToString() => base.ToString() + " ObjectId=" + ObjectId;

    /// <inheritdoc />
    public override string DebuggerDump() => base.DebuggerDump() + " ObjectId=" + ObjectId;

    protected override string Dump(bool isRecursion)
    {
        var result = NodeDump;
        if(!isRecursion)
            result += DumpData().Surround("{", "}");
        return result;
    }

    /// <summary>
    ///     Like <see cref="GetNodeDump" />, but returns the empty string when no debugger is attached
    ///     so that production code can call it without paying for the formatting work.
    /// </summary>
    [PublicAPI]
    public string NodeDumpForDebug() => Debugger.IsAttached? GetNodeDump() : "";

    /// <summary>
    ///     Breaks into the debugger when this instance's <see cref="ObjectId" /> matches any of the
    ///     given ids. Useful for stopping at a specific occurrence of an object identified in an
    ///     earlier dump.
    /// </summary>
    /// <param name="objectIds"> Ids that should trigger a break. </param>
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
            Tracer.UnconditionalBreak
                ("", () => "ObjectId==" + ObjectId + "\n" + Dump(), stackFrameDepth + 1);
        IsStopByObjectIdActive = isStopByObjectIdActive;
    }
}

/// <summary>
///     Marks the property whose value is appended to the node title when the decorated type is
///     rendered as a tree node.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class AdditionalNodeInfoAttribute : Attribute
{
    /// <summary>Name of the property providing the additional info.</summary>
    public string Property
    {
        [PublicAPI]
        get;
    }

    /// <param name="property"> Name of the property providing the additional info. </param>
    public AdditionalNodeInfoAttribute(string property) => Property = property;
}