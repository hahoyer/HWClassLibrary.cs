using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    [PublicAPI]
    public sealed class Item<TTreeItem>
        : DumpableObject
            , PrioTable.ITargetItem
            , IToken
        where TTreeItem : class
    {
        [EnableDump]
        public readonly BracketContext Context;

        public readonly bool? IsBracketAndLeftBracket;
        public readonly IItem[] PrefixItems;
        public readonly IParserTokenType<TTreeItem> Type;
        internal readonly SourcePart Characters;

        Item
        (
            IEnumerable<IItem> prefixItems,
            IParserTokenType<TTreeItem> type,
            SourcePart characters,
            BracketContext context,
            bool? isBracketAndLeftBracket
        )
        {
            PrefixItems = prefixItems.ToArray();
            Type = type;
            Characters = characters;
            Context = context;
            IsBracketAndLeftBracket = isBracketAndLeftBracket;
        }

        [EnableDump]
        public int Depth => Context?.Depth ?? 0;

        BracketContext PrioTable.ITargetItem.LeftContext => Context;

        string PrioTable.ITargetItem.Token => Type?.PrioTableId ?? PrioTable.BeginOfText;
        SourcePart IToken.Characters => Characters;
        bool? IToken.IsBracketAndLeftBracket => IsBracketAndLeftBracket;

        IEnumerable<IItem> IToken.PrecededWith => PrefixItems;

        public static Item<TTreeItem> Create
        (
            IEnumerable<IItem> prefixItems,
            IParserTokenType<TTreeItem> parserType,
            SourcePart characters,
            BracketContext context,
            bool? isBracketAndLeftBracket
        )
            =>
                new Item<TTreeItem>
                (
                    prefixItems,
                    parserType,
                    characters,
                    context,
                    isBracketAndLeftBracket);

        public static Item<TTreeItem> Create(IItem[] items, BracketContext context)
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

        public static Item<TTreeItem> CreateStart
        (
            Source source,
            PrioTable prioTable,
            IParserTokenType<TTreeItem> startParserType
        )
            =>
                new Item<TTreeItem>
                (
                    new IItem[0],
                    startParserType,
                    (source + 0).Span(0),
                    prioTable.BracketContext,
                    null);

        public Item<TTreeItem> RecreateWith
        (
            IEnumerable<IItem> newPrefixItems = null,
            IParserTokenType<TTreeItem> newType = null,
            BracketContext newContext = null
        )
            => new Item<TTreeItem>
            (
                newPrefixItems ?? PrefixItems,
                newType ?? Type,
                Characters,
                newContext ?? Context,
                IsBracketAndLeftBracket);

        public TTreeItem Create(TTreeItem left) => Type.Create(left, this, null);
    }
}