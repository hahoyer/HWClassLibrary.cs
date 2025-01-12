
// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Interface to define token types for parser.
/// </summary>
/// <typeparam name="TParserResult">Tree structure that is returned by the parser</typeparam>
public interface IParserTokenType<TParserResult>
    where TParserResult : class
{
    /// <summary>
    ///     lookup identifier for obtaining priority in priority table
    /// </summary>
    string PrioTableId { get; }

    /// <summary>
    ///     function to create one node of the resulting tree structure.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="token"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    TParserResult? Create(TParserResult? left, IToken token, TParserResult? right);
}

public interface IBracketMatch<TParserResult>
    where TParserResult : class
{
    IParserTokenType<TParserResult> Value { get; }
}