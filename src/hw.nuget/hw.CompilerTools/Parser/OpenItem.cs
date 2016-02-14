using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem> : DumpableObject
        where TTreeItem : class, ISourcePart
    {
        internal readonly TTreeItem Left;
        internal readonly Item<TTreeItem> Current;

        internal OpenItem(TTreeItem left, Item<TTreeItem> current)
        {
            Left = left;
            Current = current;
        }

        internal IType<TTreeItem> Type { get { return Current.Type; } }
        internal PrioTable.ITargetItem Item { get { return Current; } }

        internal TTreeItem Create(TTreeItem right)
        {
            if(Current.Type != null)
                return Current.Create(Left, right);
            Tracer.Assert(Left == null);
            return right;
        }

        internal static OpenItem<TTreeItem> StartItem(ScannerToken startItem, BracketContext context, IType<TTreeItem> startType, BracketContext nextContext)
        {
            return StartItem(new Item<TTreeItem>(startType, startItem, context, nextContext));
        }

        static OpenItem<TTreeItem> StartItem(Item<TTreeItem> current)
        {
            return new OpenItem<TTreeItem>(default(TTreeItem), current);
        }

        protected override string GetNodeDump()
        {
            return Tracer.Dump(Left) + " " + Current.Type.GetType().PrettyName();
        }
    }
}