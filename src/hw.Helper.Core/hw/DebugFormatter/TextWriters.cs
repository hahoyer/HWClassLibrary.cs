using System.Text;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

public sealed class TextWriters : TextWriter
{
    readonly TextWriter[] Writers;
    public TextWriters(params TextWriter[] writers) 
        => Writers 
            = Flatten(writers)
                .Distinct()
                .ToArray();

    IEnumerable<TextWriter> Flatten(TextWriter[] writers) 
        => writers.SelectMany(Flatten);

    IEnumerable<TextWriter> Flatten(TextWriter writer) 
        => writer is TextWriters writers
            ? Flatten(writers.Writers) 
            : [writer];

    public override Encoding Encoding => Writers.First().Encoding;

    public override void Write(char value)
    {
        foreach(var writer in Writers)
            writer.Write(value);
    }

    public override void Write(string? value)
    {
        foreach(var writer in Writers)
            writer.Write(value);
    }

    public override void WriteLine(string? value)
    {
        foreach(var writer in Writers)
            writer.WriteLine(value);
    }
}