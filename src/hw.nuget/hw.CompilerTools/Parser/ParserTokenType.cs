using hw.Helper;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    public abstract class ParserTokenType<TTreeItem>
        : ScannerTokenType<TTreeItem>
            , IUniqueIdProvider
            , IParserTokenType<TTreeItem>
        where TTreeItem : class
    {
        public abstract string Id { get; }

        TTreeItem IParserTokenType<TTreeItem>.Create(TTreeItem left, IToken token, TTreeItem right)
            => Create(left, token, right);

        string IParserTokenType<TTreeItem>.PrioTableId => Id;

        string IUniqueIdProvider.Value => Id;

        public override string ToString() => base.ToString() + " Id=" + Id.Quote();
        protected abstract TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);

        protected override IParserTokenType<TTreeItem> GetParserTokenType(string id) => this;
    }
}