using System.Text;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     Tee writer that forwards each write to several inner writers. Nested instances are flattened and
///     duplicate writers are removed, so wrapping repeatedly is safe.
/// </summary>
public sealed class TextWriters : TextWriter
{
    readonly TextWriter[] Writers;

    /// <param name="writers"> Writers to tee output into. Nested <see cref="TextWriters" /> are unwrapped. </param>
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

    /// <summary>The encoding of the first inner writer.</summary>
    public override Encoding Encoding => Writers.First().Encoding;

    /// <inheritdoc />
    public override void Write(char value)
    {
        foreach(var writer in Writers)
            writer.Write(value);
    }

    /// <inheritdoc />
    public override void Write(string? value)
    {
        foreach(var writer in Writers)
            writer.Write(value);
    }

    /// <inheritdoc />
    public override void WriteLine(string? value)
    {
        foreach(var writer in Writers)
            writer.WriteLine(value);
    }
}