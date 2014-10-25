using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem>
        where TTreeItem : class
    {
        internal readonly TTreeItem Left;
        internal readonly ParserItem<TTreeItem> Item;
        readonly bool _isMatch;

        internal OpenItem(TTreeItem left, ParserItem<TTreeItem> item, bool isMatch)
        {
            Left = left;
            Item = item;
            _isMatch = isMatch;
        }

        internal char Relation(string newTokenName, PrioTable prioTable) { return prioTable.Relation(newTokenName, Item.Name); }
        internal TTreeItem Create(TTreeItem args) { return Item.Create(Left, args, _isMatch); }

        internal static OpenItem<TTreeItem> StartItem(SourcePosn sourcePosn) { return StartItem(null, SourcePart.Span(sourcePosn, sourcePosn)); }
        internal static OpenItem<TTreeItem> StartItem(IType<TTreeItem> type, SourcePart part)
        {
            return new OpenItem<TTreeItem>(default(TTreeItem), new ParserItem<TTreeItem>(type, part), false);
        }
    }
}