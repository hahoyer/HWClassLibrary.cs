using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public interface IParser<TSourcePart>
    where TSourcePart : class
{
    bool Trace { get; set; }
    TSourcePart? Execute(Source source);
    TSourcePart? Execute(SourcePosition source, Stack<OpenItem<TSourcePart>>? stack);
}

public interface ISubParser<TSourcePart>
    where TSourcePart : class
{
    IParserTokenType<TSourcePart> Execute(SourcePosition sourcePosition, Stack<OpenItem<TSourcePart>>? stack = null);
}