using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace hw.PrioParser
{
    public sealed class Item<TTreeItem, TPart>
        where TTreeItem : class
    {
        public readonly IType<TTreeItem, TPart> Type;
        public readonly TPart Part;

        public Item(IType<TTreeItem, TPart> type, TPart part)
        {
            Type = type;
            Part = part;
        }

        public string Name { get { return Type == null ? PrioTable.BeginOfText : Type.PrioTableName; } }
        public bool IsEnd { get { return Type != null && Type.IsEnd; } }
        public TTreeItem Create(TTreeItem left, TTreeItem right, bool isMatch)
        {
            if(Type != null)
                return Type.Create(left, Part, right, isMatch);
            Tracer.Assert(left == null);
            return right;
        }
    }
}