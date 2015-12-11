using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace hw.DebugFormatter
{
    sealed class TextWriters : TextWriter
    {
        readonly TextWriter[] _writers;
        public TextWriters(params TextWriter[] writers) { _writers = writers; }

        public override void Write(char value)
        {
            foreach(var writer in _writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach(var writer in _writers)
                writer.Write(value);
        }

        public override void WriteLine(string value)
        {
            foreach(var writer in _writers)
                writer.WriteLine(value);
        }
        public override Encoding Encoding { get { return _writers.First().Encoding; } }
    }
}