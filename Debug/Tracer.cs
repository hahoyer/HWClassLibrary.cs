using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HWClassLibrary.Helper;
using JetBrains.Annotations;

namespace HWClassLibrary.Debug
{
    /// <summary>
    /// Summary description for Tracer.
    /// </summary>
    public class Tracer
    {
        private static int _indentCount;
        private static BindingFlags AnyBinding { get { return BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic; } }

        /// <summary>
        /// creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="sf">the stack frame where the location is stored</param>
        /// <param name="flagText">asis</param>
        /// <returns>the "FileName(LineNr,ColNr): flagText: " string</returns>
        public static string FilePosn(StackFrame sf, string flagText)
        {
            if(sf.GetFileLineNumber() == 0)
                return "<nofile> ";
            return FilePosn(sf.GetFileName(), sf.GetFileLineNumber() - 1, sf.GetFileColumnNumber(), flagText);
        }

        /// <summary>
        /// creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="FileName">asis</param>
        /// <param name="LineNr">asis</param>
        /// <param name="ColNr">asis</param>
        /// <param name="flagText">asis</param>
        /// <returns>the "FileName(LineNr,ColNr): flagText: " string</returns>
        public static string FilePosn(string FileName, int LineNr, int ColNr, string flagText)
        {
            return FileName + "(" + (LineNr + 1) + "," + ColNr + "): " + flagText + ": ";
        }

        /// <summary>
        /// creates a string to inspect a method
        /// </summary>
        /// <param name="m">the method</param>
        /// <param name="ShowParam">controls if parameter list is appended</param>
        /// <returns>string to inspect a method</returns>
        public static string DumpMethod(MethodBase m, bool ShowParam)
        {
            var result = m.DeclaringType.Name + ".";
            result += m.Name;
            if(!ShowParam)
                return result;
            result += "(";
            for(int i = 0, n = m.GetParameters().Length; i < n; i++)
            {
                if(i != 0)
                    result += ", ";
                var p = m.GetParameters()[i];
                result += p.ParameterType;
                result += " ";
                result += p.Name;
            }
            result += ")";
            return result;
        }

        /// <summary>
        /// creates a string to inspect the method call contained in current call stack
        /// </summary>
        /// <param name="depth">the index of stack frame</param>
        /// <param name="ShowParam">controls if parameter list is appended</param>
        /// <returns>string to inspect the method call</returns>
        public static string MethodHeader(int depth, bool ShowParam)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return FilePosn(sf, DumpMethod(sf.GetMethod(), ShowParam));
        }

        /// <summary>
        /// creates a string to inspect the method call contained in current call stack (without parameter list)
        /// </summary>
        /// <param name="depth">the index of stack frame</param>
        /// <returns>string to inspect the method call</returns>
        public static string MethodHeader(int depth)
        {
            return MethodHeader(depth + 1, false);
        }

        /// <summary>
        /// write a line to debug output
        /// </summary>
        /// <param name="s">the text</param>
        public static void Line(string s)
        {
            s = IndentLine(s, _indentCount);
            System.Diagnostics.Debug.WriteLine(s);
        }

        /// <summary>
        /// write a line to debug output, flagged with FileName(LineNr,ColNr): Method
        /// </summary>
        /// <param name="s">the text</param>
        /// <param name="ShowParam">controls if parameter list is appended</param>
        public static void FlaggedLine(string s, bool ShowParam)
        {
            Line(MethodHeader(1, ShowParam) + s);
        }

        /// <summary>
        /// write a line to debug output, flagged with FileName(LineNr,ColNr): Method (without parameter list)
        /// </summary>
        /// <param name="s">the text</param>
        public static void FlaggedLine(string s)
        {
            Line(MethodHeader(1, false) + s);
        }

        /// <summary>
        /// write a line to debug output, flagged with FileName(LineNr,ColNr): Method (without parameter list)
        /// </summary>
        /// <param name="stackFrameDepth">The stack frame depth.</param>
        /// <param name="s">the text</param>
        public static void FlaggedLine(int stackFrameDepth, string s)
        {
            Line(MethodHeader(stackFrameDepth + 1, false) + s);
        }

