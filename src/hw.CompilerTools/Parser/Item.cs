using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public sealed class Item<TParserResult>
    : DumpableObject
        , PrioTable.ITargetItem
        , IToken
        , ILinked<TParserResult>
    where TParserResult : class
{
    [EnableDump]
    public readonly BracketContext Context;

    public readonly BracketSide BracketSide;
    public readonly IItem[] PrefixItems;
    public readonly IParserTokenType<TParserResult> Type;
    internal readonly SourcePart Characters;

    [EnableDump]
    public int Depth => Context.Depth;

    Item
    (
        IEnumerable<IItem> prefixItems,
        IParserTokenType<TParserResult> type,
        SourcePart characters,
        BracketContext context,
        BracketSide bracketSide
    )
    {
        PrefixItems = prefixItems.ToArray();
        Type = type;
        Characters = characters;
        Context = context;
        BracketSide = bracketSide;
    }

    TParserResult? ILinked<TParserResult>.Container
    {
        get;
        set
        {
            if(field != null)
                (value == field).Assert();

            field = value;
        }
    }

    BracketContext PrioTable.ITargetItem.LeftContext => Context;

    string PrioTable.ITargetItem.Token => Type.PrioTableId;
    BracketSide IToken.BracketSide => BracketSide;
    SourcePart IToken.Characters => Characters;
    int IToken.PrecededWith => PrefixItems.Sum(item => item.Length);

    public static Item<TParserResult> Create
    (
        IItem[] items
        , SourcePosition end
        , BracketContext context
        , bool isSubParser
    )
    {
        var prefixItems = items.Take(items.Length - 1);
        var mainItem = items.Last();
        var token = end.Span(-mainItem.Length);
        var bracketSide = context.IsLeftBracket(token.Id);

        var mainItemScannerTokenType = mainItem.ScannerTokenType;
        var parserTokenFactory = mainItemScannerTokenType
            .ParserTokenFactory;
        var parserType = parserTokenFactory
            .AssertNotNull()
            .GetTokenType<TParserResult>(token.Id);

        return new
        (
            prefixItems,
            parserType,
            token,
            context,
            bracketSide
        );
    }

    public static Item<TParserResult> CreateStart
    (
        Source source,
        IParserTokenType<TParserResult> startParserType,
        BracketContext bracketContext
    )
        => new
        (
            new IItem[0],
            startParserType,
            (source + 0).Span(0),
            bracketContext,
            BracketSide.None
        );

    public Item<TParserResult> RecreateWith
    (
        IEnumerable<IItem>? newPrefixItems = null,
        IParserTokenType<TParserResult>? newType = null,
        BracketContext? newContext = null
    )
        => new
        (
            newPrefixItems ?? PrefixItems,
            newType ?? Type,
            Characters,
            newContext ?? Context,
            BracketSide
        );

    public TParserResult? Create(TParserResult? left) => Type.Create(left, this, null);

    public Item<TParserResult> CreateMatch(OpenItem<TParserResult> other)
        => new
        (
            new IItem[0],
            ((IBracketMatch<TParserResult>)Type).Value,
            Characters.End.Span(0),
            other.BracketItem.LeftContext,
            BracketSide
        );
}