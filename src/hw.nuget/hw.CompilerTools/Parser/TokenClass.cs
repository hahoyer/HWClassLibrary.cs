using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class TokenClass<TTreeItem>
        : DumpableObject,
            IParserTokenType<TTreeItem>,
            IUniqueIdProvider
        where TTreeItem : class, ISourcePart
    {
        static int _nextObjectId;

        protected TokenClass()
            : base(_nextObjectId++) { StopByObjectIds(-31); }

        string IParserTokenType<TTreeItem>.PrioTableId => Id;

        TTreeItem IParserTokenType<TTreeItem>.Create(TTreeItem left, IToken token, TTreeItem right)
            => Create(left, token, right);

        protected abstract TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + Id.Quote() + ")";
        public override string ToString() => base.ToString() + " Id=" + Id.Quote();
        string IUniqueIdProvider.Value => Id;
        public abstract string Id { get; }
    }
}