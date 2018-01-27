using System;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class ParserTokenType<TTreeItem>
        : ScannerTokenType<TTreeItem>,
            IUniqueIdProvider,
            IParserTokenType<TTreeItem>
        where TTreeItem : class, ISourcePartProxy
    {
        TTreeItem IParserTokenType<TTreeItem>.Create(TTreeItem left, IToken token, TTreeItem right) 
            => Create(left, token, right);

        string IParserTokenType<TTreeItem>.PrioTableId => Id;

        string IUniqueIdProvider.Value => Id;

        public abstract string Id {get;}
        protected abstract TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);

        protected override IParserTokenType<TTreeItem> GetParserTokenType(string id) => this;

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + Id.Quote() + ")";
        public override string ToString() => base.ToString() + " Id=" + Id.Quote();
    }
}