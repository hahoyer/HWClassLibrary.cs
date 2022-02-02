using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[PublicAPI]
public class TextPart
{
    public TextPosition Start;
    public TextPosition End;
}

[PublicAPI]
public sealed class TextPosition
{
    public int LineNumber;
    public int ColumnNumber;
}