#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.Debug
{
    /// <summary>
    ///     Summary description for Dumpable.
    /// </summary>
    [Dump("Dump")]
    [DebuggerDisplay("{DebuggerDumpString}")]
    public class Dumpable
    {
        static readonly Stack<MethodDumpTraceItem> _methodDumpTraceSwitches = new Stack<MethodDumpTraceItem>();

        /// <summary>
        ///     generate dump string to be shown in debug windows
        /// </summary>
        /// <returns> </returns>
        public virtual string DebuggerDump() { return Tracer.Dump(this); }

        /// <summary>
        ///     dump string to be shown in debug windows
        /// </summary>
        [DisableDump]
        [UsedImplicitly]
        public string DebuggerDumpString { get { return DebuggerDump().Replace("\n", "\r\n"); } }

        /// <summary>
        ///     Method dump with break,
        /// </summary>
        /// <param name="p"> </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static void NotImplementedFunction(params object[] p)
        {
            var os = Tracer.DumpMethodWithData("not implemented", 1, null, p);
            Tracer.Line(os);
            Tracer.TraceBreak();
        }

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
                var os = Tracer.DumpMethodWithData("", 1, this, p);
                Tracer.Line(os);
                Tracer.IndentStart();
            }
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
                var os = Tracer.DumpData("", 1, new[] {name, value});
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
                Tracer.Line(Tracer.MethodHeader(1) + "[returns] " + Tracer.Dump(rv));
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
                Tracer.Line(Tracer.MethodHeader(1) + "[returns]");
                if(breakExecution)
                    Tracer.TraceBreak();
            }
        }

        [DebuggerHidden]
        protected void BreakExecution()
        {
            if(IsMethodDumpTraceActive)
                Tracer.TraceBreak();
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
            _methodDumpTraceSwitches.Pop();
        }

        static void CheckDumpLevel(int depth)
        {
            if(!Debugger.IsAttached)
                return;
            var top = _methodDumpTraceSwitches.Peek();
            Tracer.Assert(top.FrameCount == Tracer.CurrentFrameCount(depth + 1));
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
            var os = Tracer.DumpMethodWithData(text, 1, this, p);
            Tracer.Line(os);
            Tracer.TraceBreak();
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
            var os = Tracer.DumpData(text, 1, p);
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

            var os = Tracer.DumpMethodWithData("not implemented", 1, this, p);
            Tracer.Line(os);
            Tracer.TraceBreak();
        }

        public string Dump()
        {
            var oldIsInDump = _isInDump;
            _isInDump = true;
            try
            {
                return Dump(oldIsInDump);
            }
            finally
            {
                _isInDump = oldIsInDump;
            }
        }

        protected virtual string Dump(bool isRecursion)
        {
            var surround = "<recursion>";
            if(!isRecursion)
                surround = DumpData().Surround("{", "}");
            return GetType().PrettyName() + surround;
        }

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
        public bool IsInDump { get { return _isInDump; } }

        bool _isInDump;
        public static bool? IsMethodDumpTraceInhibited;

        /// <summary>
        ///     Default dump of data
        /// </summary>
        /// <returns> </returns>
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

        public void Dispose() { }

        static void StartMethodDump(int depth, bool trace)
        {
            if(!Debugger.IsAttached)
                return;
            var frameCount = Tracer.CurrentFrameCount(depth + 1);
            _methodDumpTraceSwitches.Push(new MethodDumpTraceItem(frameCount, trace));
        }

        sealed class MethodDumpTraceItem
        {
            readonly int _frameCount;
            readonly bool _trace;

            public MethodDumpTraceItem(int frameCount, bool trace)
            {
                _frameCount = frameCount;
                _trace = trace;
            }

            internal int FrameCount { get { return _frameCount; } }
            internal bool Trace { get { return _trace; } }
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
                return _methodDumpTraceSwitches.Peek().Trace;
            }
        }
    }
}