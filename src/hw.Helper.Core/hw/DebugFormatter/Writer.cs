using System.Diagnostics;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Indented, thread-aware sink for log output. Writes are serialised with a lock and prefixed with a
///     <c>[oldThreadId-&gt;newThreadId]</c> marker whenever the active thread changes (visible only when
///     a debugger is attached).
/// </summary>
public sealed class Writer
{
    sealed class WriteInitiator
    {
        string LastName = "";
        string Name = "";

        public bool ThreadChanged => Name != LastName;

        public string ThreadFlagString => "[" + LastName + "->" + Name + "]\n";

        public void NewThread()
        {
            LastName = Name;
            Name = Thread.CurrentThread.ManagedThreadId.ToString();
        }
    }

    /// <summary>
    ///     Severity ambient flag set for the duration of every write. Sinks reading this value see the
    ///     level of the message currently being emitted.
    /// </summary>
    [PublicAPI]
    public static LogLevel LogLevel = LogLevel.Information;

    readonly WriteInitiator Instance = new();
    int IndentCount;
    bool IsLineStart = true;

    /// <summary>
    ///     Constructs a writer and registers <see cref="DebugTextWriter" /> with the console (no-op when
    ///     no debugger is attached).
    /// </summary>
    public Writer() => DebugTextWriter.Register();

    internal void IndentStart() => IndentCount++;
    internal void IndentEnd() => IndentCount--;

    internal void ThreadSafeWrite(string? text, bool isLine, LogLevel level)
    {
        if(level == LogLevel.None)
            return;
        lock(Instance)
        {
            Instance.NewThread();

            text = text?.Indent(isLineStart: IsLineStart, count: IndentCount);

            if(Instance.ThreadChanged && Debugger.IsAttached)
            {
                var threadFlagString = Instance.ThreadFlagString;
                if(!IsLineStart)
                {
                    threadFlagString = "\n" + threadFlagString;
                    if(text == "" || text?[0] != '\n')
                        threadFlagString = "..." + threadFlagString;
                }

                Debug.Write(threadFlagString);
            }

            Write(text, isLine, level);

            IsLineStart = isLine;
        }
    }

    static void Write(string? text, bool isLine, LogLevel level)
    {
        var lastLevel = LogLevel;
        LogLevel = level;
        if(isLine)
            Console.WriteLine(text);
        else
            Console.Write(text);
        LogLevel = lastLevel;
    }
}