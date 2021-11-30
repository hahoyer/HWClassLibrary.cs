using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Parser
{
    [PublicAPI]
    public sealed class Item<TSourcePart>
        : DumpableObject
            , PrioTable.ITargetItem
            , IToken
            , ILinked<TSourcePart>
        where TSourcePart : class
    {
        [EnableDump]
        public readonly BracketContext Context;

        public readonly bool? IsBracketAndLeftBracket;
        public readonly IItem[] PrefixItems;
        public readonly IParserTokenType<TSourcePart> Type;
        internal readonly SourcePart Characters;

        TSourcePart Container;

        Item
        (
            IEnumerable<IItem> prefixItems,
            IParserTokenType<TSourcePart> type,
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

        TSourcePart ILinked<TSourcePart>.Container
        {
            get => Container;
            set
            {
                if(Container != null)
                    (value == Container).Assert();

                Container = value;
            }
        }

        BracketContext PrioTable.ITargetItem.LeftContext => Context;

        string PrioTable.ITargetItem.Token => Type?.PrioTableId ?? PrioTable.BeginOfText;
        SourcePart IToken.Characters => Characters;
        bool? IToken.IsBracketAndLeftBracket => IsBracketAndLeftBracket;

        IEnumerable<IItem> IToken.PrecededWith => PrefixItems;

        [EnableDump]
        public int Depth => Context?.Depth ?? 0;

        public static Item<TSourcePart> Create(IItem[] items, BracketContext context)
        {
            var prefixItems = items.Take(items.Length - 1);
            var mainItem = items.Last();
            var isBracketAndLeftBracket = context.IsLeftBracket(mainItem.SourcePart.Id);

            var parserType = mainItem
                .ScannerTokenType
                .ParserTokenFactory
                .GetTokenType<TSourcePart>(mainItem.SourcePart.Id);

            return new
            (
                prefixItems,
                parserType,
                mainItem.SourcePart,
                context,
                isBracketAndLeftBracket
            );
        }

        public static Item<TSourcePart> CreateStart
        (
            Source source,
            PrioTable prioTable,
            IParserTokenType<TSourcePart> startParserType
        )
            => new
            (
                new IItem[0],
                startParserType,
                (source + 0).Span(0),
                prioTable.BracketContext,
                null
            );

        public Item<TSourcePart> RecreateWith
        (
            IEnumerable<IItem> newPrefixItems = null,
            IParserTokenType<TSourcePart> newType = null,
            BracketContext newContext = null
        )
            => new
            (
                newPrefixItems ?? PrefixItems,
                newType ?? Type,
                Characters,
                newContext ?? Context,
                IsBracketAndLeftBracket
            );

        public TSourcePart Create(TSourcePart left) => Type.Create(left, this, null);

        public Item<TSourcePart> CreateMatch(OpenItem<TSourcePart> other)
            => new
            (
                new IItem[0],
                ((IBracketMatch<TSourcePart>)Type).Value,
                Characters.End.Span(0),
                other.BracketItem.LeftContext,
                this.GetRightContext().IsLeftBracket("")
            );
    }
}