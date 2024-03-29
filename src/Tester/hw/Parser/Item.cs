using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Parser;

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

    Item(SourcePosition sourcePosition, BracketContext context)
    {
        PrefixItems = new IItem[0];
        Characters = sourcePosition.Span(0);
        Context = context;
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
    int IToken.PrecededWith => PrefixItems.Sum(item => item.Length);

    [EnableDump]
    public int Depth => Context?.Depth ?? 0;

    public static Item<TSourcePart> Create(IItem[] items, SourcePosition end, BracketContext context, bool isSubParser)
    {
        var prefixItems = items.Take(items.Length - 1);
        var mainItem = items.Last();
        var token = end.Span(-mainItem.Length);
        var isBracketAndLeftBracket = context.IsLeftBracket(token.Id);

        var mainItemScannerTokenType = mainItem.ScannerTokenType;
        var parserTokenFactory = mainItemScannerTokenType
            .ParserTokenFactory;
        var parserType = parserTokenFactory
            .GetTokenType<TSourcePart>(token.Id);

        return new
        (
            prefixItems,
            parserType,
            token,
            context,
            isBracketAndLeftBracket
        );
    }

    public static Item<TSourcePart> CreateStart
    (
        Source source,
        IParserTokenType<TSourcePart> startParserType,
        BracketContext bracketContext
    )
        => new
        (
            new IItem[0],
            startParserType,
            (source + 0).Span(0),
            bracketContext,
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
            IsBracketAndLeftBracket
        );
}