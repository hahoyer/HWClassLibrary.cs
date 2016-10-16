using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem> : DumpableObject
        where TTreeItem : class, ISourcePart
    {
        internal readonly IToken Token;
        internal readonly TTreeItem Left;
        internal readonly IParserTokenType<TTreeItem> Type;
        internal readonly PrioTable.ITargetItem BracketItem;

        internal OpenItem(TTreeItem left, Item<TTreeItem> current)
        {
            Left = left;
            Type = current.Type;
            Token = current;
            BracketItem = current;
        }

        internal int NextDepth => BracketItem.GetRightDepth();

        internal TTreeItem Create(TTreeItem right)
        {
            if(Type != null)
                return Type.Create(Left, Token, right);
            Tracer.Assert(Left == null);
            return right;
        }

        protected override string GetNodeDump()
            => Tracer.Dump(Left) + " " + Type.GetType().PrettyName();
    }
}