using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Item<TTreeItem> : DumpableObject, PrioTable.ITargetItem
        where TTreeItem : class, ISourcePart
    {
        [EnableDump]
        internal readonly IType<TTreeItem> Type;
        internal readonly ScannerToken Token;
        internal readonly BracketContext Context;
        internal readonly BracketContext NextContext;
        Item<TTreeItem> _match;

        internal Item
            (
            IType<TTreeItem> type,
            ScannerToken token,
            BracketContext context,
            BracketContext nextContext)
        {
            Type = type;
            Token = token;
            Context = context;
            NextContext = nextContext;
        }

        [EnableDump]
        internal int Depth => Context?.Depth ?? 0;

        internal Item
            (Scanner<TTreeItem>.Item other, BracketContext context, BracketContext nextContext)
            : this(other.Type.Type, other.Token, context, nextContext) {}

        internal TTreeItem Create(TTreeItem left, TTreeItem right)
        {
            return Type.Create(left, new Token(Token.PrecededWith, Token.Characters), right);
        }

        BracketContext PrioTable.ITargetItem.Context => Context;
        string PrioTable.ITargetItem.Token => Type?.PrioTableId ?? PrioTable.BeginOfText;
        [DisableDump]
        internal Item<TTreeItem> Match => _match ?? (_match = CreateMatch());

        Item<TTreeItem> CreateMatch()
        {
            var matchType = ((IBracketMatch<TTreeItem>) Type).Value;
            // Since it is a match, Context changes to NextContext
            return new Item<TTreeItem>(matchType, Token, NextContext, NextContext);
        }
    }
}