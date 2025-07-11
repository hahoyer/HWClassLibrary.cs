using System.Diagnostics;
using System.Reflection;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

[PublicAPI]
public static class Tracer
{
    sealed class AssertionFailedException(string result) : Exception(result);

    sealed class BreakException : Exception;

    public const string VisualStudioLineFormat =
        "{fileName}({lineNumber},{columnNumber},{lineNumberEnd},{columnNumberEnd}): {tagText}: ";

    public static readonly Dumper Dumper = new();

    public static bool IsBreakDisabled;

    static readonly Writer Writer = new();

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
                    LineNumber = stackFrame.GetFileLineNumber() - 1, ColumnNumber = stackFrame.GetFileColumnNumber()-1
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
                    LineNumber = lineNumber, ColumnNumber = columnNumber1-1
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
                    LineNumber = lineNumber, ColumnNumber = columnNumber1-1
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
                , End = new() { LineNumber = lineNumberEnd, ColumnNumber = columnNumber1End - 1
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
            .Replace("{columnNumberEnd}", (end.ColumnNumber+1).ToString())
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

    public static string CallingMethodName(int stackFrameDepth = 0)
    {
        var stackFrame = new StackTrace(true).GetFrame(stackFrameDepth + 1);
        return (stackFrame?.GetMethod()).DumpMethod(false);
    }

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

    [DebuggerHidden]
    [ContractAnnotation("b: false => halt")]
    public static void Assert(this bool b, string s) => b.Assert(() => s, 1);

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

    public static int CurrentFrameCount(int stackFrameDepth) => new StackTrace(true).FrameCount - stackFrameDepth;

    [DebuggerHidden]
    public static void LaunchDebugger() => Debugger.Launch();

    public static void IndentStart() => Writer.IndentStart();
    public static void IndentEnd() => Writer.IndentEnd();

    [IsLoggingFunction]
    public static void Log(this string? value, LogLevel level = LogLevel.Information)
        => Writer.ThreadSafeWrite(value, true, level);

    [IsLoggingFunction]
    public static void LogLinePart(this string value, LogLevel level = LogLevel.Information)
        => Writer.ThreadSafeWrite(value, false, level);

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
            result += (parameterInfos[index].Name??"_").IsSetTo(parameters[index]);
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

[PublicAPI]
public enum FilePositionTag
{
    Debug
    , Output
    , Query
    , Test
    , Profiler
}

interface IDumpExceptAttribute
{
    bool IsException(object? target);
}

public abstract class DumpAttributeBase : Attribute;
public sealed class IsLoggingFunction : Attribute;