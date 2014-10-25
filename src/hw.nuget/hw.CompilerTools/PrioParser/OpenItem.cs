using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public sealed class OpenItem<TTreeItem, TPart>
        where TTreeItem : class
    {
        internal readonly TTreeItem Left;
        internal readonly Item<TTreeItem, TPart> Item;
        readonly bool _isMatch;

        internal OpenItem(TTreeItem left, Item<TTreeItem, TPart> item, bool isMatch)
        {
            Left = left;
            Item = item;
            _isMatch = isMatch;
        }

        internal char Relation(string newTokenName, PrioTable prioTable) { return prioTable.Relation(newTokenName, Item.Name); }
        internal TTreeItem Create(TTreeItem args) { return Item.Create(Left, args, _isMatch); }

        internal static OpenItem<TTreeItem, TPart> StartItem(IPosition<TTreeItem, TPart> start) { return StartItem(null, start.Span(start)); }
        internal static OpenItem<TTreeItem, TPart> StartItem(IType<TTreeItem, TPart> type, TPart part)
        {
            return new OpenItem<TTreeItem, TPart>(default(TTreeItem), new Item<TTreeItem, TPart>(type, part), false);
        }
    }
}