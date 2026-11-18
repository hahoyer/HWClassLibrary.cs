using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Static facade for diagnostic output: dump formatting, stack-trace inspection, assertion helpers
///     and indented logging. Most members are extension methods so they read naturally at the call site.
/// </summary>
[PublicAPI]
public static class Tracer
{
    extension(string target)
    {
        /// <summary>
        ///     Parses a textual <see cref="Exception.StackTrace" /> into structured frames. Lines that
        ///     do not match the expected ".NET <c>at … in … :line N</c>" shape are silently dropped.
        /// </summary>
        public IEnumerable<StackTraceLevel> ParseStackTrace()
        {
            if(string.IsNullOrWhiteSpace(target))
                return [];

            return StackFrameRegex
                .Matches(target)
                .Select(match => new StackTraceLevel
                {
                    Method = match.Groups["method"].Value.Trim(), Filename = match.Groups["file"].Value.Trim()
                    , LineNrString = match.Groups["line"].Value.Trim()
                });
        }
    }

    extension(Exception exception)
    {
        /// <summary>
        ///     Multi-line rendering of <paramref name="exception" /> with the throwing method, an
        ///     IDE-clickable stack trace and any inner exception, suitable for pasting into a log.
        /// </summary>
        public string Dump
        {
            get
            {
                var levels = exception.StackTrace?.ParseStackTrace().ToArray() ?? [];
                var targetSite = exception.TargetSite == null
                    ? null
                    : $"Method: {exception.TargetSite.DumpMethod(false)}\r\nStack:"
                    + levels
                        .Select
                        (level
                            =>
                            "\n"
                            + FilePosition
                            (
                                level.Filename,
                                level.LineNr,
                                1,
                                FilePositionTag.Exception
                            )
                            + level.Method
                        )
                        .Stringify()
                        .Indent();
                var result
                    = $"{(targetSite == null? "" : targetSite + ": ")}{exception.GetType().PrettyName()}({exception.Message.Quote()})";
                if(exception.InnerException == null)
                    return result;
                return result + "\nInnerException:\n  " + exception.InnerException.LogDump().Indent();
            }
        }
    }

    sealed class AssertionFailedException(string result) : Exception(result);

    sealed class BreakException : Exception;

    /// <summary>
    ///     Pattern used by <see cref="FilePosition(string?, TextPart?, FilePositionTag)" /> and
    ///     friends. Following Visual Studio's "Edit.GotoNextLocation" convention makes the resulting
    ///     line clickable in the IDE output window.
    /// </summary>
    public const string VisualStudioLineFormat =
        "{fileName}({lineNumber},{columnNumber},{lineNumberEnd},{columnNumberEnd}): {tagText}: ";

    /// <summary>Shared dump engine used by <see cref="Dump" />, <see cref="DumpData(object?)" /> and friends.</summary>
    public static readonly Dumper Dumper = new();

    /// <summary>
    ///     When <c>true</c>, <see cref="TraceBreak" /> and assertion helpers throw instead of calling
    ///     <see cref="Debugger.Break" />. Useful for unit tests that exercise failing paths.
    /// </summary>
    public static bool IsBreakDisabled;

    static readonly Writer Writer = new();

    static readonly Regex StackFrameRegex = new(
        @"^\s*at\s+(?<method>.+?)\s+in\s+(?<file>.+?):line\s+(?<line>\d+)",
        RegexOptions.Multiline | RegexOptions.Compiled
    );

    /// <summary>
    ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
    /// </summary>
    /// <param name="stackFrame"> the stack frame where the location is stored </param>
    /// <param name="tag"> </param>
    /// <returns> the "FileName(lineNumber,ColNr): tag: " string </returns>
    public static string FilePosition(StackFrame? stackFrame, FilePositionTag tag)
    {
        // ReSharper disable once StringLiteralTypo
        if(stackFrame == null || stackFrame.GetFileLineNumber() == 0)
            return "<nofile> " + tag;
        return FilePosition
        (
            stackFrame.GetFileName(),
            new()
            {
                Start = new()
                {
                    LineNumber = stackFrame.GetFileLineNumber() - 1, ColumnNumber = stackFrame.GetFileColumnNumber() - 1
                }
            }
            ,
            tag
        );
    }

