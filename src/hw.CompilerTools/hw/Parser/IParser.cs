using hw.Scanner;


// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public interface IParser<TParserResult>
    where TParserResult : class
{
    bool Trace { get; set; }
    TParserResult? Execute(Source source);
    TParserResult? Execute(SourcePosition source, Stack<OpenItem<TParserResult>>? stack);
}

public interface ISubParser<TParserResult>
    where TParserResult : class
{
    IParserTokenType<TParserResult> Execute(SourcePosition sourcePosition, Stack<OpenItem<TParserResult>>? stack = null);
}