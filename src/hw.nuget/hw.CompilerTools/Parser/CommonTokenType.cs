using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class CommonTokenType<TTreeItem> : ScannerTokenType<TTreeItem>,
            IUniqueIdProvider,
            IParserTokenType<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        protected abstract TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);

        TTreeItem IParserTokenType<TTreeItem>.Create
            (TTreeItem left, IToken token, TTreeItem right) => Create(left, token, right);

        string IParserTokenType<TTreeItem>.PrioTableId => Id;

        string IUniqueIdProvider.Value => Id;

        public abstract string Id { get; }

        protected override IParserTokenType<TTreeItem> GetParserTokenType(string id) => this;
    }
}