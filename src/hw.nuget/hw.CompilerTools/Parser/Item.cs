using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class Item<TTreeItem> : DumpableObject, PrioTable.ITargetItem, IToken
        where TTreeItem : class, ISourcePartProxy
    {
        internal static Item<TTreeItem> Create
            (
                IEnumerable<IItem> prefixItems,
                IParserTokenType<TTreeItem> parserType,
                SourcePart characters,
                BracketContext context,
                bool? isBracketAndLeftBracket)
            =>
            new Item<TTreeItem>
            (
                prefixItems,
                parserType,
                characters,
                context,
                isBracketAndLeftBracket);

        internal static Item<TTreeItem> Create(IItem[] items, BracketContext context)
        {
            var prefixItems = items.Take(items.Length - 1);
            var mainItem = items.Last();
            var isBracketAndLeftBracket = context.IsBracketAndLeftBracket(mainItem.SourcePart.Id);

            var parserType = mainItem
                .ScannerTokenType
                .ParserTokenFactory
                .GetTokenType<TTreeItem>(mainItem.SourcePart.Id);

            return new Item<TTreeItem>
            (
                prefixItems,
                parserType,
                mainItem.SourcePart,
                context,
                isBracketAndLeftBracket);
        }

        internal static Item<TTreeItem> CreateStart
            (
                Source source,
                PrioTable prioTable,
                IParserTokenType<TTreeItem> startParserType)
            =>
            new Item<TTreeItem>
            (
                new IItem[0],
                startParserType,
                (source + 0).Span(0),
                prioTable.BracketContext,
                null);

        [EnableDump]
        internal readonly BracketContext Context;
        internal readonly IItem[] PrefixItems;
        internal readonly IParserTokenType<TTreeItem> Type;
        internal readonly SourcePart Characters;
        internal readonly bool? IsBracketAndLeftBracket;

        Item
        (
            IEnumerable<IItem> prefixItems,
            IParserTokenType<TTreeItem> type,
            SourcePart characters,
            BracketContext context,
            bool? isBracketAndLeftBracket)
        {
            PrefixItems = prefixItems.ToArray();
            Type = type;
            Characters = characters;
            Context = context;
            IsBracketAndLeftBracket = isBracketAndLeftBracket;
        }

        [EnableDump]
        internal int Depth => Context?.Depth ?? 0;

        internal Item<TTreeItem> RecreateWith
            (
                IEnumerable<IItem> newPrefixItems = null,
                IParserTokenType<TTreeItem> newType = null,
                BracketContext newContext = null)
            => new Item<TTreeItem>
            (
                newPrefixItems ?? PrefixItems,
                newType ?? Type,
                Characters,
                newContext ?? Context,
                IsBracketAndLeftBracket);

        string PrioTable.ITargetItem.Token => Type?.PrioTableId ?? PrioTable.BeginOfText;
        BracketContext PrioTable.ITargetItem.LeftContext => Context;

        internal TTreeItem Create(TTreeItem left) => Type.Create(left, this, null);

        IEnumerable<IItem> IToken.PrecededWith => PrefixItems;
        SourcePart IToken.Characters => Characters;
        bool? IToken.IsBracketAndLeftBracket => IsBracketAndLeftBracket;
    }
}