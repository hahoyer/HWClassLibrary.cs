using hw.Scanner;


// ReSharper disable CheckNamespace

namespace hw.Parser;

sealed class SubParser<TTreeItem>
(
    IParser<TTreeItem> parser
    , Func<TTreeItem?, IParserTokenType<TTreeItem>> converter
    , Func<Stack<OpenItem<TTreeItem>>?, Stack<OpenItem<TTreeItem>>?>? prepareStack = null
)
    : ISubParser<TTreeItem>
    where TTreeItem : class
{
    readonly Func<Stack<OpenItem<TTreeItem>>?, Stack<OpenItem<TTreeItem>>?> PrepareStack = prepareStack ?? DefaultPrepareStack;
    static Stack<OpenItem<TTreeItem>>? DefaultPrepareStack(Stack<OpenItem<TTreeItem>>? _) => null;

    IParserTokenType<TTreeItem>
        ISubParser<TTreeItem>.Execute(SourcePosition sourcePosition, Stack<OpenItem<TTreeItem>>? stack)
        => converter(parser.Execute(sourcePosition, PrepareStack(stack)));
}