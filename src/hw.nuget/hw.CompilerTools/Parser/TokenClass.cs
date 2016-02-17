using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class TokenClass<TTreeItem>
        : DumpableObject, IIconKeyProvider, IType<TTreeItem>, IUniqueIdProvider,
            Scanner<TTreeItem>.IType
        where TTreeItem : class, ISourcePart
    {
        static int _nextObjectId;

        protected TokenClass()
            : base(_nextObjectId++)
        {
            StopByObjectIds(-31);
        }

        string IIconKeyProvider.IconKey => "Symbol";

        string IType<TTreeItem>.PrioTableId => Id;

        TTreeItem IType<TTreeItem>.Create(TTreeItem left, IToken token, TTreeItem right)
            => Create(left, token, right);

        ISubParser<TTreeItem> Scanner<TTreeItem>.IType.NextParser => Next;

        IType<TTreeItem> Scanner<TTreeItem>.IType.Type => this;

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