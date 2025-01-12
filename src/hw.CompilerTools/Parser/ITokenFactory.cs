using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Extended token factory, that is used inside of parser.
/// </summary>
/// <typeparam name="TParserResult"></typeparam>
public interface ITokenFactory<TParserResult> : ITokenFactory
    where TParserResult : class
{
    /// <summary>
    ///     Returns the pseudo token to use at the beginning of the sorce part to parse.
    /// </summary>
    IParserTokenType<TParserResult>? BeginOfText { get; }
}