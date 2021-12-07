using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Parser
{
    /// <summary>
    /// Used as base for all token types. 
    /// </summary>
    /// <typeparam name="TSourcePart">Tree structure that is returned by the parser</typeparam>
    public abstract class ParserTokenType<TSourcePart>
        : ScannerTokenType<TSourcePart>
            , IUniqueIdProvider
            , IParserTokenType<TSourcePart>
        where TSourcePart : class
    {
        TSourcePart IParserTokenType<TSourcePart>.Create(TSourcePart left, IToken token, TSourcePart right)
        {
            var result = Create(left, token, right);
            if(token is ILinked<TSourcePart> treeLinkedToken)
                treeLinkedToken.Container = result;
            return result;
        }

        string IParserTokenType<TSourcePart>.PrioTableId => Id;

        string IUniqueIdProvider.Value => Id;
        public abstract string Id { get; }
        protected abstract TSourcePart Create(TSourcePart left, IToken token, TSourcePart right);

        public override string ToString() => base.ToString() + " Id=" + Id.Quote();

        protected override IParserTokenType<TSourcePart> GetParserTokenType(string id) => this;
    }

    interface ILinked<TSourcePart>
    {
        TSourcePart Container { get; set; }
    }
}