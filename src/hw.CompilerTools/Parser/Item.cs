using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

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

    public readonly BracketSide BracketSide;
    public readonly IItem[] PrefixItems;
    public readonly IParserTokenType<TSourcePart> Type;
    internal readonly SourcePart Characters;

    [EnableDump]
    public int Depth => Context.Depth;

    Item
    (
        IEnumerable<IItem> prefixItems,
        IParserTokenType<TSourcePart> type,
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

    TSourcePart? ILinked<TSourcePart>.Container
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

    public static Item<TSourcePart> Create
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
            .GetTokenType<TSourcePart>(token.Id);

        return new
        (
            prefixItems,
            parserType,
            token,
            context,
            bracketSide
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
            BracketSide.None
        );

    public Item<TSourcePart> RecreateWith
    (
        IEnumerable<IItem>? newPrefixItems = null,
        IParserTokenType<TSourcePart>? newType = null,
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

    public TSourcePart? Create(TSourcePart? left) => Type.Create(left, this, null);

    public Item<TSourcePart> CreateMatch(OpenItem<TSourcePart> other)
        => new
        (
            new IItem[0],
            ((IBracketMatch<TSourcePart>)Type).Value,
            Characters.End.Span(0),
            other.BracketItem.LeftContext,
            BracketSide
        );
}