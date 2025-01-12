using hw.Parser;

// ReSharper disable CheckNamespace

namespace hw.Scanner;

/// <summary>
///     Scanner interface that is used by <see cref="PrioParser{TParserResult}" /> to split source into tokens.
/// </summary>
public interface IScanner
{
    /// <summary>
    ///     Get the next group of tokens, that belongs together,
    ///     like the actual token and leading whitespaces.
    /// </summary>
    /// <param name="sourcePosition">
    ///     The position in the source, where to start. The position is advanced to the end of the token
    ///     group.
    /// </param>
    /// <returns>
    ///     A list of tokens that are taken from source position given. All items except the last (or only) items are
    ///     whitespaces.
    /// </returns>
    IItem[] GetNextTokenGroup(SourcePosition sourcePosition);
}

/// <summary>
///     Tokens that are returned by <see cref="IScanner" />.
/// </summary>
public interface IItem
{
    /// <summary>
    ///     General classification of the item returned
    /// </summary>
    IScannerTokenType ScannerTokenType { get; }

    /// <summary>
    ///     Source position and length of the item.
    /// </summary>
    int Length { get; }
}

/// <summary>
///     There are two general cases: <see cref="WhiteSpaceTokenType" /> and <see cref="hw.Parser.ScannerTokenType" />
/// </summary>
public interface IScannerTokenType
{
    /// <summary>
    ///     Use this to link to the <see cref="IParserTokenFactory" /> if desired.
    ///     If null is returned, this token type is categorised as a blank character (whitespace or comment) with no further meaning.
    /// </summary>
    IParserTokenFactory? ParserTokenFactory { get; }

    /// <summary>
    ///     Identifier used for debugging purposes
    /// </summary>
    string Id { get; }
}

/// <summary>
///     Interface to get the <see cref="IParserTokenType{TParserResult}" />
/// </summary>
public interface IParserTokenFactory
{
    /// <summary>
    ///     Create the parser token type from characters found by the scanner (stripped by whitespaces).
    /// </summary>
    /// <typeparam name="TParserResult"></typeparam>
    /// <param name="id">characters without whitespaces</param>
    /// <returns></returns>
    IParserTokenType<TParserResult> GetTokenType<TParserResult>(string id)
        where TParserResult : class;
}