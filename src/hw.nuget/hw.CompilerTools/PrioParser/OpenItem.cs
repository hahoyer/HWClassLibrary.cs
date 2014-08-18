using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public sealed class OpenItem<T>
        where T : class
    {
        internal readonly T Left;
        internal readonly Item<T> Item;
        readonly bool _isMatch;

        internal OpenItem(T left, Item<T> item, bool isMatch)
        {
            Left = left;
            Item = item;
            _isMatch = isMatch;
        }

        internal char Relation(string newTokenName, PrioTable prioTable) { return prioTable.Relation(newTokenName, Item.Name); }
        internal T Create(T args) { return Item.Create(Left, args, _isMatch); }

        internal static OpenItem<T> StartItem(IPosition<T> start) { return new OpenItem<T>(default(T), new Item<T>(null, start.Span(start)),false); }
    }
}