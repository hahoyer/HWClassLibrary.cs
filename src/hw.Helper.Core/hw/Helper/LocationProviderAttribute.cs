using System.Runtime.CompilerServices;
using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Captures the source location of the attribute's textual occurrence using
///     <see cref="CallerFilePathAttribute" /> / <see cref="CallerLineNumberAttribute" />, so a class
///     or method can report where it is declared without hard-coded paths.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
[MeansImplicitUse]
[PublicAPI]
public class LocationProviderAttribute : Attribute, ILocationProvider
{
    /// <summary>Source file and line where the attribute appears.</summary>
    public SourceFilePosition Where;

    /// <param name="fileName"> Filled in by the compiler from <see cref="CallerFilePathAttribute" />. </param>
    /// <param name="lineNumber"> Filled in by the compiler from <see cref="CallerLineNumberAttribute" />. </param>
    public LocationProviderAttribute([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        => Where = new()
        {
            FileName = fileName, LineNumber = lineNumber
        };

    SourceFilePosition ILocationProvider.Where => Where;
}

/// <summary>A (file, line) pair, renderable as an IDE-clickable Visual Studio location.</summary>
[PublicAPI]
public sealed class SourceFilePosition
{
    /// <summary>Source file name, or <c>null</c> if unknown.</summary>
    public string? FileName;

    /// <summary>Line number inside <see cref="FileName" />.</summary>
    public int LineNumber;

    /// <summary>Renders the position in the Visual Studio "Edit.GotoNextLocation" format with <paramref name="tag" /> as the marker.</summary>
    public string ToString(FilePositionTag tag)
        => Tracer.FilePosition(FileName, new() { Start = new() { LineNumber = LineNumber } }, tag);
}

/// <summary>Implemented by anything that knows where it was declared in source.</summary>
[PublicAPI]
public interface ILocationProvider
{
    /// <summary>Source location of the declaration.</summary>
    SourceFilePosition Where { get; }
}