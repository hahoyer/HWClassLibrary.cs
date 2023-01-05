using System;
using System.Diagnostics;
using System.Threading;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

sealed class Writer
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

    readonly WriteInitiator Instance = new();

    int IndentCount;
    bool IsLineStart = true;

    public Writer() => DebugTextWriter.Register();

    internal void IndentStart() => IndentCount++;
    internal void IndentEnd() => IndentCount--;

    internal void ThreadSafeWrite(string text, bool isLine)
    {
        lock(Instance)
        {
            Instance.NewThread();

            text = text.Indent(isLineStart: IsLineStart, count: IndentCount);

            if(Instance.ThreadChanged && Debugger.IsAttached)
            {
                var threadFlagString = Instance.ThreadFlagString;
                if(!IsLineStart)
                {
                    threadFlagString = "\n" + threadFlagString;
                    if(text == "" || text[0] != '\n')
                        threadFlagString = "..." + threadFlagString;
                }

                Debug.Write(threadFlagString);
            }

            Write(text, isLine);

            IsLineStart = isLine;
        }
    }

    static void Write(string text, bool isLine)
    {
        if(isLine)
            Console.WriteLine(text);
        else
            Console.Write(text);
    }
}