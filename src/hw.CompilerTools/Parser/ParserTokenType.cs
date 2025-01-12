using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Parser;

/// <summary>
///     Used as base for all token types.
/// </summary>
/// <typeparam name="TParserResult">Tree structure that is returned by the parser</typeparam>
public abstract class ParserTokenType<TParserResult>
    : ScannerTokenType<TParserResult>
        , IUniqueIdProvider
        , IParserTokenType<TParserResult>
    where TParserResult : class
{
    TParserResult? IParserTokenType<TParserResult>.Create(TParserResult? left, IToken token, TParserResult? right)
    {
        var result = Create(left, token, right);
        if(token is ILinked<TParserResult> treeLinkedToken)
            treeLinkedToken.Container = result;
        return result;
    }

    string IParserTokenType<TParserResult>.PrioTableId => Id;

    string IUniqueIdProvider.Value => Id;
    [PublicAPI]
    public abstract string Id { get; }
    protected abstract TParserResult? Create(TParserResult? left, IToken token, TParserResult? right);

    public override string ToString() => base.ToString() + " Id=" + Id.Quote();

    protected override IParserTokenType<TParserResult> GetParserTokenType(string id) => this;
}

public interface ILinked<TParserResult>
{
    [PublicAPI]
    TParserResult? Container { get; set; }
}