        /// <summary>
        /// generic dump function by use of reflection
        /// </summary>
        /// <param name="x">the object to dump</param>
        /// <returns></returns>
        public static string Dump(object x)
        {
            if(x == null)
                return "null";
            return InternalDump(true, x.GetType(), x);
        }

        /// <summary>
        /// generic dump function by use of reflection
        /// </summary>
        /// <param name="x">the object to dump</param>
        /// <returns></returns>
        public static string DumpData(object x)
        {
            if(x == null)
                return "";
            return InternalDumpData(x.GetType(), x);
        }

        private static string InternalDump(bool isTop, Type t, object x)
        {
            var xl = x as IList;
            if(xl != null)
                return InternalDump(xl);
            var xd = x as IDictionary;
            if(xd != null)
                return InternalDump(xd);
            var co = x as CodeObject;
            if(co != null)
                return InternalDump(co);

            if(t.IsPrimitive || t.ToString().StartsWith("System."))
                return x.ToString();

            if(t.ToString() == "Outlook.ApplicationClass")
                return x.ToString();
            if(t.ToString() == "Outlook.NameSpaceClass")
                return x.ToString();
            if(t.ToString() == "Outlook.InspectorClass")
                return x.ToString();

            var dea = DumpExcludeAttribute(t);
            if(dea != null)
                return dea.Dump(isTop, t, x);

            var Return = BaseDump(t, x) + InternalDumpData(t, x);
            if(Return != "")
                Return = Surround("(", Return, ")");

            if(isTop || Return != "")
                Return = t + Return;

            return Return;
        }

        private static string InternalDump(CodeObject co)
        {
            var cse = co as CodeSnippetExpression;
            if(cse != null)
                return cse.Value;

            throw new NotImplementedException();
        }

        private static string InternalDump(IList xl)
        {
            var result = "";
            for(var i = 0; i < xl.Count; i++)
            {
                if(i > 0)
                    result += "\n";
                result += "[" + i + "] " + Dump(xl[i]);
            }
            return "Count=" + xl.Count + HWString.Surround("{", result, "}");
        }

        private static string InternalDump(IDictionary xd)
        {
            var result = "";
            foreach(DictionaryEntry entry in xd)
            {
                if(result != "")
                    result += "\n";
                result += "[" + Dump(entry.Key) + "] " + Dump(entry.Value);
            }
            return HWString.Surround("{", result, "}");
        }

        private static DumpClassAttribute DumpExcludeAttribute(Type t)
        {
            var result = DumpExcludeAttributeClass(t);
            if(result != null)
                return result;
            var results = DumpExcludeAttributeInterfaces(t);
            if(results.Length == 0)
                return null;
            if(results.Length == 1)
                return results[0];
            throw new NotImplementedException();
        }

        private static DumpClassAttribute[] DumpExcludeAttributeInterfaces(Type t)
        {
            var al = new ArrayList();
            foreach(var i in t.GetInterfaces())
                al.AddRange(DumpExcludeAttributeInterfaces(i));
            if(al.Count == 0)
                return new DumpClassAttribute[0];

            return (DumpClassAttribute[]) al.ToArray();
        }

        private static DumpClassAttribute DumpExcludeAttributeSimple(MemberInfo t)
        {
            var a = Attribute.GetCustomAttributes(t, typeof(DumpClassAttribute));
            if(a.Length == 0)
                return null;
            return (DumpClassAttribute) a[0];
        }

        private static DumpClassAttribute DumpExcludeAttributeClass(Type t)
        {
            var result = DumpExcludeAttributeSimple(t);
            if(result != null)
                return result;
            if(t.BaseType != null)
                return DumpExcludeAttribute(t.BaseType);
            return null;
        }

