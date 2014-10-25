using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class TokenClass<TTreeItem> : DumpableObject, IIconKeyProvider, IType<TTreeItem>, INameProvider
        where TTreeItem : class
    {
        static int _nextObjectId;
        string _name;

        protected TokenClass()
            : base(_nextObjectId++) { StopByObjectId(-31); }

        string IIconKeyProvider.IconKey { get { return "Symbol"; } }

        string INameProvider.Name { set { Name = value; } }

        TTreeItem IType<TTreeItem>.Create(TTreeItem left, SourcePart part, TTreeItem right, bool isMatch)
        {
            if(isMatch && AcceptsMatch || !isMatch && AcceptsMismatch)
                return Create(left, part, right);
            return Mismatch(left, part, right);
        }

        string IType<TTreeItem>.PrioTableName { get { return Name; } }
        ISubParser<TTreeItem> IType<TTreeItem>.Next { get { return Next; } }

        protected virtual ISubParser<TTreeItem> Next { get { return null; } }

        protected virtual TTreeItem Mismatch(TTreeItem left, SourcePart part, TTreeItem right)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }

        protected virtual bool AcceptsMismatch { get { return !AcceptsMatch; } }
        protected virtual bool AcceptsMatch { get { return false; } }
        protected abstract TTreeItem Create(TTreeItem left, SourcePart part, TTreeItem right);

        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + Name.Quote() + ")"; }

        [Node]
        [DisableDump]
        internal string Name { get { return _name; } set { _name = value; } }

        public override string ToString() { return base.ToString() + " Name=" + _name.Quote(); }
    }
}