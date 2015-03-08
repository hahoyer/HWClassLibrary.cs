using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem> : DumpableObject
        where TTreeItem : class
    {
        internal readonly TTreeItem Left;
        internal readonly IType<TTreeItem> Type;
        readonly Token _token;

        internal OpenItem(TTreeItem left, IType<TTreeItem> type, Token token)
        {
            Left = left;
            Type = type;
            _token = token;
        }

        internal TTreeItem Create(TTreeItem right)
        {
            if(Type != null)
                return Type.Create(Left, _token, right);
            Tracer.Assert(Left == null);
            return right;
        }

        internal static OpenItem<TTreeItem> StartItem(Token startItem)
        {
            return StartItem(null, startItem);
        }

        static OpenItem<TTreeItem> StartItem(IType<TTreeItem> type, Token partOfStartItem)
        {
            return new OpenItem<TTreeItem>(default(TTreeItem), type, partOfStartItem);
        }

        protected override string GetNodeDump()
        {
            return Tracer.Dump(Left) + " " + Type.GetType().PrettyName();
        }
    }
}