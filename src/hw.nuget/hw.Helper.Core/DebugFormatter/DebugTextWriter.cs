using System;
using System.Diagnostics;
using System.IO;
using System.Text;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

public sealed class DebugTextWriter : TextWriter
{
    static readonly DebugTextWriter Instance = new();

    public override Encoding Encoding => Encoding.UTF8;
    public override void Write(char value) => Debug.Write(value);
    public override void Write(string value) => Debug.Write(value);
    public override void WriteLine(string value) => Debug.WriteLine(value);

    public static void Register(bool exclusive = true)
    {
        if(!Debugger.IsAttached)
            return;

        Console.SetOut(exclusive? Instance : new TextWriters(Instance, Console.Out));
    }
}