using System.Text;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

sealed class TextWriters(params TextWriter[] writers) : TextWriter
{
    public override Encoding Encoding => writers.First().Encoding;

    public override void Write(char value)
    {
        foreach(var writer in writers)
            writer.Write(value);
    }

    public override void Write(string? value)
    {
        foreach(var writer in writers)
            writer.Write(value);
    }

    public override void WriteLine(string? value)
    {
        foreach(var writer in writers)
            writer.WriteLine(value);
    }
}