using System.Diagnostics;
using System.Text;

// ReSharper disable CheckNamespace

namespace hw.DebugFormatter;

/// <summary>
///     <see cref="TextWriter" /> that forwards every write to <see cref="Debug" /> (visible in the IDE
///     output window). Writes are gated by <see cref="Enabled" /> and only have an effect while a
///     debugger is attached.
/// </summary>
public sealed class DebugTextWriter : TextWriter
{
    /// <summary>Master switch that suppresses all output when set to <c>false</c>.</summary>
    [PublicAPI]
    public static bool Enabled = true;

    static readonly DebugTextWriter Instance = new();

    /// <inheritdoc />
    public override Encoding Encoding => Encoding.UTF8;

    /// <inheritdoc />
    public override void Write(char value)
    {
        if(Enabled)
            Debug.Write(value);
    }

    /// <inheritdoc />
    public override void Write(string? value)
    {
        if(Enabled)
            Debug.Write(value);
    }

    /// <inheritdoc />
    public override void WriteLine(string? value)
    {
        if(Enabled)
            Debug.WriteLine(value);
    }

    /// <summary>
    ///     Redirects <see cref="Console.Out" /> through this writer (no-op when no debugger is attached).
    /// </summary>
    /// <param name="exclusive">
    ///     When <c>true</c>, the previous <see cref="Console.Out" /> is dropped; otherwise it continues
    ///     to receive output in addition to the debug stream.
    /// </param>
    public static void Register(bool exclusive = false)
    {
        if(!Debugger.IsAttached)
            return;

        Console.SetOut(exclusive? Instance : new TextWriters(Instance, Console.Out));
    }
}