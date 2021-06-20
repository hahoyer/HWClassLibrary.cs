using System;
using System.Collections.Generic;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    sealed class SubParser<TTreeItem> : ISubParser<TTreeItem>
        where TTreeItem : class
    {
        readonly Func<TTreeItem, IParserTokenType<TTreeItem>> Converter;
        readonly IParser<TTreeItem> Parser;
        readonly Func<Stack<OpenItem<TTreeItem>>, Stack<OpenItem<TTreeItem>>> PrepareStack;

        public SubParser
        (
            IParser<TTreeItem> parser,
            Func<TTreeItem, IParserTokenType<TTreeItem>> converter,
            Func<Stack<OpenItem<TTreeItem>>, Stack<OpenItem<TTreeItem>>> prepareStack = null
        )
        {
            Parser = parser;
            Converter = converter;
            PrepareStack = prepareStack ?? (stack => null);
        }

        IParserTokenType<TTreeItem> ISubParser<TTreeItem>.Execute
            (SourcePosition sourcePosition, Stack<OpenItem<TTreeItem>> stack) =>
            Converter(Parser.Execute(sourcePosition, PrepareStack(stack)));
    }
}