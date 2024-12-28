using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Parser;

public enum BracketSide {Left, None, Right}


/// <summary>
///     Complete token obtained by the scanner, including preceding whitespaces
/// </summary>
public interface IToken
{
    /// <summary>
    ///     Whitespace items that precede this token.
    /// </summary>
    [DisableDump]
    int PrecededWith { get; }

    /// <summary>
    ///     Characters the form the actual token.
    /// </summary>
    [DisableDump]
    SourcePart Characters { get; }

    /// <summary>
    ///     Will be null if it is not bracket, true for left bracket and false for right bracket
    /// </summary>
    [DisableDump]
    BracketSide BracketSide { get; }
}

[PublicAPI]
public static class TokenExtension
{
    public static SourcePart GetSourcePart(this IToken token)
        => (token.Characters.Start - token.PrecededWith).Span(token.Characters.End);

    internal static SourcePart GetPrefixSourcePart(this IToken token)
        => token.Characters.Start.Span(-token.PrecededWith);
}