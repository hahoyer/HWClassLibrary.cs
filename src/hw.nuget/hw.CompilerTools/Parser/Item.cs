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
        internal static Item<TTreeItem> Create
            (IType<TTreeItem> type, Token token, BracketContext context)
            => new Item<TTreeItem>(type, token, context);

        internal static Item<TTreeItem> Create
            (Scanner<TTreeItem>.Item other, BracketContext context)
            => new Item<TTreeItem>(other.Type.Type, Token.Create(other.Token, context), context);

        internal static Item<TTreeItem> CreateStart(Source source, PrioTable prioTable, IType<TTreeItem> startType)
            => new Item<TTreeItem>(startType , Token.CreateStart(source), prioTable.BracketContext);

        [EnableDump]
        internal readonly IType<TTreeItem> Type;
        internal readonly BracketContext Context;
        internal readonly Token Token;

        Item(IType<TTreeItem> type, Token token, BracketContext context)
        {
            Type = type;
            Context = context;
            Token = token;
        }

        //context.IsBracketAndLeftBracket(result.Token.Id)
        [EnableDump]
        internal int Depth => Context?.Depth ?? 0;

        string PrioTable.ITargetItem.Token => Type?.PrioTableId ?? PrioTable.BeginOfText;
        BracketContext PrioTable.ITargetItem.LeftContext => Context;

        internal TTreeItem Create(TTreeItem left) => Type.Create(left, Token, null);
    }
}