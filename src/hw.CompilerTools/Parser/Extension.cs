using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public static class Extension
{
    public static TOut Operation<TIn, TOut>(this IOperator<TIn, TOut> @operator, TIn left, IToken token, TIn right)
        where TIn : class => left == null? right == null
            ? @operator.Terminal(token)
            : @operator.Prefix(token, right) :
        right == null? @operator.Suffix(left, token) : @operator.Infix(left, token, right);

    public static ISubParser<TTreeItem> Convert<TTreeItem>
        (this IParser<TTreeItem> parser, Func<TTreeItem, IParserTokenType<TTreeItem>> converter)
        where TTreeItem : class => new SubParser<TTreeItem>(parser, converter);

    public static string TreeDump(this IBinaryTreeItem value)
    {
        if(value == null)
            return "<null>";

        var result = "(";
        result += TreeDump(value.Left);
        result += " ";
        result += value.TokenId;
        result += " ";
        result += TreeDump(value.Right);
        result += ")";
        return result;
    }

    public static int BracketBalance(this IToken token)
        => token.BracketSide switch
        {
            BracketSide.Left=> -1
            , BracketSide.Right=> 1
            , BracketSide.None => 0
            , var _ => throw new ArgumentOutOfRangeException()
        };

    internal static string TreeDump<TTreeItem>(TTreeItem value)
        where TTreeItem : class
        => value is IBinaryTreeItem t
            ? TreeDump(t) 
            : Tracer.Dump(value);
}