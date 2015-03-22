using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class TokenClass<TTreeItem>
        : DumpableObject, IIconKeyProvider, IType<TTreeItem>, IUniqueIdProvider
        where TTreeItem : class, ISourcePart
    {
        static int _nextObjectId;

        protected TokenClass()
            : base(_nextObjectId++)
        {
            StopByObjectId(-31);
        }

        string IIconKeyProvider.IconKey { get { return "Symbol"; } }
        TTreeItem IType<TTreeItem>.Create(TTreeItem left, IToken token, TTreeItem right)
        {
            return Create(left, token, right);
        }
        string IType<TTreeItem>.PrioTableId { get { return Id; } }
        ISubParser<TTreeItem> IType<TTreeItem>.NextParser { get { return Next; } }
        IType<TTreeItem> IType<TTreeItem>.NextTypeIfMatched { get { return NextTypeIfMatched; } }

        protected virtual ISubParser<TTreeItem> Next { get { return null; } }
        protected virtual IType<TTreeItem> NextTypeIfMatched { get { return null; } }
        protected abstract TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);
        protected override string GetNodeDump()
        {
            return base.GetNodeDump() + "(" + Id.Quote() + ")";
        }

        public override string ToString() { return base.ToString() + " Id=" + Id.Quote(); }
        string IUniqueIdProvider.Value { get { return Id; } }
        public abstract string Id { get; }
    }

    public interface IUniqueIdProvider
    {
        string Value { get; }
    }
}