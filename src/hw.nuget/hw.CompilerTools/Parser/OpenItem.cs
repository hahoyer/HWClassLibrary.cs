using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem> : DumpableObject
        where TTreeItem : class, ISourcePart
    {
        internal readonly TTreeItem Left;
        readonly Item<TTreeItem> _current;

        internal OpenItem(TTreeItem left, Item<TTreeItem> current)
        {
            Left = left;
            _current = current;
        }

        internal IType<TTreeItem> Type { get { return _current.Type; } }

        internal TTreeItem Create(TTreeItem right)
        {
            if(_current.Type != null)
                return _current.Create(Left, right);
            Tracer.Assert(Left == null);
            return right;
        }

        internal static OpenItem<TTreeItem> StartItem(ScannerToken startItem)
        {
            return StartItem(new Item<TTreeItem>(null, startItem));
        }

        static OpenItem<TTreeItem> StartItem(Item<TTreeItem> current)
        {
            return new OpenItem<TTreeItem>(default(TTreeItem), current);
        }

        protected override string GetNodeDump()
        {
            return Tracer.Dump(Left) + " " + _current.Type.GetType().PrettyName();
        }
    }
}