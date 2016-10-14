using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class TokenClass
        : DumpableObject,
            IParserTypeProvider,
            IScannerType
    {
        static int NextObjectId;

        protected TokenClass()
            : base(NextObjectId++) {}

        IParserType<TTreeItem> IParserTypeProvider.GetType<TTreeItem>(string id)
        {
            NotImplementedMethod(id);
            return null;
        }
    }

    public abstract class TokenClass<TTreeItem>
        : DumpableObject,
            IParserType<TTreeItem>,
            IUniqueIdProvider,
            PrioParser<TTreeItem>.ISubParserProvider
        where TTreeItem : class, ISourcePart
    {
        static int _nextObjectId;

        protected TokenClass()
            : base(_nextObjectId++)
        {
            StopByObjectIds(-31);
        }

        string IParserType<TTreeItem>.PrioTableId => Id;

        TTreeItem IParserType<TTreeItem>.Create(TTreeItem left, IToken token, TTreeItem right)
            => Create(left, token, right);

        ISubParser<TTreeItem> PrioParser<TTreeItem>.ISubParserProvider.NextParser => Next;

        protected virtual ISubParser<TTreeItem> Next => null;
        protected abstract TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + Id.Quote() + ")";

        public override string ToString() => base.ToString() + " Id=" + Id.Quote();

        string IUniqueIdProvider.Value => Id;
        public abstract string Id { get; }
    }

    public interface IUniqueIdProvider
    {
        string Value { get; }
    }
}