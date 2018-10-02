using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.DebugFormatter
{
    /// <summary>     Summary description for Tracer. </summary>
    public static class Tracer
    {
        sealed class AssertionFailedException : Exception
        {
            public AssertionFailedException(string result)
                : base(result) {}
        }

        sealed class BreakException : Exception {}

        [UsedImplicitly]
        public const string VisualStudioLineFormat =
            "{fileName}({lineNr},{colNr},{lineNrEnd},{colNrEnd}): {tagText}: ";

        static readonly Writer _writer = new Writer();
        public static readonly Dumper Dumper = new Dumper();

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
            return FilePosn
            (
                sf.GetFileName(),
                sf.GetFileLineNumber() - 1,
                sf.GetFileColumnNumber(),
                tag
            );
        }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="fileName"> asis </param>
        /// <param name="lineNr"> asis </param>
        /// <param name="colNr"> asis </param>
        /// <param name="tag"> asis </param>
        /// <returns> the "fileName(lineNr,colNr): tag: " string </returns>
        public static string FilePosn
        (
            string fileName,
            int lineNr,
            int colNr,
            FilePositionTag tag
        )
            => FilePosn(fileName, lineNr, colNr, lineNr, colNr, tag);

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="fileName"> asis </param>
        /// <param name="lineNr"> asis </param>
        /// <param name="colNr"> asis </param>
        /// <param name="colNrEnd"></param>
        /// <param name="tag"> asis </param>
        /// <param name="lineNrEnd"></param>
        /// <returns> the "fileName(lineNr,colNr): tag: " string </returns>
        public static string FilePosn
        (
            string fileName,
            int lineNr,
            int colNr,
            int lineNrEnd,
            int colNrEnd,
            FilePositionTag tag)
        {
            var tagText = tag.ToString();
            return FilePosn(fileName, lineNr, colNr, lineNrEnd, colNrEnd, tagText);
        }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="fileName"> asis </param>
        /// <param name="lineNr"> asis </param>
        /// <param name="colNr"> asis </param>
        /// <param name="colNrEnd"></param>
        /// <param name="tagText"> asis </param>
        /// <param name="lineNrEnd"></param>
        /// <returns> the "fileName(lineNr,colNr): tag: " string </returns>
        public static string FilePosn
        (
            string fileName,
            int lineNr,
            int colNr,
            int lineNrEnd,
            int colNrEnd,
            string tagText)
            => VisualStudioLineFormat
                .Replace("{fileName}", fileName)
                .Replace("{lineNr}", (lineNr + 1).ToString())
                .Replace("{colNr}", colNr.ToString())
                .Replace("{lineNrEnd}", (lineNrEnd + 1).ToString())
                .Replace("{colNrEnd}", colNrEnd.ToString())
                .Replace("{tagText}", tagText);

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
                result += m.GetGenericArguments().Select(t => t.PrettyName()).Stringify(", ");
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
        /// <param name="stackFrameDepth"> the index of stack frame </param>
        /// <param name="tag"> </param>
        /// <param name="showParam"> controls if parameter list is appended </param>
        /// <returns> string to inspect the method call </returns>
        public static string MethodHeader
        (
            FilePositionTag tag = FilePositionTag.Debug,
            bool showParam = false,
            int stackFrameDepth = 0)
        {
            var sf = new StackTrace(true).GetFrame(stackFrameDepth + 1);
            return FilePosn(sf, tag) + DumpMethod(sf.GetMethod(), showParam);
        }

        public static string CallingMethodName(int stackFrameDepth = 0)
        {
            var sf = new StackTrace(true).GetFrame(stackFrameDepth + 1);
            return DumpMethod(sf.GetMethod(), false);
        }

        [UsedImplicitly]
        public static string StackTrace(FilePositionTag tag, int stackFrameDepth = 0)
        {
            var stackTrace = new StackTrace(true);
            var result = "";
            for(var i = stackFrameDepth + 1; i < stackTrace.FrameCount; i++)
            {
                var stackFrame = stackTrace.GetFrame(i);
                var filePosn = FilePosn(stackFrame, tag) + DumpMethod(stackFrame.GetMethod(), false);
                result += "\n" + filePosn;
            }

            return result;
        }

        /// <summary>
        ///     write a line to debug output
        /// </summary>
        /// <param name="s"> the text </param>
        public static void Line(string s) => _writer.ThreadSafeWrite(s, true);

        /// <summary>
        ///     write a line to debug output
        /// </summary>
        /// <param name="s"> the text </param>
        public static void LinePart(string s) => _writer.ThreadSafeWrite(s, false);

        /// <summary>
        ///     write a line to debug output, flagged with FileName(LineNr,ColNr): Method (without parameter list)
        /// </summary>
        /// <param name="s"> the text </param>
        /// <param name="flagText"> </param>
        /// <param name="showParam"></param>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        public static void FlaggedLine
        (
            string s,
            FilePositionTag flagText = FilePositionTag.Debug,
            bool showParam = false,
            int stackFrameDepth = 0)
        {
            var methodHeader = MethodHeader
            (
                flagText,
                stackFrameDepth: stackFrameDepth + 1,
                showParam: showParam);
            Line(methodHeader + " " + s);
        }

        /// <summary>
        ///     generic dump function by use of reflection
        /// </summary>
        /// <param name="x"> the object to dump </param>
        /// <returns> </returns>
        public static string Dump(object x) => Dumper.Dump(x);


        /// <summary>
        ///     generic dump function by use of reflection
        /// </summary>
        /// <param name="x"> the object to dump </param>
        /// <returns> </returns>
        public static string DumpData(object x)
        {
            if(x == null)
                return "";
            return Dumper.DumpData(x);
        }

        /// <summary>
        ///     Generic dump function by use of reflection.
        /// </summary>
        /// <param name="name">Identifies the the value in result. Recomended use is nameof($value$), but any string is possible.</param>
        /// <param name="value">The object, that will be dumped, by use of <see cref="DumpData(object)" />.</param>
        /// <returns>A string according to pattern $name$ = $value$</returns>
        [DebuggerHidden]
        public static string DumpValue(this string name, object value)
            => DumpData("", new[] {name, value}, 1);

        /// <summary>
        ///     creates a string to inspect the method call contained in stack. Runtime parameters are dumped too.
        /// </summary>
        /// <param name="parameter"> parameter objects list for the frame </param>
        public static void DumpStaticMethodWithData(params object[] parameter)
        {
            var result = DumpMethodWithData("", null, parameter, 1);
            Line(result);
        }

        internal static string DumpMethodWithData
            (string text, object thisObject, object[] parameter, int stackFrameDepth = 0)
        {
            var sf = new StackTrace(true).GetFrame(stackFrameDepth + 1);
            return FilePosn
                       (sf, FilePositionTag.Debug) +
                   DumpMethod(sf.GetMethod(), true) +
                   text +
                   DumpMethodWithData(sf.GetMethod(), thisObject, parameter).Indent();
        }

        /// <summary>
        ///     Dumps the data.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <param name="data"> The data, as name/value pair. </param>
        /// <param name="stackFrameDepth"> The stack stackFrameDepth. </param>
        /// <returns> </returns>
        public static string DumpData(string text, object[] data, int stackFrameDepth = 0)
        {
            var sf = new StackTrace(true).GetFrame(stackFrameDepth + 1);
            return FilePosn
                       (sf, FilePositionTag.Debug) +
                   DumpMethod(sf.GetMethod(), true) +
                   text +
                   DumpMethodWithData(null, data).Indent();
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
        /// <param name="cond"> The cond. </param>
        /// <param name="getText"> The data. </param>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static void ConditionalBreak
            (string cond, Func<string> getText = null, int stackFrameDepth = 0)
        {
            var result = "Conditional break: " + cond + "\nData: " + (getText == null ? "" : getText());
            FlaggedLine(result, stackFrameDepth: stackFrameDepth + 1);
            TraceBreak();
        }

        /// <summary>
        ///     Check boolean expression
        /// </summary>
        /// <param name="b">
        ///     if set to <c>true</c> [b].
        /// </param>
        /// <param name="getText"> The text. </param>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        [DebuggerHidden]
        public static void ConditionalBreak
            (bool b, Func<string> getText = null, int stackFrameDepth = 0)
        {
            if(b)
                ConditionalBreak("", getText, stackFrameDepth + 1);
        }

        /// <summary>
        ///     Throws the assertion failed.
        /// </summary>
        /// <param name="cond"> The cond. </param>
        /// <param name="getText"> The data. </param>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// created 15.10.2006 18:04
        [DebuggerHidden]
        public static void ThrowAssertionFailed
            (string cond, Func<string> getText = null, int stackFrameDepth = 0)
        {
            var result = AssertionFailed(cond, getText, stackFrameDepth + 1);
            throw new AssertionFailedException(result);
        }

        /// <summary>
        ///     Function used in assertions
        /// </summary>
        /// <param name="cond"> The cond. </param>
        /// <param name="getText"> The data. </param>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        /// <returns> </returns>
        [DebuggerHidden]
        public static string AssertionFailed
            (string cond, Func<string> getText = null, int stackFrameDepth = 0)
        {
            var result = "Assertion Failed: " + cond + "\nData: " + (getText == null ? "" : getText());
            FlaggedLine(result, stackFrameDepth: stackFrameDepth + 1);
            AssertionBreak(result);
            return result;
        }

        /// <summary>
        ///     Check boolean expression
        /// </summary>
        /// <param name="b">
        ///     if set to <c>true</c> [b].
        /// </param>
        /// <param name="getText"> The text. </param>
        /// <param name="stackFrameDepth"> The stack frame depth. </param>
        [DebuggerHidden]
        [ContractAnnotation("b: false => halt")]
        public static void Assert(bool b, Func<string> getText = null, int stackFrameDepth = 0)
        {
            if(b)
                return;
            AssertionFailed("", getText, stackFrameDepth + 1);
        }

        [DebuggerHidden]
        [ContractAnnotation("b: false => halt")]
        public static void Assert(bool b, string s) {Assert(b, () => s, 1);}

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

        public static int CurrentFrameCount(int stackFrameDepth) => new StackTrace(true).FrameCount - stackFrameDepth;

        [DebuggerHidden]
        public static void LaunchDebugger() => Debugger.Launch();

        public static void IndentStart() => _writer.IndentStart();
        public static void IndentEnd() => _writer.IndentEnd();

        public static void WriteLine(this string value) => Line(value);
        public static void WriteLinePart(this string value) => LinePart(value);
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

    public abstract class DumpAttributeBase : Attribute {}
}