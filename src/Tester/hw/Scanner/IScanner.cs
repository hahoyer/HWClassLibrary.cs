using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Scanner
{
    /// <summary>
    ///     Scanner interface that is used by <see cref="PrioParser{TSourcePart}" /> to split source into tokens.
    /// </summary>
    public interface IScanner
    {
        /// <summary>
        ///     Get the next group of tokens, that belongs together, like the actual token and leading or trailing whitespaces.
        /// </summary>
        /// <param name="sourcePosition">
        ///     The position in the source, where to start. The position is advanced to the end of the token
        ///     group.
        /// </param>
        /// <returns>A list of tokens that are taken from source position given.</returns>
        IItem[] GetNextTokenGroup(SourcePosition sourcePosition);
    }

    public interface IItem
    {
        IScannerTokenType ScannerTokenType { get; }
        SourcePart SourcePart { get; }
    }

    public interface IScannerTokenType
    {
        IParserTokenFactory ParserTokenFactory { get; }
        string Id { get; }
    }

    public interface IParserTokenFactory
    {
        IParserTokenType<TSourcePart> GetTokenType<TSourcePart>(string id)
            where TSourcePart : class;
    }
}