        private static string InternalDumpData(Type t, object x)
        {
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

        private static List<int> CheckMemberAttributes(MemberInfo[] f, object x)
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

        private static bool CheckDumpDataAttribute(MemberInfo m)
        {
            var dda = (DumpDataAttribute) Attribute.GetCustomAttribute(m, typeof(DumpDataAttribute));
            if(dda != null)
                return dda.Dump;

            return !IsPrivate(m);
        }

        private static bool IsPrivate(MemberInfo m)
        {
            if(m is PropertyInfo)
                return ((PropertyInfo) m).GetGetMethod(true).IsPrivate;
            return ((FieldInfo) m).IsPrivate;
        }

        private static string DumpMembers(MemberInfo[] f, object x)
        {
            return DumpSomeMembers(CheckMemberAttributes(f, x), f, x);
        }

        private static string DumpSomeMembers(IList<int> l, MemberInfo[] f, object x)
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

        private static bool CheckDumpExceptAttribute(MemberInfo f, object x)
        {
            var deas = (DumpExceptAttribute[]) Attribute.GetCustomAttributes(f, typeof(DumpExceptAttribute));
            foreach(var dea in deas)
            {
                var v = Value(f, x);
                if(dea.Exception == null)
                {
                    if(v == null)
                        return false;
                    if(v.Equals(DateTime.MinValue))
                        return false;
                    return true;
                }
                if(v.Equals(dea.Exception))
                    return false;
            }
            return true;
        }

        private static string BaseDump(Type t, object x)
        {
            var BaseDump = "";
            if(t.BaseType != null && t.BaseType.ToString() != "System.Object" &&
                t.BaseType.ToString() != "System.ValueType")
                BaseDump = InternalDump(false, t.BaseType, x);
            if(BaseDump != "")
                BaseDump = "\nBase:" + BaseDump;
            return BaseDump;
        }

        private static object Value(MemberInfo info, object x)
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
        /// Indent
        /// </summary>
        public static void IndentStart()
        {
            _indentCount++;
        }

        /// <summary>
        /// Unindent
        /// </summary>
        public static void IndentEnd()
        {
            _indentCount--;
        }

        private static string IndentElem(int count)
        {
            var result = "";
            for(var i = 0; i < count; i++)
                result += "    ";
            return result;
        }

        private static string IndentLine(string s, int count)
        {
            var indentElem = IndentElem(count);
            return indentElem + s.Replace("\n", "\n" + indentElem);
        }

        /// <summary>
        /// Indent paramer by 4 spaces
        /// </summary>
        /// <param name="s"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Indent(string s, int count)
        {
            return s.Replace("\n", "\n" + IndentElem(count));
        }

        /// <summary>
        /// Indent paramer by 4 spaces
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Indent(string s)
        {
            return Indent(s, 1);
        }

        /// <summary>
        /// Surrounds string by left and right parenthesis. 
        /// If string contains any carriage return, some indenting is done also 
        /// </summary>
        /// <param name="Left"></param>
        /// <param name="data"></param>
        /// <param name="Right"></param>
        /// <returns></returns>
        public static string Surround(string Left, string data, string Right)
        {
            if(data.IndexOf("\n") < 0)
                return Left + data + Right;
            return "\n" + Left + Indent("\n" + data, 1) + "\n" + Right;
        }

        /// <summary>
        /// creates a string to inspect the method call contained in stack. Runtime parameters are dumped too.
        /// </summary>
        /// <param name="text">some text</param>
        /// <param name="depth">the index of stack frame</param>
        /// <param name="thisObject">"this" for the stack frame</param>
        /// <param name="parameter">parameter objects list for the frame</param>
        /// <returns>the dump string</returns>
        public static string DumpMethodWithData(string text, int depth, object thisObject, object[] parameter)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return FilePosn(sf, DumpMethod(sf.GetMethod(), true))
                + text
                    + Indent(DumpMethodWithData(sf.GetMethod(), thisObject, parameter), 1);
        }

