#region Copyright (C) 2013

//     Project HWClassLibrary
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
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;

namespace HWClassLibrary.Debug
{
    /// <summary>
    ///     Summary description for Tracer.
    /// </summary>
    public static class Tracer
    {
        static int _indentCount;
        static bool _isLineStart = true;
        static BindingFlags AnyBinding { get { return BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic; } }

        [UsedImplicitly]
        public static bool IsBreakDisabled;

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="sf"> the stack frame where the location is stored </param>
        /// <param name="tag"> </param>
        /// <returns> the "FileName(LineNr,ColNr): tag: " string </returns>
        public static string FilePosn(this StackFrame sf, FilePositionTag tag)
        {
            if(sf.GetFileLineNumber() == 0)
                return "<nofile> " + tag;
            return FilePosn(sf.GetFileName(), sf.GetFileLineNumber() - 1, sf.GetFileColumnNumber(), tag);
        }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="fileName"> asis </param>
        /// <param name="lineNr"> asis </param>
        /// <param name="colNr"> asis </param>
        /// <param name="tag"> asis </param>
        /// <returns> the "fileName(lineNr,colNr): tag: " string </returns>
        public static string FilePosn(string fileName, int lineNr, int colNr, FilePositionTag tag)
        {
            var tagText = tag.ToString();
            return FilePosn(fileName, lineNr, colNr, tagText);
        }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="fileName"> asis </param>
        /// <param name="lineNr"> asis </param>
        /// <param name="colNr"> asis </param>
        /// <param name="tagText"> asis </param>
        /// <returns> the "fileName(lineNr,colNr): tag: " string </returns>
        public static string FilePosn(string fileName, int lineNr, int colNr, string tagText)
        {
            return fileName
                   + "("
                   + (lineNr + 1)
                   + ","
                   + colNr
                   + "): "
                   + tagText
                   + ": ";
        }

        /// <summary>
        ///     creates a string to inspect a method
        /// </summary>
        /// <param name="m"> the method </param>
        /// <param name="showParam"> controls if parameter list is appended </param>
        /// <returns> string to inspect a method </returns>
        public static string DumpMethod(this MethodBase m, bool showParam)
        {
            var result = m.DeclaringType.PrettyName() + ".";
            result += m.Name;
            if(!showParam)
                return result;
            if(m.IsGenericMethod)
            {
                result += "<";
                result += m
                    .GetGenericArguments()
                    .Select(t => t.PrettyName())
                    .Stringify(", ");
                result += ">";
            }
            result += "(";
            for(int i = 0, n = m.GetParameters().Length; i < n; i++)
            {
                if(i != 0)
                    result += ", ";
                var p = m.GetParameters()[i];
                result += p.ParameterType.PrettyName();
                result += " ";
                result += p.Name;
            }
            result += ")";
            return result;
        }

        /// <summary>
        ///     creates a string to inspect the method call contained in current call stack
        /// </summary>
        /// <param name="depth"> the index of stack frame </param>
        /// <param name="tag"> </param>
        /// <param name="showParam"> controls if parameter list is appended </param>
        /// <returns> string to inspect the method call </returns>
        public static string MethodHeader(int depth, FilePositionTag tag = FilePositionTag.Debug, bool showParam = false)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return FilePosn(sf, tag) + DumpMethod(sf.GetMethod(), showParam);
        }

