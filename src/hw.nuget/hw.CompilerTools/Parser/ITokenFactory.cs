using hw.Scanner;

namespace hw.Parser
{
    /// <summary>
    /// Extended token factory, that is used inside of parser. 
    /// </summary>
    /// <typeparam name="TSourcePart"></typeparam>
    public interface ITokenFactory<TSourcePart> : ITokenFactory
        where TSourcePart : class, ISourcePartProxy
    {
        /// <summary>
        /// Returns the pseudo token to use at the beginning of the sorce part to parse.
        /// </summary>
        IParserTokenType<TSourcePart> BeginOfText { get; }
    }
}