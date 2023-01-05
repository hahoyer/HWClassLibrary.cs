using System;
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
    public int ColumnNumber1;

    [Obsolete("Use ColumnNumber1 since it is actually ColumnNUmber+1", true)]
    public int ColumnNumber
    {
        get => ColumnNumber1 - 1;
        set => ColumnNumber1 = value + 1;
    }
}