using System.IO;
using System.Linq;
using System.Text;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

sealed class TextWriters : TextWriter
{
    readonly TextWriter[] Writers;
    public TextWriters(params TextWriter[] writers) => Writers = writers;
    public override Encoding Encoding => Writers.First().Encoding;

    public override void Write(char value)
    {
        foreach(var writer in Writers)
            writer.Write(value);
    }

    public override void Write(string value)
    {
        foreach(var writer in Writers)
            writer.Write(value);
    }

    public override void WriteLine(string value)
    {
        foreach(var writer in Writers)
            writer.WriteLine(value);
    }
}