    /// <summary>
    ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
    /// </summary>
    public static string FilePosition
    (
        string? fileName,
        int lineNumber,
        int columnNumber1,
        FilePositionTag tag
    )
        => FilePosition(fileName
            , new()
            {
                Start = new()
                {
                    LineNumber = lineNumber, ColumnNumber = columnNumber1 - 1
                }
            }
            , tag);

    /// <summary>
    ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
    /// </summary>
    public static string FilePosition
    (
        string? fileName,
        int lineNumber,
        int columnNumber1,
        int lineNumberEnd,
        int columnNumber1End,
        FilePositionTag tag
    )
        => FilePosition(fileName
            , new()
            {
                Start = new()
                {
                    LineNumber = lineNumber, ColumnNumber = columnNumber1 - 1
                }
                , End = new()
                {
                    LineNumber = lineNumberEnd, ColumnNumber = columnNumber1End - 1
                }
            }
            , tag);

    /// <summary>
    ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
    /// </summary>
    public static string FilePosition
    (
        string? fileName,
        int lineNumber,
        int columnNumber1,
        int lineNumberEnd,
        int columnNumber1End,
        string tagText
    )
        => FilePosition(fileName
            , new()
            {
                Start = new() { LineNumber = lineNumber, ColumnNumber = columnNumber1 - 1 }
                , End = new()
                {
                    LineNumber = lineNumberEnd, ColumnNumber = columnNumber1End - 1
                }
            }
            , tagText);

    public static string FilePosition(string? fileName, TextPart? textPart, FilePositionTag tag)
        => FilePosition(fileName, textPart, tag.ToString());

    public static string FilePosition(string? fileName, TextPart? textPart, string tagText)
    {
        var start = textPart?.Start ?? new TextPosition { LineNumber = 1, ColumnNumber = 0 };
        var end = textPart?.End ?? start;
        return VisualStudioLineFormat
            .Replace("{fileName}", fileName)
            .Replace("{lineNumber}", (start.LineNumber + 1).ToString())
            .Replace("{columnNumber}", (start.ColumnNumber + 1).ToString())
            .Replace("{lineNumberEnd}", (end.LineNumber + 1).ToString())
            .Replace("{columnNumberEnd}", (end.ColumnNumber + 1).ToString())
            .Replace("{tagText}", tagText);
    }

