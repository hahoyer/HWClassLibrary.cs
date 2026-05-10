using System.Diagnostics;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Base class that gives derived types a structured debug representation and a small toolkit of
///     method-tracing helpers. The reflection-based dump used by the debugger display can be tuned via
///     attributes such as <see cref="DumpAttribute" />, <see cref="EnableDumpAttribute" /> and
///     <see cref="DisableDumpAttribute" />.
/// </summary>
[Dump("Dump")]
[DebuggerDisplay("{" + nameof(DebuggerDumpString) + "}")]
public class Dumpable
{
    sealed class MethodDumpTraceItem
    {
        internal int FrameCount { get; }
        internal bool Trace { get; }

        public MethodDumpTraceItem(int frameCount, bool trace)
        {
            FrameCount = frameCount;
            Trace = trace;
        }
    }

    /// <summary>
    ///     Global override for method-dump tracing. When <c>null</c> (default), tracing follows the
    ///     per-call switches pushed by <c>StartMethodDump</c>. Set to <c>true</c> to suppress all method
    ///     dump output, or <c>false</c> to force it.
    /// </summary>
    [PublicAPI]
    public static bool? IsMethodDumpTraceInhibited;

    static readonly Stack<MethodDumpTraceItem> MethodDumpTraceSwitches = new();

    /// <summary>
    ///     Re-entry guard: <c>true</c> while the dump of this instance is in progress. Used to break
    ///     cycles in object graphs.
    /// </summary>
    [DisableDump]
    [PublicAPI]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsInDump { get; set; }

    /// <summary>
    ///     Override to customise the string shown in debugger windows. The default uses
    ///     <see cref="Tracer.Dump(object?)" />.
    /// </summary>
    [PublicAPI]
    public virtual string DebuggerDump() => Tracer.Dump(this);

    /// <summary>
    ///     Renders the data portion (members) of this instance. Falls back to
    ///     <c>"&lt;not implemented&gt;"</c> when the dump throws.
    /// </summary>
    [PublicAPI]
    public virtual string DumpData()
    {
        string result;
        try
        {
            result = Tracer.DumpData(this);
        }
        catch(Exception)
        {
            result = "<not implemented>";
        }

        return result;
    }

    protected virtual string Dump(bool isRecursion)
    {
        var surround = "<recursion>";
        if(!isRecursion)
            surround = DumpData().Surround("{", "}");
        return GetType().PrettyName() + surround;
    }

    /// <summary>
    ///     CRLF-normalised version of <see cref="DebuggerDump" />, bound to
    ///     <see cref="DebuggerDisplayAttribute" />.
    /// </summary>
    [DisableDump]
    [PublicAPI]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string DebuggerDumpString => DebuggerDump().Replace("\n", "\r\n");

    /// <summary>
    ///     One-letter alias for <see cref="DebuggerDumpString" />, intended to be typed quickly in the
    ///     debugger watch window.
    /// </summary>
    [DisableDump]
    [PublicAPI]
    // ReSharper disable once InconsistentNaming
    public string d => DebuggerDumpString;

    static bool IsMethodDumpTraceActive
    {
        get
        {
            if(IsMethodDumpTraceInhibited == null)
                return Debugger.IsAttached && MethodDumpTraceSwitches.Peek().Trace;
            return !IsMethodDumpTraceInhibited.Value;
        }
    }

    /// <summary>
    ///     Marker for static methods that have not been implemented yet. Logs the call site and the
    ///     given parameters, then breaks into the debugger.
    /// </summary>
    /// <param name="p"> Parameter values to include in the log. </param>
    [DebuggerHidden]
    [IsLoggingFunction]
    [PublicAPI]
    public static void NotImplementedFunction(params object?[] p)
    {
        var os = Tracer.DumpMethodWithData("not implemented", thisObject: null, p, 1);
        os.Log(LogLevel.Debug);
        Tracer.TraceBreak();
    }