        /// <summary>
        /// Dumps the data.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="depth">The stack depth.</param>
        /// <param name="data">The data, as name/value pair.</param>
        /// <returns></returns>
        public static string DumpData(string text, int depth, object[] data)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return FilePosn(sf, DumpMethod(sf.GetMethod(), true))
                + text
                    + Indent(DumpMethodWithData(null, data), 1);
        }

        private static string DumpMethodWithData(MethodBase m, object o, object[] p)
        {
            var result = "\n";
            result += "this=";
            result += Dump(o);
            result += "\n";
            result += DumpMethodWithData(m.GetParameters(), p);
            return result;
        }

        private static string DumpMethodWithData(ParameterInfo[] infos, object[] p)
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
        /// Function used for condition al break
        /// </summary>
        /// <param name="stackFrameDepth">The stack frame depth.</param>
        /// <param name="cond">The cond.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static string ConditionalBreak(int stackFrameDepth, string cond, string data)
        {
            var result = "Conditional break: " + cond + "\nData: " + data;
            FlaggedLine(stackFrameDepth + 1, result);
            Debugger.Break();
            return result;
        }

        /// <summary>
        /// Check boolean expression
        /// </summary>
        /// <param name="stackFrameDepth">The stack frame depth.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <param name="text">The text.</param>
        [DebuggerHidden]
        public static void ConditionalBreak(int stackFrameDepth, bool b, string text)
        {
            if(b)
                ConditionalBreak(stackFrameDepth + 1, "", text);
        }

        [DebuggerHidden]
        public static void ConditionalBreak(bool cond, string data)
        {
            if(cond)
                ConditionalBreak(1, cond, data);
        }

        /// <summary>
        /// Throws the assertion failed.
        /// </summary>
        /// <param name="stackFrameDepth">The stack frame depth.</param>
        /// <param name="cond">The cond.</param>
        /// <param name="data">The data.</param>
        /// created 15.10.2006 18:04
        [DebuggerHidden]
        public static void ThrowAssertionFailed(int stackFrameDepth, string cond, string data)
        {
            var result = AssertionFailed(stackFrameDepth + 1, cond, data);
            throw new AssertionFailedException(result);
        }

        /// <summary>
        /// Throws the assertion failed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="s1">The s1.</param>
        /// created 16.12.2006 18:28
        [DebuggerHidden]
        public static void ThrowAssertionFailed(string s, string s1)
        {
            ThrowAssertionFailed(1, s, s1);
        }

        /// <summary>
        /// Function used in assertions
        /// </summary>
        /// <param name="stackFrameDepth">The stack frame depth.</param>
        /// <param name="cond">The cond.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static string AssertionFailed(int stackFrameDepth, string cond, string data)
        {
            var result = "Assertion Failed: " + cond + "\nData: " + data;
            FlaggedLine(stackFrameDepth + 1, result);
            Debugger.Break();
            return result;
        }

        /// <summary>
        /// Check boolean expression
        /// </summary>
        /// <param name="stackFrameDepth">The stack frame depth.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <param name="text">The text.</param>
        [DebuggerHidden, AssertionMethod]
        public static void Assert(int stackFrameDepth, [AssertionCondition(AssertionConditionType.IS_TRUE)] bool b, string text)
        {
            if(b)
                return;
            AssertionFailed(stackFrameDepth + 1, "", text);
        }

        /// <summary>
        /// Check boolean expression
        /// </summary>
        /// <param name="stackFrameDepth">The stack frame depth.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        [DebuggerHidden, AssertionMethod]
        public static void Assert(int stackFrameDepth, [AssertionCondition(AssertionConditionType.IS_TRUE)] bool b)
        {
            if(b)
                return;
            AssertionFailed(stackFrameDepth + 1, "", "");
        }

        /// <summary>
        /// Asserts the specified b.
        /// </summary>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// created 16.12.2006 18:27
        [DebuggerHidden, AssertionMethod]
        public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] bool b)
        {
            Assert(1, b);
        }

        /// <summary>
        /// Asserts the specified b.
        /// </summary>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <param name="s">The s.</param>
        /// created 16.12.2006 18:29
        [DebuggerHidden, AssertionMethod]
        public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] bool b, string s)
        {
            Assert(1, b, s);
        }

        /// <summary>
        /// Assertions the failed.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="s1">The s1.</param>
        /// created 16.12.2006 18:30
        [DebuggerHidden]
        public static void AssertionFailed(string s, string s1)
        {
            AssertionFailed(1, s, s1);
        }

        /// <summary>
        /// Outputs the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// Created 09.09.07 12:03 by hh on HAHOYER-DELL
        public static void ConsoleOutput(string text)
        {
            FlaggedLine(text);
            Console.WriteLine(text);
        }

        private class AssertionFailedException : Exception
        {
            public AssertionFailedException(string result) : base(result) {}
        }
    }
}