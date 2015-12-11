using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace hw.DebugFormatter
{
    public sealed class DebugTextWriter : TextWriter
    {
        static readonly DebugTextWriter _instance = new DebugTextWriter();
        public override Encoding Encoding { get { return Encoding.UTF8; } }
        public override void Write(char value) { System.Diagnostics.Debug.Write(value); }
        public override void Write(string value) { System.Diagnostics.Debug.Write(value); }
        public override void WriteLine(string value) { System.Diagnostics.Debug.WriteLine(value); }
        public static void Register(bool exclusive = true)
        {
            if(!Debugger.IsAttached)
                return;

            Console.SetOut(exclusive ? (TextWriter) _instance : new TextWriters(_instance, Console.Out));
        }
    }
}