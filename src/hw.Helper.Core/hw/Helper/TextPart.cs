// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Half-open span of text identified by line/column coordinates. <see cref="End" /> is the
///     position just after the last included character; either side may be <c>null</c> to denote
///     "from the start" or "to the end".
/// </summary>
[PublicAPI]
public class TextPart
{
    /// <summary>Inclusive start position, or <c>null</c> for "beginning of file".</summary>
    public TextPosition? Start;

    /// <summary>Exclusive end position, or <c>null</c> for "end of file".</summary>
    public TextPosition? End;
}

/// <summary>Zero-based line and column inside a source file.</summary>
[PublicAPI]
public sealed class TextPosition
{
    /// <summary>Zero-based line number.</summary>
    public int LineNumber;

    /// <summary>Zero-based column number on <see cref="LineNumber" />.</summary>
    public int ColumnNumber;
}