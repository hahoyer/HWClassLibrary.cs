using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace hw.PrioParser
{
    public sealed class RawItem
    {
        public readonly IType Type;
        public readonly IPart Part;

        public RawItem(IType type, IPart part)
        {
            Type = type;
            Part = part;
        }
    }

    public sealed class Item<T>
        where T : class
    {
        public readonly IType<T> Type;
        public readonly IPart Part;

        public Item(IType<T> type, IPart part)
        {
            Type = type;
            Part = part;
        }

        public string Name { get { return Type == null ? PrioTable.BeginOfText : Type.PrioTableName; } }
        public bool IsEnd { get { return Type != null && Type.IsEnd; } }
        public T Create(T left, T right, bool isMatch)
        {
            if(Type != null)
                return Type.Create(left, Part, right, isMatch);
            Tracer.Assert(left == null);
            return right;
        }
    }
}