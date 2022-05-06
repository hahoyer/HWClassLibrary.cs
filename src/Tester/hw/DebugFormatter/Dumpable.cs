using System;
using System.Collections.Generic;
using System.Diagnostics;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

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

    [PublicAPI]
    public static bool? IsMethodDumpTraceInhibited;

    static readonly Stack<MethodDumpTraceItem> MethodDumpTraceSwitches = new();

    /// <summary>
    ///     Gets a value indicating whether this instance is in dump.
    /// </summary>
    /// <value>
    ///     <c>true</c>
    ///     if this instance is in dump; otherwise,
    ///     <c>false</c>
    ///     .
    /// </value>
    /// created 23.09.2006 17:39
    [DisableDump]
    [PublicAPI]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsInDump { get; set; }

    /// <summary>
    ///     generate dump string to be shown in debug windows
    /// </summary>
    /// <returns> </returns>
    [PublicAPI]
    public virtual string DebuggerDump() => Tracer.Dump(this);

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
    ///     dump string to be shown in debug windows
    /// </summary>
    [DisableDump]
    [PublicAPI]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string DebuggerDumpString => DebuggerDump().Replace("\n", "\r\n");

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
    ///     Method dump with break,
    /// </summary>
    /// <param name="p"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [IsLoggingFunction]
    [PublicAPI]
    public static void NotImplementedFunction(params object[] p)
    {
        var os = Tracer.DumpMethodWithData("not implemented", null, p, 1);
        os.Log();
        Tracer.TraceBreak();
    }

    /// <summary>
    ///     Method start dump,
    /// </summary>
    /// <param name="name"> </param>
    /// <param name="value"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [IsLoggingFunction]
    [PublicAPI]
    public static void Dump(string name, object value)
    {
        if(IsMethodDumpTraceActive)
        {
            var os = Tracer.DumpData("", new[] { name, value }, 1);
            os.Log();
        }
    }

    /// <summary>
    ///     Method start dump,
    /// </summary>
    /// <param name="name"> </param>
    /// <param name="getValue"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [IsLoggingFunction]
    [PublicAPI]
    public static void Dump(string name, Func<object> getValue)
    {
        if(IsMethodDumpTraceActive)
        {
            var os = Tracer.DumpData("", new[] { name, getValue() }, 1);
            os.Log();
        }
    }

    // ReSharper disable once InconsistentNaming
    [PublicAPI]
    [IsLoggingFunction]
    public void t() => DebuggerDumpString.Log();

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

    [PublicAPI]
    public void Dispose() { }

    /// <summary>
    ///     Method dump,
    /// </summary>
    /// <param name="rv"> </param>
    /// <param name="breakExecution"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [IsLoggingFunction]
    protected static T ReturnMethodDump<T>(T rv, bool breakExecution = true)
    {
        if(IsMethodDumpTraceActive)
        {
            Tracer.IndentEnd();
            (Tracer.MethodHeader(stackFrameDepth: 1) + "[returns] " + Tracer.Dump(rv)).Log();
            if(breakExecution)
                Tracer.TraceBreak();
        }

        return rv;
    }

    /// <summary>
    ///     Method dump,
    /// </summary>
    [DebuggerHidden]
    [PublicAPI]
    [IsLoggingFunction]
    protected static void ReturnVoidMethodDump(bool breakExecution = true)
    {
        if(IsMethodDumpTraceActive)
        {
            Tracer.IndentEnd();
            (Tracer.MethodHeader(stackFrameDepth: 1) + "[returns]").Log();
            if(breakExecution)
                Tracer.TraceBreak();
        }
    }

    /// <summary>
    ///     Method dump,
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
    ///     Method dump with break,
    /// </summary>
    /// <param name="text"> </param>
    /// <param name="p"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [PublicAPI]
    [IsLoggingFunction]
    protected static void DumpDataWithBreak(string text, params object[] p)
    {
        var os = Tracer.DumpData(text, p, 1);
        os.Log();
        Tracer.TraceBreak();
    }

    /// <summary>
    ///     Method start dump,
    /// </summary>
    /// <param name="trace"> </param>
    /// <param name="p"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [IsLoggingFunction]
    protected void StartMethodDump(bool trace, params object[] p)
    {
        StartMethodDump(1, trace);
        if(!IsMethodDumpTraceActive)
            return;
        var os = Tracer.DumpMethodWithData("", this, p, 1);
        os.Log();
        Tracer.IndentStart();
    }

    [DebuggerHidden]
    [PublicAPI]
    protected void BreakExecution()
    {
        if(IsMethodDumpTraceActive)
            Tracer.TraceBreak();
    }

    /// <summary>
    ///     Method dump with break,
    /// </summary>
    /// <param name="text"> </param>
    /// <param name="p"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [PublicAPI]
    [IsLoggingFunction]
    protected void DumpMethodWithBreak(string text, params object[] p)
    {
        var os = Tracer.DumpMethodWithData(text, this, p, 1);
        os.Log();
        Tracer.TraceBreak();
    }

    /// <summary>
    ///     Method dump with break,
    /// </summary>
    /// <param name="p"> </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [IsLoggingFunction]
    protected void NotImplementedMethod(params object[] p)
    {
        if(IsInDump)
            throw new NotImplementedException();

        var os = Tracer.DumpMethodWithData("not implemented", this, p, 1);
        os.Log();
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

    public static TValue[] T<TValue>(params TValue[] value) => value;
}