    /// <summary>
    ///     creates a string to inspect a method
    /// </summary>
    /// <param name="methodBase"> the method </param>
    /// <param name="showParam"> controls if parameter list is appended </param>
    /// <returns> string to inspect a method </returns>
    public static string DumpMethod(this MethodBase? methodBase, bool showParam)
    {
        if(methodBase == null)
            return "<nomethod>";

        var result = methodBase.DeclaringType!.PrettyName() + ".";
        result += methodBase.Name;
        if(!showParam)
            return result;
        if(methodBase.IsGenericMethod)
        {
            result += "<";
            result += methodBase.GetGenericArguments().Select(t => t.PrettyName()).Stringify(", ");
            result += ">";
        }

        result += "(";
        for(int parameterIndex = 0, n = methodBase.GetParameters().Length; parameterIndex < n; parameterIndex++)
        {
            if(parameterIndex != 0)
                result += ", ";
            var parameterInfo = methodBase.GetParameters()[parameterIndex];
            result += parameterInfo.ParameterType.PrettyName();
            result += " ";
            result += parameterInfo.Name;
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
        int stackFrameDepth = 0
    )
    {
        var stackFrame = new StackTrace(true).GetFrame(stackFrameDepth + 1);
        if(stackFrame == null)
            return "";
        return FilePosition(stackFrame, tag) + stackFrame.GetMethod().DumpMethod(showParam);
    }

    /// <summary>
    ///     Returns a printable name for the method <paramref name="stackFrameDepth" /> frames above
    ///     the caller of this function (without parameter list).
    /// </summary>
    /// <param name="stackFrameDepth"> Number of additional frames to skip above the caller. </param>
    public static string CallingMethodName(int stackFrameDepth = 0)
    {
        var stackFrame = new StackTrace(true).GetFrame(stackFrameDepth + 1);
        return (stackFrame?.GetMethod()).DumpMethod(false);
    }

    /// <summary>
    ///     Multi-line dump of the call stack at the call site, with each line formatted via
    ///     <see cref="FilePosition(StackFrame?, FilePositionTag)" /> so the IDE can navigate to it.
    /// </summary>
    /// <param name="tag"> Tag stamped on every line (e.g. <see cref="FilePositionTag.Debug" />). </param>
    /// <param name="stackFrameDepth"> Number of frames to skip at the top of the trace. </param>
    public static string StackTrace(FilePositionTag tag, int stackFrameDepth = 0)
    {
        var stackTrace = new StackTrace(true);
        var result = "";
        for(var frameDepth = stackFrameDepth + 1; frameDepth < stackTrace.FrameCount; frameDepth++)
        {
            var stackFrame = stackTrace.GetFrame(frameDepth);
            var filePosition = FilePosition(stackFrame, tag) + (stackFrame?.GetMethod()).DumpMethod(false);
            result += "\n" + filePosition;
        }

        return result;
    }

    /// <summary>
    ///     write a line to debug output, flagged with FileName(lineNumber,ColNr): Method (without parameter list)
    /// </summary>
    /// <param name="text"> the text </param>
    /// <param name="flagText"> </param>
    /// <param name="showParam"></param>
    /// <param name="stackFrameDepth"> The stack frame depth. </param>
    /// <param name="level"></param>
    [IsLoggingFunction]
    public static void FlaggedLine
    (
        this string text,
        FilePositionTag flagText = FilePositionTag.Debug,
        bool showParam = false,
        int stackFrameDepth = 0,
        LogLevel level = LogLevel.Debug
    )
    {
        var methodHeader = MethodHeader
        (
            flagText,
            stackFrameDepth: stackFrameDepth + 1,
            showParam: showParam);
        (methodHeader + " " + text).Log(level);
    }

    /// <summary>
    ///     Logs <paramref name="text" /> prefixed with the caller's method header (file/line + method
    ///     name).
    /// </summary>
    /// <param name="text"> The text to log. </param>
    /// <param name="flagText"> Tag stamped on the file-position prefix. </param>
    /// <param name="showParam"> If <c>true</c>, includes the parameter list in the method header. </param>
    /// <param name="stackFrameDepth"> Frames to skip when locating the caller. </param>
    /// <param name="level"> Severity of the resulting log entry. </param>
    [IsLoggingFunction]
    public static void Log
    (
        this string text,
        FilePositionTag flagText,
        bool showParam = false,
        int stackFrameDepth = 0,
        LogLevel level = LogLevel.Debug
    )
    {
        var methodHeader = MethodHeader
        (
            flagText,
            stackFrameDepth: stackFrameDepth + 1,
            showParam: showParam);
        (methodHeader + " " + text).Log(level);
    }

    /// <summary>
    ///     generic dump function by use of reflection
    /// </summary>
    /// <param name="target"> the object to dump </param>
    /// <returns> </returns>
    public static string Dump(object? target) => Dumper.Dump(target);

    /// <summary>Extension-method form of <see cref="Dump" /> for fluent call sites.</summary>
    public static string LogDump(this object target) => Dumper.Dump(target);

    /// <summary>
    ///     generic dump function by use of reflection
    /// </summary>
    /// <param name="target"> the object to dump </param>
    /// <returns> </returns>
    public static string DumpData(object? target) => target == null? "" : Dumper.DumpData(target);

    /// <summary>
    ///     Generic dump function by use of reflection.
    /// </summary>
    /// <param name="name">Identifies the value in result. Recommended use is nameof($value$), but any string is possible.</param>
    /// <param name="value">The object, that will be dumped, by use of <see cref="DumpData(object)" />.</param>
    /// <returns>A string according to pattern $name$ = $value$</returns>
    [DebuggerHidden]
    public static string DumpValue(this string name, object value)
        => DumpData("", [name, value], 1);

    /// <summary>
    ///     creates a string to inspect the method call contained in stack. Runtime parameters are dumped too.
    /// </summary>
    /// <param name="parameter"> parameter objects list for the frame </param>
    public static void DumpStaticMethodWithData(params object[] parameter)
    {
        var result = DumpMethodWithData("", null, parameter, 1);
        result.Log();
    }

    /// <summary>
    ///     Dumps the data.
    /// </summary>
    /// <param name="text"> The text. </param>
    /// <param name="data"> The data, as name/value pair. </param>
    /// <param name="stackFrameDepth"> The stack stackFrameDepth. </param>
    /// <returns> </returns>
    public static string DumpData(string text, object?[] data, int stackFrameDepth = 0)
    {
        var stackFrame = new StackTrace(true).GetFrame(stackFrameDepth + 1);
        return FilePosition
                (stackFrame, FilePositionTag.Debug)
            + (stackFrame?.GetMethod()).DumpMethod(true)
            + text
            + DumpMethodWithData(null, data).Indent();
    }

    /// <summary>Formats a name/value pair as <c>name=&lt;dump-of-value&gt;</c>.</summary>
    public static string IsSetTo(this string name, object? value) => name + "=" + Dump(value);

    /// <summary>
    ///     Function used for condition al break
    /// </summary>
    /// <param name="cond"> The cond. </param>
    /// <param name="getText"> The data. </param>
    /// <param name="stackFrameDepth"> The stack frame depth. </param>
    /// <returns> </returns>
    [DebuggerHidden]
    [IsLoggingFunction]
    public static void UnconditionalBreak(string cond, Func<string>? getText = null, int stackFrameDepth = 0)
    {
        var result = "Conditional break: " + cond + "\nData: " + (getText == null? "" : getText());
        result.FlaggedLine(stackFrameDepth: stackFrameDepth + 1);
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
    public static void ConditionalBreak(this bool b, Func<string>? getText = null, int stackFrameDepth = 0)
    {
        if(b)
            UnconditionalBreak("", getText, stackFrameDepth + 1);
    }

    /// <summary>
    ///     Throws the assertion failed.
    /// </summary>
    /// <param name="cond"> The cond. </param>
    /// <param name="getText"> The data. </param>
    /// <param name="stackFrameDepth"> The stack frame depth. </param>
    /// created 15.10.2006 18:04
    [DebuggerHidden]
    public static void ThrowAssertionFailed(string cond, Func<string>? getText = null, int stackFrameDepth = 0)
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
    [IsLoggingFunction]
    public static string AssertionFailed(string cond, Func<string>? getText = null, int stackFrameDepth = 0)
    {
        var result = "Assertion Failed: " + cond + "\nData: " + (getText == null? "" : getText());
        result.FlaggedLine(stackFrameDepth: stackFrameDepth + 1);
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
    public static void Assert(this bool b, Func<string>? getText = null, int stackFrameDepth = 0)
    {
        if(b)
            return;
        AssertionFailed("", getText, stackFrameDepth + 1);
    }

    /// <summary>
    ///     Convenience overload of <see cref="Assert(bool, Func{string}?, int)" /> that takes a literal
    ///     message instead of a producer.
    /// </summary>
    /// <param name="b"> Condition that must hold. </param>
    /// <param name="s"> Message to record on failure. </param>
    [DebuggerHidden]
    [ContractAnnotation("b: false => halt")]
    public static void Assert(this bool b, string s) => b.Assert(() => s, 1);

    /// <summary>
    ///     Breaks into the debugger if one is attached. With <see cref="IsBreakDisabled" /> set, throws
    ///     a private break exception instead.
    /// </summary>
    [DebuggerHidden]
    public static void TraceBreak()
    {
        if(!Debugger.IsAttached)
            return;
        if(IsBreakDisabled)
            throw new BreakException();
        Debugger.Break();
    }

    /// <summary>
    ///     Check expression
    /// </summary>
    /// <param name="b">
    ///     if not null.
    /// </param>
    /// <param name="getText"> Message in case of fail. </param>
    /// <param name="stackFrameDepth"> The stack frame depth. </param>
    [DebuggerHidden]
    [ContractAnnotation("b: null => halt")]
    public static void AssertIsNotNull(this object? b, Func<string>? getText = null, int stackFrameDepth = 0)
    {
        if(b != null)
            return;
        AssertionFailed("", getText, stackFrameDepth + 1);
    }

    /// <summary>
    ///     Check expression
    /// </summary>
    /// <param name="b">
    ///     if null.
    /// </param>
    /// <param name="getText"> Message in case of fail. </param>
    /// <param name="stackFrameDepth"> The stack frame depth. </param>
    [DebuggerHidden]
    [ContractAnnotation("b: notnull => halt")]
    public static void AssertIsNull(this object? b, Func<string>? getText = null, int stackFrameDepth = 0)
    {
        if(b == null)
            return;
        AssertionFailed(b.LogDump(), getText, stackFrameDepth + 1);
    }

    /// <summary>
    ///     Check if expression has target type
    /// </summary>
    /// <param name="target">
    /// </param>
    /// <param name="getText"> Message in case of fail. </param>
    /// <param name="stackFrameDepth"> The stack frame depth. </param>
    [DebuggerHidden]
    public static void Assert<TTargetType>(this object target, Func<string>? getText = null, int stackFrameDepth = 0)
    {
        if(target is TTargetType)
            return;
        AssertionFailed($"is {typeof(TTargetType).PrettyName()}", getText, stackFrameDepth + 1);
    }

    /// <summary>
    ///     Returns the number of stack frames currently above the caller, optionally offset by
    ///     <paramref name="stackFrameDepth" />. Used by the method-dump infrastructure to detect
    ///     unbalanced start/end pairs.
    /// </summary>
    /// <param name="stackFrameDepth"> Frames to subtract from the raw frame count. </param>
    public static int CurrentFrameCount(int stackFrameDepth) => new StackTrace(true).FrameCount - stackFrameDepth;

    /// <summary>Launches a debugger from code; usually only sensible during initial process startup.</summary>
    [DebuggerHidden]
    public static void LaunchDebugger() => Debugger.Launch();

    /// <summary>Increases the global indent applied to subsequent <see cref="Log(string?, LogLevel)" /> calls.</summary>
    public static void IndentStart() => Writer.IndentStart();

    /// <summary>Decreases the global indent applied to subsequent <see cref="Log(string?, LogLevel)" /> calls.</summary>
    public static void IndentEnd() => Writer.IndentEnd();

    /// <summary>Writes <paramref name="value" /> as a complete line through the shared <see cref="Writer" />.</summary>
    /// <param name="value"> Line to write. </param>
    /// <param name="level"> Severity tagged onto the write. </param>
    [IsLoggingFunction]
    public static void Log(this string? value, LogLevel level = LogLevel.Information)
        => Writer.ThreadSafeWrite(value, true, level);

    /// <summary>Writes <paramref name="value" /> without appending a newline.</summary>
    /// <param name="value"> Text to append to the current line. </param>
    /// <param name="level"> Severity tagged onto the write. </param>
    [IsLoggingFunction]
    public static void LogLinePart(this string value, LogLevel level = LogLevel.Information)
        => Writer.ThreadSafeWrite(value, false, level);

    /// <summary>
    ///     Renders the call site, the runtime arguments and an optional prefix into the logging
    ///     format used by <c>StartMethodDump</c> and <see cref="DumpStaticMethodWithData" />.
    /// </summary>
    /// <param name="text"> Prefix shown before the parameter list. </param>
    /// <param name="thisObject"> Receiver of the call, or <c>null</c> for static methods. </param>
    /// <param name="parameter"> Argument values matching the method's signature, optionally followed by name/value pairs. </param>
    /// <param name="stackFrameDepth"> Frames to skip when locating the call site. </param>
    public static string DumpMethodWithData
        (string text, object? thisObject, object?[] parameter, int stackFrameDepth = 0)
    {
        var stackFrame = new StackTrace(true).GetFrame(stackFrameDepth + 1);
        return FilePosition
                (stackFrame, FilePositionTag.Debug)
            + (stackFrame?.GetMethod()).DumpMethod(true)
            + text
            + DumpMethodWithData(stackFrame?.GetMethod(), thisObject, parameter).Indent();
    }

    static string DumpMethodWithData(MethodBase? methodBase, object? target, object?[] parameters)
    {
        var result = "\n";
        result += "this".IsSetTo(target);
        result += "\n";
        result += DumpMethodWithData(methodBase?.GetParameters(), parameters);
        return result;
    }

    static string DumpMethodWithData(ParameterInfo[]? parameterInfos, object?[] parameters)
    {
        parameterInfos ??= [];
        var result = "";

        for(var index = 0; index < parameterInfos.Length; index++)
        {
            if(index > 0)
                result += "\n";
            result += (parameterInfos[index].Name ?? "_").IsSetTo(parameters[index]);
        }

        for(var index = parameterInfos.Length; index < parameters.Length; index += 2)
        {
            result += "\n";
            result += ((string)parameters[index]!).IsSetTo(parameters[index + 1]);
        }

        return result;
    }

    [DebuggerHidden]
    static void AssertionBreak(string result)
    {
        if(!Debugger.IsAttached || IsBreakDisabled)
            throw new AssertionFailedException(result);
        Debugger.Break();
    }
}

/// <summary>One parsed entry from an <see cref="Exception.StackTrace" /> string.</summary>
public class StackTraceLevel
{
    /// <summary>The "<c>at …</c>" method signature as captured from the original line.</summary>
    public required string Method;

    /// <summary>Source file the frame came from.</summary>
    public required string Filename;

    /// <summary>Raw line-number text; parse via <see cref="LineNr" />.</summary>
    public required string LineNrString;

    /// <summary>Numeric line number; throws if <see cref="LineNrString" /> is not a valid integer.</summary>
    public int LineNr => int.Parse(LineNrString);
}

/// <summary>
///     Tag stamped on the file-position prefix produced by
///     <see cref="Tracer.FilePosition(StackFrame?, FilePositionTag)" />. The tag becomes the third
///     field in the Visual-Studio-clickable line and helps distinguish the call's purpose.
/// </summary>
[PublicAPI]
public enum FilePositionTag
{
    /// <summary>General-purpose debug output.</summary>
    Debug
    , Output
    , Query
    , Test
    , Profiler
    , Exception
}

interface IDumpExceptAttribute
{
    bool IsException(object? target);
}

/// <summary>Common base for attributes recognised by the dump infrastructure.</summary>
public abstract class DumpAttributeBase : Attribute;

/// <summary>
///     Marks a method as a logging helper so that ReSharper-style call-site information (caller member
///     name, file, line) is preserved through it instead of being attributed to the helper itself.
/// </summary>
public sealed class IsLoggingFunction : Attribute;