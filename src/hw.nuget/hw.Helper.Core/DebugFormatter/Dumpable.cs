using System;
using System.Collections.Generic;
using System.Diagnostics;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.DebugFormatter
{
    /// <summary>
    ///     Summary description for Dumpable.
    /// </summary>
    [Dump("Dump")]
    [DebuggerDisplay("{DebuggerDumpString}")]
    public class Dumpable
    {
        sealed class MethodDumpTraceItem
        {
            public MethodDumpTraceItem(int frameCount, bool trace)
            {
                FrameCount = frameCount;
                Trace = trace;
            }

            internal int FrameCount {get;}
            internal bool Trace {get;}
        }

        static readonly Stack<MethodDumpTraceItem> MethodDumpTraceSwitches = new Stack<MethodDumpTraceItem>();
        public static bool? IsMethodDumpTraceInhibited;

        /// <summary>
        ///     Method dump with break,
        /// </summary>
        /// <param name="p"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static void NotImplementedFunction(params object[] p)
        {
            var os = Tracer.DumpMethodWithData("not implemented", null, p, 1);
            Tracer.Line(os);
            Tracer.TraceBreak();
        }

        /// <summary>
        ///     Method start dump,
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="value"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static void Dump(string name, object value)
        {
            if(IsMethodDumpTraceActive)
            {
                var os = Tracer.DumpData("", new[] {name, value}, 1);
                Tracer.Line(os);
            }
        }

        /// <summary>
        ///     Method start dump,
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="getValue"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static void Dump(string name, Func<object> getValue)
        {
            if(IsMethodDumpTraceActive)
            {
                var os = Tracer.DumpData("", new[] {name, getValue()}, 1);
                Tracer.Line(os);
            }
        }

        /// <summary>
        ///     Method dump,
        /// </summary>
        /// <param name="rv"> </param>
        /// <param name="breakExecution"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        protected static T ReturnMethodDump<T>(T rv, bool breakExecution = true)
        {
            if(IsMethodDumpTraceActive)
            {
                Tracer.IndentEnd();
                Tracer.Line(Tracer.MethodHeader(stackFrameDepth: 1) + "[returns] " + Tracer.Dump(rv));
                if(breakExecution)
                    Tracer.TraceBreak();
            }

            return rv;
        }

        /// <summary>
        ///     Method dump,
        /// </summary>
        [DebuggerHidden]
        protected static void ReturnVoidMethodDump(bool breakExecution = true)
        {
            if(IsMethodDumpTraceActive)
            {
                Tracer.IndentEnd();
                Tracer.Line(Tracer.MethodHeader(stackFrameDepth: 1) + "[returns]");
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

        static void CheckDumpLevel(int depth)
        {
            if(!Debugger.IsAttached)
                return;
            var top = MethodDumpTraceSwitches.Peek();
            if(top.Trace)
                Tracer.Assert(top.FrameCount == Tracer.CurrentFrameCount(depth + 1));
        }

        /// <summary>
        ///     Method dump with break,
        /// </summary>
        /// <param name="text"> </param>
        /// <param name="p"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        protected static void DumpDataWithBreak(string text, params object[] p)
        {
            var os = Tracer.DumpData(text, p, 1);
            Tracer.Line(os);
            Tracer.TraceBreak();
        }

        static void StartMethodDump(int depth, bool trace)
        {
            if(!Debugger.IsAttached)
                return;
            var frameCount = trace ? Tracer.CurrentFrameCount(depth + 1) : 0;
            MethodDumpTraceSwitches.Push(new MethodDumpTraceItem(frameCount, trace));
        }

        static bool IsMethodDumpTraceActive
        {
            get
            {
                if(IsMethodDumpTraceInhibited != null)
                    return !IsMethodDumpTraceInhibited.Value;
                if(!Debugger.IsAttached)
                    return false;
                //CheckDumpLevel(2);
                return MethodDumpTraceSwitches.Peek().Trace;
            }
        }

        /// <summary>
        ///     dump string to be shown in debug windows
        /// </summary>
        [DisableDump]
        [UsedImplicitly]
        public string DebuggerDumpString => DebuggerDump().Replace("\n", "\r\n");

        [DisableDump]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public string d => DebuggerDumpString;

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
        public bool IsInDump {get; set;}

        /// <summary>
        ///     generate dump string to be shown in debug windows
        /// </summary>
        /// <returns> </returns>
        public virtual string DebuggerDump() => Tracer.Dump(this);

        // ReSharper disable once InconsistentNaming
        public void t() => Tracer.Line(DebuggerDumpString);

        /// <summary>
        ///     Method start dump,
        /// </summary>
        /// <param name="trace"> </param>
        /// <param name="p"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        protected void StartMethodDump(bool trace, params object[] p)
        {
            StartMethodDump(1, trace);
            if(IsMethodDumpTraceActive)
            {
                var os = Tracer.DumpMethodWithData("", this, p, 1);
                Tracer.Line(os);
                Tracer.IndentStart();
            }
        }

        [DebuggerHidden]
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
        protected void DumpMethodWithBreak(string text, params object[] p)
        {
            var os = Tracer.DumpMethodWithData(text, this, p, 1);
            Tracer.Line(os);
            Tracer.TraceBreak();
        }

        /// <summary>
        ///     Method dump with break,
        /// </summary>
        /// <param name="p"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        protected void NotImplementedMethod(params object[] p)
        {
            if(IsInDump)
                throw new NotImplementedException();

            var os = Tracer.DumpMethodWithData("not implemented", this, p, 1);
            Tracer.Line(os);
            Tracer.TraceBreak();
        }

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

        protected virtual string Dump(bool isRecursion)
        {
            var surround = "<recursion>";
            if(!isRecursion)
                surround = DumpData().Surround("{", "}");
            return GetType().PrettyName() + surround;
        }

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

        public void Dispose() {}
    }
}