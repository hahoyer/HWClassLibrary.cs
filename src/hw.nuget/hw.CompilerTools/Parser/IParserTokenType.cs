// ReSharper disable CheckNamespace
namespace hw.Parser
{
    /// <summary>
    /// Interface to define token types for parser. 
    /// </summary>
    /// <typeparam name="TSourcePart">Tree structure that is returned by the parser</typeparam>
    public interface IParserTokenType<TSourcePart>
        where TSourcePart : class
    {
        /// <summary>
        /// lookup identifier for obtaining priority in priority table
        /// </summary>
        string PrioTableId { get; }
        /// <summary>
        /// function to create one node of the resulting tree structure.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="token"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        TSourcePart Create(TSourcePart left, IToken token, TSourcePart right);
    }

    public interface IBracketMatch<TSourcePart>
        where TSourcePart : class
    {
        IParserTokenType<TSourcePart> Value { get; }
    }
}