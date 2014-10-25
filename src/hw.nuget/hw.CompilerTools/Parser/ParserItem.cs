using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class ParserItem<TTreeItem>
        where TTreeItem : class
    {
        public readonly IType<TTreeItem> Type;
        public readonly SourcePart Part;

        public ParserItem(IType<TTreeItem> type, SourcePart part)
        {
            Type = type;
            Part = part;
        }

        public string Name { get { return Type == null ? PrioTable.BeginOfText : Type.PrioTableName; } }
        public TTreeItem Create(TTreeItem left, TTreeItem right, bool isMatch)
        {
            if(Type != null)
                return Type.Create(left, Part, right, isMatch);
            Tracer.Assert(left == null);
            return right;
        }
    }
}