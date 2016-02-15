using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Item<TTreeItem>
        : DumpableObject
            , PrioTable.ITargetItem
        where TTreeItem : class, ISourcePart
    {
        [EnableDump]
        internal readonly IType<TTreeItem> Type;
        internal readonly BracketContext Context;
        internal readonly BracketContext NextContext;
        internal readonly Token Token;

        internal Item
            (IType<TTreeItem> type, Token token, BracketContext context, BracketContext nextContext)
        {
            Type = type;
            Context = context;
            NextContext = nextContext;
            Token = token;
        }

        internal Item
            (Scanner<TTreeItem>.Item other, BracketContext context, BracketContext nextContext)
            : this(other.Type.Type, new Token(other.Token), context, nextContext) { }

        [EnableDump]
        internal int Depth => Context?.Depth ?? 0;

        string PrioTable.ITargetItem.Token => Type?.PrioTableId ?? PrioTable.BeginOfText;
        BracketContext PrioTable.ITargetItem.LeftContext => Context;
        int PrioTable.ITargetItem.NextDepth => NextContext.Depth;

        internal Item<TTreeItem> GetBracketMatch(bool isMatch, OpenItem<TTreeItem> other)
        {
            var newType = Type;
            if(isMatch)
                newType = ((IBracketMatch<TTreeItem>) newType).Value;

            var newToken = new Token(null, Token.SourcePart.End.Span(0));

            return new Item<TTreeItem>
                (newType, newToken, other.BracketItem.LeftContext, NextContext);
        }
    }
}