        public static string CallingMethodName(int depth)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return DumpMethod(sf.GetMethod(), false);
        }

        [UsedImplicitly]
        public static string StackTrace(FilePositionTag tag) { return StackTrace(1, tag); }

        [UsedImplicitly]
        public static string StackTrace(int depth, FilePositionTag tag)
        {
            var stackTrace = new StackTrace(true);
            var result = "";
            for(var i = depth + 1; i < stackTrace.FrameCount; i++)
            {
                var stackFrame = stackTrace.GetFrame(i);
                var filePosn = FilePosn(stackFrame, tag) + DumpMethod(stackFrame.GetMethod(), false);
                result += "\n" + filePosn;
            }
            return result;
        }

        sealed class WriteInitiator
        {
            string _name = "";
            string _lastName = "";

            public bool ThreadChanged { get { return _name != _lastName; } }

            public string ThreadFlagString { get { return "[" + _lastName + "->" + _name + "]\n"; } }

            public void NewThread()
            {
                _lastName = _name;
                _name = Thread.CurrentThread.ManagedThreadId.ToString();
            }
        }

        static readonly WriteInitiator _writeInitiator = new WriteInitiator();

        /// <summary>
        ///     write a line to debug output
        /// </summary>
        /// <param name="s"> the text </param>
        public static void Line(string s) { ThreadSafeWrite(s, true); }

        /// <summary>
        ///     write a line to debug output
        /// </summary>
        /// <param name="s"> the text </param>
        public static void LinePart(string s) { ThreadSafeWrite(s, false); }

        static void ThreadSafeWrite(string s, bool isLine)
        {
            lock(_writeInitiator)
            {
                _writeInitiator.NewThread();

                s = IndentLine(_isLineStart, s, _indentCount);

                if(_writeInitiator.ThreadChanged && Debugger.IsAttached)
                {
                    var threadFlagString = _writeInitiator.ThreadFlagString;
                    if(!_isLineStart)
                        if(s.Length > 0 && s[0] == '\n')
                            threadFlagString = "\n" + threadFlagString;
                        else
                            throw new NotImplementedException();
                    System.Diagnostics.Debug.Write(threadFlagString);
                }

                Write(s, isLine);

                _isLineStart = isLine;
            }
        }

        static void Write(string s, bool isLine)
        {
            if(Debugger.IsAttached)
                if(isLine)
                    System.Diagnostics.Debug.WriteLine(s);
                else
                    System.Diagnostics.Debug.Write(s);
            else if(isLine)
                Console.WriteLine(s);
            else
                Console.Write(s);
        }

        /// <summary>
        ///     write a line to debug output, flagged with FileName(LineNr,ColNr): Method
        /// </summary>
        /// <param name="s"> the text </param>
        /// <param name="showParam"> controls if parameter list is appended </param>
        /// <param name="flagText"> </param>
        public static void FlaggedLine(string s, bool showParam, FilePositionTag flagText = FilePositionTag.Debug) { Line(MethodHeader(1, flagText, showParam) + s); }

        /// <summary>
        ///     write a line to debug output, flagged with FileName(LineNr,ColNr): Method (without parameter list)
        /// </summary>
        /// <param name="s"> the text </param>
        /// <param name="flagText"> </param>
        public static void FlaggedLine(string s, FilePositionTag flagText = FilePositionTag.Debug) { Line(MethodHeader(1, flagText) + s); }

        /// <summary>
        ///     write a line to debug output, flagged with FileName(LineNr,ColNr): Method (without parameter list)
        /// </summary>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <param name="s"> the text </param>
        /// <param name="flagText"> </param>
        public static void FlaggedLine(int stackFrameDepth, string s, FilePositionTag flagText = FilePositionTag.Debug) { Line(MethodHeader(stackFrameDepth + 1, flagText) + s); }

        /// <summary>
        ///     generic dump function by use of reflection
        /// </summary>
        /// <param name="x"> the object to dump </param>
        /// <returns> </returns>
        public static string Dump(object x)
        {
            if(x == null)
                return "null";
            return InternalDump(true, x.GetType(), x);
        }

        /// <summary>
        ///     generic dump function by use of reflection
        /// </summary>
        /// <param name="x"> the object to dump </param>
        /// <returns> </returns>
        public static string DumpData(object x)
        {
            if(x == null)
                return "";
            return InternalDumpData(x.GetType(), x);
        }

        static string InternalDump(bool isTop, Type t, object x)
        {
            var xl = x as IList;
            if(xl != null)
                return InternalDump(xl);

            var xd = x as IDictionary;
            if(xd != null)
                return InternalDump(xd);

            var xc = x as ICollection;
            if(xc != null)
                return InternalDump(xc);

            var co = x as CodeObject;
            if(co != null)
                return InternalDump(co);
            var xt = x as Type;
            if(xt != null)
                return xt.PrettyName();

            if(t.IsPrimitive || t.ToString().StartsWith("System."))
                return x.ToString();

            if(t.ToString() == "Outlook.ApplicationClass")
                return x.ToString();
            if(t.ToString() == "Outlook.NameSpaceClass")
                return x.ToString();
            if(t.ToString() == "Outlook.InspectorClass")
                return x.ToString();

            var dea = DumpClassAttribute(t);
            if(dea != null)
                return dea.Dump(isTop, t, x);

            var result = BaseDump(t, x) + InternalDumpData(t, x);
            if(result != "")
                result = Surround("(", result, ")");

            if(isTop || result != "")
                result = t + result;

            return result;
        }

        static string InternalDump(ICollection xc)
        {
            return "Count="
                   + xc.Count
                   + xc
                         .Cast<object>()
                         .Select(Dump)
                         .Stringify("\n", true)
                         .Surround("{", "}");
        }

        static string InternalDump(this CodeObject co)
        {
            var cse = co as CodeSnippetExpression;
            if(cse != null)
                return cse.Value;

            throw new NotImplementedException();
        }

        static string InternalDump(this IList xl)
        {
            return "Count="
                   + xl.Count
                   + xl
                         .Cast<object>()
                         .Select(Dump)
                         .Stringify("\n", true)
                         .Surround("{", "}");
        }

        static string InternalDump(this IDictionary xd)
        {
            var keys = xd.Keys.Cast<object>();
            var dictionaryEntries = keys.Select(key=>new {Key=key, Value=xd[key]}).ToArray();
            return dictionaryEntries
                .Select(entry => "[" + Dump(entry.Key) + "] " + Dump(entry.Value))
                .Stringify("\n")
                .Surround("{", "}");
        }

        static DumpClassAttribute DumpClassAttribute(this Type t)
        {
            var result = DumpClassAttributeClass(t);
            if(result != null)
                return result;
            var results = DumpClassAttributeInterfaces(t);
            if(results.Length == 0)
                return null;
            if(results.Length == 1)
                return results[0];
            throw new NotImplementedException();
        }

        static DumpClassAttribute[] DumpClassAttributeInterfaces(this Type t)
        {
            var al = new ArrayList();
            foreach(var i in t.GetInterfaces())
                al.AddRange(DumpClassAttributeInterfaces(i));
            if(al.Count == 0)
                return new DumpClassAttribute[0];

            return (DumpClassAttribute[]) al.ToArray();
        }

        static DumpClassAttribute DumpClassAttributeClass(this Type t)
        {
            var result = t.GetRecentAttribute<DumpClassAttribute>();
            if(result != null)
                return result;
            if(t.BaseType != null)
                return DumpClassAttribute(t.BaseType);
            return null;
        }

        static string InternalDumpData(this Type t, object x)
        {
            var dumpData = t.GetAttribute<DumpDataClassAttribute>(false);
            if(dumpData != null)
                return dumpData.Dump(t, x);
            var f = t.GetFields(AnyBinding);
            var fieldDump = "";
            if(f.Length > 0)
                fieldDump = DumpMembers(f, x);
            MemberInfo[] p = t.GetProperties(AnyBinding);
            var propertyDump = "";
            if(p.Length > 0)
                propertyDump = DumpMembers(p, x);
            if(fieldDump == "")
                return propertyDump;
            if(propertyDump == "")
                return fieldDump;
            return fieldDump + "\n" + propertyDump;
        }

        static List<int> CheckMemberAttributes(MemberInfo[] f, object x)
        {
            var l = new List<int>();
            for(var i = 0; i < f.Length; i++)
            {
                var pi = f[i] as PropertyInfo;
                if(pi != null && pi.GetIndexParameters().Length > 0)
                    continue;
                if(!CheckDumpDataAttribute(f[i]))
                    continue;
                if(!CheckDumpExceptAttribute(f[i], x))
                    continue;
                l.Add(i);
            }
            return l;
        }

        static bool CheckDumpDataAttribute(this MemberInfo m)
        {
            var dda = m.GetAttribute<DumpEnabledAttribute>(true);
            if(dda != null)
                return dda.IsEnabled;

            return !IsPrivateOrDump(m);
        }

        static bool IsPrivateOrDump(this MemberInfo m)
        {
            if(m.Name.Contains("Dump") || m.Name.Contains("dump"))
                return true;

            var fieldInfo = m as FieldInfo;
            if(fieldInfo != null)
                return fieldInfo.IsPrivate;

            if(((PropertyInfo) m).CanRead)
                return ((PropertyInfo) m).GetGetMethod(true).IsPrivate;
            return true;
        }

        static string DumpMembers(MemberInfo[] f, object x) { return DumpSomeMembers(CheckMemberAttributes(f, x), f, x); }

        static string DumpSomeMembers(IList<int> l, MemberInfo[] f, object x)
        {
            var result = "";
            for(var i = 0; i < l.Count; i++)
            {
                if(i > 0)
                    result += "\n";
                if(l.Count > 10)
                    result += i + ":";
                result += f[l[i]].Name;
                result += "=";
                try
                {
                    result += Dump(Value(f[l[i]], x));
                }
                catch(Exception)
                {
                    result += "<not implemented>";
                }
            }
            return result;
        }

        static bool CheckDumpExceptAttribute(this MemberInfo f, object x)
        {
            foreach(var dea in Attribute.GetCustomAttributes(f, typeof(DumpAttributeBase)).Select(ax => ax as IDumpExceptAttribute).Where(ax => ax != null))
            {
                var v = Value(f, x);
                return !dea.IsException(v);
            }
            return true;
        }

        static string BaseDump(this Type t, object x)
        {
            var baseDump = "";
            if(t.BaseType != null && t.BaseType.ToString() != "System.Object" &&
               t.BaseType.ToString() != "System.ValueType")
                baseDump = InternalDump(false, t.BaseType, x);
            if(baseDump != "")
                baseDump = "\nBase:" + baseDump;
            return baseDump;
        }

        static object Value(this MemberInfo info, object x)
        {
            var fi = info as FieldInfo;
            if(fi != null)
                return fi.GetValue(x);
            var pi = info as PropertyInfo;
            if(pi != null)
                return pi.GetValue(x, null);

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Indent
        /// </summary>
        public static void IndentStart() { _indentCount++; }

        /// <summary>
        ///     Unindent
        /// </summary>
        public static void IndentEnd() { _indentCount--; }

        static string IndentElem(int count)
        {
            var result = "";
            for(var i = 0; i < count; i++)
                result += "    ";
            return result;
        }

        static string IndentLine(bool isLineStart, string s, int count)
        {
            var indentElem = IndentElem(count);
            return (isLineStart ? indentElem : "") + s.Replace("\n", "\n" + indentElem);
        }

        /// <summary>
        ///     Indent paramer by 4 * count spaces
        /// </summary>
        /// <param name="s"> </param>
        /// <param name="count"> </param>
        /// <returns> </returns>
        public static string Indent(string s, int count = 1) { return s.Replace("\n", "\n" + IndentElem(count)); }

        /// <summary>
        ///     Surrounds string by left and right parenthesis. If string contains any carriage return, some indenting is done also
        /// </summary>
        /// <param name="left"> </param>
        /// <param name="data"> </param>
        /// <param name="right"> </param>
        /// <returns> </returns>
        public static string Surround(string left, string data, string right)
        {
            if(data.IndexOf("\n", StringComparison.Ordinal) < 0)
                return left + data + right;
            return "\n" + left + Indent("\n" + data) + "\n" + right;
        }

        /// <summary>
        ///     creates a string to inspect the method call contained in stack. Runtime parameters are dumped too.
        /// </summary>
        /// <param name="parameter"> parameter objects list for the frame </param>
        public static void DumpStaticMethodWithData(params object[] parameter)
        {
            var result = DumpMethodWithData("", 1, null, parameter);
            Line(result);
        }

        internal static string DumpMethodWithData(string text, int depth, object thisObject, object[] parameter)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return FilePosn(sf, FilePositionTag.Debug)
                   + (DumpMethod(sf.GetMethod(), true))
                   + text
                   + Indent(DumpMethodWithData(sf.GetMethod(), thisObject, parameter));
        }

        /// <summary>
        ///     Dumps the data.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <param name="depth"> The stack depth. </param>
        /// <param name="data"> The data, as name/value pair. </param>
        /// <returns> </returns>
        public static string DumpData(string text, int depth, object[] data)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return FilePosn(sf, FilePositionTag.Debug)
                   + DumpMethod(sf.GetMethod(), true)
                   + text
                   + Indent(DumpMethodWithData(null, data));
        }

        static string DumpMethodWithData(MethodBase m, object o, object[] p)
        {
            var result = "\n";
            result += "this=";
            result += Dump(o);
            result += "\n";
            result += DumpMethodWithData(m.GetParameters(), p);
            return result;
        }

        static string DumpMethodWithData(ParameterInfo[] infos, object[] p)
        {
            var result = "";
            var n = 0;
            if(infos != null)
                n = infos.Length;
            for(var i = 0; i < n; i++)
            {
                if(i > 0)
                    result += "\n";
                Assert(infos != null);
                Assert(infos[i] != null);
                result += infos[i].Name;
                result += "=";
                result += Dump(p[i]);
            }
            for(var i = n; i < p.Length; i += 2)
            {
                result += "\n";
                result += (string) p[i];
                result += "=";
                result += Dump(p[i + 1]);
            }
            return result;
        }

        /// <summary>
        ///     Function used for condition al break
        /// </summary>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <param name="cond"> The cond. </param>
        /// <param name="data"> The data. </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static void ConditionalBreak(int stackFrameDepth, string cond, Func<string> data)
        {
            var result = "Conditional break: " + cond + "\nData: " + (data == null ? "" : data());
            FlaggedLine(stackFrameDepth + 1, result);
            TraceBreak();
        }

        /// <summary>
        ///     Check boolean expression
        /// </summary>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <param name="b">
        ///     if set to <c>true</c> [b].
        /// </param>
        /// <param name="text"> The text. </param>
        [DebuggerHidden]
        public static void ConditionalBreak(int stackFrameDepth, bool b, Func<string> text)
        {
            if(b)
                ConditionalBreak(stackFrameDepth + 1, "", text);
        }

        [DebuggerHidden]
        public static void ConditionalBreak(bool cond, Func<string> data = null)
        {
            if(cond)
                ConditionalBreak(1, true, data);
        }

        /// <summary>
        ///     Throws the assertion failed.
        /// </summary>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <param name="cond"> The cond. </param>
        /// <param name="data"> The data. </param>
        /// created 15.10.2006 18:04
        [DebuggerHidden]
        public static void ThrowAssertionFailed(int stackFrameDepth, string cond, Func<string> data)
        {
            var result = AssertionFailed(stackFrameDepth + 1, cond, data);
            throw new AssertionFailedException(result);
        }

        /// <summary>
        ///     Throws the assertion failed.
        /// </summary>
        /// <param name="s"> The s. </param>
        /// <param name="s1"> The s1. </param>
        /// created 16.12.2006 18:28
        [DebuggerHidden]
        public static void ThrowAssertionFailed(string s, Func<string> s1) { ThrowAssertionFailed(1, s, s1); }

        /// <summary>
        ///     Function used in assertions
        /// </summary>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <param name="cond"> The cond. </param>
        /// <param name="data"> The data. </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static string AssertionFailed(int stackFrameDepth, string cond, Func<string> data)
        {
            var result = "Assertion Failed: " + cond + "\nData: " + data();
            FlaggedLine(stackFrameDepth + 1, result);
            AssertionBreak(result);
            return result;
        }

        /// <summary>
        ///     Check boolean expression
        /// </summary>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <param name="b">
        ///     if set to <c>true</c> [b].
        /// </param>
        /// <param name="text"> The text. </param>
        [DebuggerHidden]
        public static void Assert(int stackFrameDepth, [AssertionCondition(AssertionConditionType.IsTrue)] bool b, Func<string> text)
        {
            if(b)
                return;
            AssertionFailed(stackFrameDepth + 1, "", text);
        }

        /// <summary>
        ///     Check boolean expression
        /// </summary>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <param name="b">
        ///     if set to <c>true</c> [b].
        /// </param>
        [DebuggerHidden]
        [AssertionMethod]
        public static void Assert(int stackFrameDepth, [AssertionCondition(AssertionConditionType.IsTrue)] bool b)
        {
            if(b)
                return;
            AssertionFailed(stackFrameDepth + 1, "", () => "");
        }

        /// <summary>
        ///     Asserts the specified b.
        /// </summary>
        /// <param name="b">
        ///     if set to <c>true</c> [b].
        /// </param>
        /// created 16.12.2006 18:27
        [DebuggerHidden]
        [AssertionMethod]
        public static void Assert([AssertionCondition(AssertionConditionType.IsTrue)] bool b) { Assert(1, b); }

        /// <summary>
        ///     Asserts the specified b.
        /// </summary>
        /// <param name="b">
        ///     if set to <c>true</c> [b].
        /// </param>
        /// <param name="s"> The s. </param>
        /// created 16.12.2006 18:29
        [DebuggerHidden]
        [AssertionMethod]
        public static void Assert([AssertionCondition(AssertionConditionType.IsTrue)] bool b, Func<string> s) { Assert(1, b, s); }

        [DebuggerHidden]
        [AssertionMethod]
        public static void Assert([AssertionCondition(AssertionConditionType.IsTrue)] bool b, string s) { Assert(1, b, () => s); }

        /// <summary>
        ///     Assertions the failed.
        /// </summary>
        /// <param name="s"> The s. </param>
        /// <param name="s1"> The s1. </param>
        /// created 16.12.2006 18:30
        [DebuggerHidden]
        public static void AssertionFailed(string s, Func<string> s1) { AssertionFailed(1, s, s1); }

        /// <summary>
        ///     Outputs the specified text.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// Created 09.09.07 12:03 by hh on HAHOYER-DELL
        public static void ConsoleOutput(string text)
        {
            FlaggedLine(text, FilePositionTag.Output);
            Console.WriteLine(text);
        }

        sealed class AssertionFailedException : Exception
        {
            public AssertionFailedException(string result)
                : base(result) { }
        }

        sealed class BreakException : Exception
        {}

        [DebuggerHidden]
        static void AssertionBreak(string result)
        {
            if(!Debugger.IsAttached || IsBreakDisabled)
                throw new AssertionFailedException(result);
            Debugger.Break();
        }

        [UsedImplicitly]
        [DebuggerHidden]
        public static void TraceBreak()
        {
            if(!Debugger.IsAttached)
                return;
            if(IsBreakDisabled)
                throw new BreakException();
            Debugger.Break();
        }

        [UsedImplicitly]
        public static string CallingMethodName() { return CallingMethodName(1); }

        public static int CurrentFrameCount(int depth) { return new StackTrace(true).FrameCount - depth; }

        [DebuggerHidden]
        public static void LaunchDebugger() { Debugger.Launch(); }
    }

    public enum FilePositionTag
    {
        Debug,
        Output,
        Query,
        Test,
        Profiler
    }

    interface IDumpExceptAttribute
    {
        bool IsException(object target);
    }

    public abstract class DumpAttributeBase : Attribute
    {}
}