using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem> : DumpableObject
        where TTreeItem : class
    {
        internal readonly TTreeItem Left;
        internal readonly IType<TTreeItem> Type;
        readonly SourcePart _part;

        internal OpenItem(TTreeItem left, IType<TTreeItem> type, SourcePart part)
        {
            Left = left;
            Type = type;
            _part = part;
        }

        internal TTreeItem Create(TTreeItem right)
        {
            if(Type != null)
                return Type.Create(Left, _part, right);
            Tracer.Assert(Left == null);
            return right;
        }

        internal static OpenItem<TTreeItem> StartItem(SourcePart sourcePart)
        {
            return StartItem(null, sourcePart);
        }

        static OpenItem<TTreeItem> StartItem(IType<TTreeItem> type, SourcePart partOfStartItem)
        {
            return new OpenItem<TTreeItem>(default(TTreeItem), type, partOfStartItem);
        }

        protected override string GetNodeDump() { return Tracer.Dump(Left) + " " + Type.GetType().PrettyName(); }
    }
}