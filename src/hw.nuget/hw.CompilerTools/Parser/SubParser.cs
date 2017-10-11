using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    sealed class SubParser<TTreeItem> : ISubParser<TTreeItem>
        where TTreeItem : class, ISourcePartProxy
    {
        readonly IParser<TTreeItem> _parser;
        readonly Func<TTreeItem, IParserTokenType<TTreeItem>> _converter;
        readonly Func<Stack<OpenItem<TTreeItem>>, Stack<OpenItem<TTreeItem>>> _prepareStack;

        public SubParser
            (
            IParser<TTreeItem> parser,
            Func<TTreeItem, IParserTokenType<TTreeItem>> converter,
            Func<Stack<OpenItem<TTreeItem>>, Stack<OpenItem<TTreeItem>>> prepareStack = null)
        {
            _parser = parser;
            _converter = converter;
            _prepareStack = prepareStack ?? (stack => null);
        }

        IParserTokenType<TTreeItem> ISubParser<TTreeItem>.Execute
            (SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> stack)
        {
            return _converter(_parser.Execute(sourcePosn, _prepareStack(stack)));
        }
    }
}