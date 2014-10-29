using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem> : DumpableObject
        where TTreeItem : class
    {
        internal readonly TTreeItem Left;
        internal readonly ParserItem<TTreeItem> Item;

        internal OpenItem(TTreeItem left, ParserItem<TTreeItem> item)
        {
            Left = left;
            Item = item;
        }

        internal char Relation(string newTokenName, PrioTable prioTable) { return prioTable.Relation(newTokenName, Item.Name); }
        internal TTreeItem Create(TTreeItem right) { return Item.Create(Left, right); }

        internal static OpenItem<TTreeItem> StartItem(SourcePosn sourcePosn)
        {
            return StartItem(null, SourcePart.Span(sourcePosn, sourcePosn));
        }
        internal static OpenItem<TTreeItem> StartItem(IType<TTreeItem> type, SourcePart part)
        {
            return new OpenItem<TTreeItem>(default(TTreeItem), new ParserItem<TTreeItem>(type, part));
        }
        
        protected override string GetNodeDump() { return Tracer.Dump(Left) + " " + Item.NodeDump; }
    }
}