    /// <summary>
    ///     Logs a name/value pair from inside an instrumented method. No-op when method-dump tracing
    ///     is inactive.
    /// </summary>
    /// <param name="name"> Label shown in the log. </param>
    /// <param name="value"> Value to dump. </param>
    [DebuggerHidden]
    [IsLoggingFunction]
    [PublicAPI]
    public static void Dump(string name, object? value)
    {
        if(IsMethodDumpTraceActive)
        {
            var os = Tracer.DumpData("", [name, value], 1);
            os.Log(LogLevel.Debug);
        }
    }

    /// <summary>
    ///     Like <see cref="Dump(string, object?)" />, but the value is computed lazily and only when
    ///     tracing is active.
    /// </summary>
    /// <param name="name"> Label shown in the log. </param>
    /// <param name="getValue"> Producer for the value to dump. </param>
    [DebuggerHidden]
    [IsLoggingFunction]
    [PublicAPI]
    public static void Dump(string name, Func<object> getValue)
    {
        if(IsMethodDumpTraceActive)
        {
            var os = Tracer.DumpData("", [name, getValue()], 1);
            os.Log(LogLevel.Debug);
        }
    }

    /// <summary>One-letter alias for logging this instance, intended for use from the debugger.</summary>
    // ReSharper disable once InconsistentNaming
    [PublicAPI]
    [IsLoggingFunction]
    public void t() => DebuggerDumpString.Log(LogLevel.Debug);

    /// <summary>
    ///     Re-entrancy-safe dump entry point: sets <see cref="IsInDump" /> for the duration of the
    ///     call so that recursive references render as <c>&lt;recursion&gt;</c>.
    /// </summary>
    public string Dump()
    {
        var oldIsInDump = IsInDump;
        IsInDump = true;
        try
        {
            return Dump(oldIsInDump);
        }
        finally
        {
            IsInDump = oldIsInDump;
        }
    }

    /// <summary>Empty placeholder so instances can be used inside <c>using</c> blocks.</summary>
    [PublicAPI]
    public void Dispose() { }

    /// <summary>
    ///     Closing counterpart to <c>StartMethodDump</c> for value-returning methods. Logs the return
    ///     value and optionally breaks into the debugger.
    /// </summary>
    /// <param name="rv"> Return value to log and pass through. </param>
    /// <param name="breakExecution"> If <c>true</c>, breaks into the debugger after logging. </param>
    [DebuggerHidden]
    [IsLoggingFunction]
    protected static T ReturnMethodDump<T>(T rv, bool breakExecution = true)
    {
        if(IsMethodDumpTraceActive)
        {
            Tracer.IndentEnd();
            (Tracer.MethodHeader(stackFrameDepth: 1) + "[returns] " + Tracer.Dump(rv)).Log(LogLevel.Debug);
            if(breakExecution)
                Tracer.TraceBreak();
        }

        return rv;
    }

    /// <summary>
    ///     Closing counterpart to <c>StartMethodDump</c> for void methods.
    /// </summary>
    /// <param name="breakExecution"> If <c>true</c>, breaks into the debugger after logging. </param>
    [DebuggerHidden]
    [PublicAPI]
    [IsLoggingFunction]
    protected static void ReturnVoidMethodDump(bool breakExecution = true)
    {
        if(IsMethodDumpTraceActive)
        {
            Tracer.IndentEnd();
            (Tracer.MethodHeader(stackFrameDepth: 1) + "[returns]").Log(LogLevel.Debug);
            if(breakExecution)
                Tracer.TraceBreak();
        }
    }

    /// <summary>
    ///     Pops the trace switch pushed by <c>StartMethodDump</c> without logging a return value, and
    ///     asserts that the call stack depth matches the matching start.
    /// </summary>
    [DebuggerHidden]
    protected static void EndMethodDump()
    {
        if(!Debugger.IsAttached)
            return;

        CheckDumpLevel(1);
        MethodDumpTraceSwitches.Pop();
    }

