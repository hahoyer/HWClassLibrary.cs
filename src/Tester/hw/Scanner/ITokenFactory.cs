// ReSharper disable CheckNamespace

namespace hw.Scanner;

/// <summary>
///     Factory that can be used for instance in <see cref="TwoLayerScanner" /> to obtain tokens.
/// </summary>
public interface ITokenFactory
{
    /// <summary>
    ///     Returns the token type when reaching the end source.
    /// </summary>
    IScannerTokenType EndOfText { get; }

    /// <summary>
    ///     Returns the token type when no class can be matched.
    /// </summary>
    IScannerTokenType InvalidCharacterError { get; }

    /// <summary>
    ///     Returns the possible token classes, each class is a pair of a token class and a match function.
    /// </summary>
    LexerItem[] Classes { get; }
}