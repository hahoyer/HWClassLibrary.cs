using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

public sealed class DebugTextWriter : TextWriter
{
    [PublicAPI]
    public static bool Enabled = true;

    static readonly DebugTextWriter Instance = new();

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        if(Enabled)
            Debug.Write(value);
    }

    public override void Write(string value)
    {
        if(Enabled)
            Debug.Write(value);
    }

    public override void WriteLine(string value)
    {
        if(Enabled)
            Debug.WriteLine(value);
    }

    public static void Register(bool exclusive = false)
    {
        if(!Debugger.IsAttached)
            return;

        Console.SetOut(exclusive? Instance : new TextWriters(Instance, Console.Out));
    }
}