    /// <summary>
    ///     Logs <paramref name="text" /> and the supplied data, then breaks into the debugger.
    /// </summary>
    /// <param name="text"> Free-form prefix shown before the data. </param>
    /// <param name="p"> Data values to dump. </param>
    [DebuggerHidden]
    [PublicAPI]
    [IsLoggingFunction]
    protected static void DumpDataWithBreak(string text, params object?[] p)
    {
        var os = Tracer.DumpData(text, p, 1);
        os.Log(LogLevel.Debug);
        Tracer.TraceBreak();
    }

    /// <summary>
    ///     Opens a method-dump scope. Pair every call with <see cref="EndMethodDump" /> or one of the
    ///     <c>Return…MethodDump</c> helpers. When <paramref name="trace" /> is <c>true</c> the call,
    ///     <paramref name="p" /> and subsequent intra-method <c>Dump</c> calls are written to the log.
    /// </summary>
    /// <param name="trace"> Whether to actually emit log output for this scope. </param>
    /// <param name="p"> Argument values to record on entry. </param>
    [DebuggerHidden]
    [IsLoggingFunction]
    protected void StartMethodDump(bool trace, params object?[] p)
    {
        StartMethodDump(1, trace);
        if(!IsMethodDumpTraceActive)
            return;
        var os = Tracer.DumpMethodWithData("", this, p, 1);
        os.Log(LogLevel.Debug);
        Tracer.IndentStart();
    }

    /// <summary>Breaks into the debugger only if method-dump tracing is active for the current scope.</summary>
    [DebuggerHidden]
    [PublicAPI]
    protected void BreakExecution()
    {
        if(IsMethodDumpTraceActive)
            Tracer.TraceBreak();
    }

    /// <summary>
    ///     Logs the current method together with <paramref name="text" /> and the supplied parameters,
    ///     then breaks into the debugger.
    /// </summary>
    /// <param name="text"> Free-form prefix shown before the parameter list. </param>
    /// <param name="p"> Argument values to dump. </param>
    [DebuggerHidden]
    [PublicAPI]
    [IsLoggingFunction]
    protected void DumpMethodWithBreak(string text, params object?[] p)
    {
        var os = Tracer.DumpMethodWithData(text, this, p, 1);
        os.Log(LogLevel.Debug);
        Tracer.TraceBreak();
    }

    /// <summary>
    ///     Marker for instance methods that have not been implemented yet. Logs the call site and
    ///     parameters, then breaks into the debugger. Throws <see cref="NotImplementedException" />
    ///     when invoked while a dump of <c>this</c> is already in progress, to avoid re-entry.
    /// </summary>
    /// <param name="p"> Argument values to log. </param>
    [DebuggerHidden]
    [IsLoggingFunction]
    protected void NotImplementedMethod(params object?[] p)
    {
        if(IsInDump)
            throw new NotImplementedException();

        var os = Tracer.DumpMethodWithData("not implemented", this, p, 1);
        os.Log(LogLevel.Debug);
        Tracer.TraceBreak();
    }

    static void CheckDumpLevel(int depth)
    {
        if(!Debugger.IsAttached)
            return;
        var top = MethodDumpTraceSwitches.Peek();
        if(top.Trace)
            (top.FrameCount == Tracer.CurrentFrameCount(depth + 1)).Assert();
    }

    static void StartMethodDump(int depth, bool trace)
    {
        if(!Debugger.IsAttached)
            return;
        var frameCount = trace? Tracer.CurrentFrameCount(depth + 1) : 0;
        MethodDumpTraceSwitches.Push(new(frameCount, trace));
    }

    /// <summary>
    ///     Convenience for building a typed array from a parameter list, useful when calling overloads
    ///     that expect <c>params object?[]</c>.
    /// </summary>
    public static TValue?[] T<TValue>(params TValue?[] value) => value;
}