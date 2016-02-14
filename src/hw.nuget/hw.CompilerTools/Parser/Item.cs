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

        internal Item<TTreeItem> GetMatch(PrioTable.ITargetItem left) 
        {
            var matchType = (Type as IBracketMatch<TTreeItem>)?.GetValue(left.Token);
            if(matchType == null)
                return new Item<TTreeItem>(Type, Token, left.LeftContext, NextContext);

            var token = new Token(null, Token.SourcePart.End.Span(0));
            return new Item<TTreeItem>(matchType, token, left.LeftContext, NextContext);
        }
    }
    
}