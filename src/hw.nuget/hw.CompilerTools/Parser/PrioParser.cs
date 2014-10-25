using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    class PrioParser<TTreeItem> : IParser<TTreeItem>
        where TTreeItem : class
    {
        readonly PrioTable _prioTable;
        readonly IScanner<TTreeItem> _scanner;
        readonly ITokenFactory<TTreeItem> _tokenFactory;

        public PrioParser(PrioTable prioTable, IScanner<TTreeItem> scanner, ITokenFactory<TTreeItem> tokenFactory)
        {
            _prioTable = prioTable;
            _scanner = scanner;
            _tokenFactory = tokenFactory;
        }
        TTreeItem IParser<TTreeItem>.Execute(SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> stack)
        {
            if(stack == null)
            {
                stack = new Stack<OpenItem<TTreeItem>>();
                var openItem = OpenItem<TTreeItem>.StartItem(sourcePosn);
                stack.Push(openItem);
            }

            var startLevel = stack.Count;

            do
            {
                var item = NextToken(sourcePosn, stack);
                TTreeItem result = null;
                do
                {
                    var topItem = stack.Peek();
                    var relation = topItem.Relation(item.Name, _prioTable);

                    if(relation != '+')
                        result = stack.Pop().Create(result);

                    if(relation == '-')
                        continue;

                    if(startLevel > stack.Count)
                        return result;

                    stack.Push(new OpenItem<TTreeItem>(result, item, relation == '='));
                    result = null;
                } while(result != null);
            } while(true);
        }

        ParserItem<TTreeItem> NextToken
            (SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> stack)
        {
            var result = _scanner.NextToken(sourcePosn, _tokenFactory, stack);
            if(result.Type == null || result.Type.Next == null)
                return result.ToParserItem;

            var subType = result.Type.Next.Execute(sourcePosn, stack);
            var sourcePart = SourcePart.Span(result.Part.Start,sourcePosn);
            return new ParserItem<TTreeItem>(subType,sourcePart);
        }


        public static TTreeItem Operation(IOperator<TTreeItem> @operator, TTreeItem left, SourcePart token, TTreeItem right)
        {
            return left == null
                ? (right == null ? @operator.Terminal(token) : @operator.Prefix(token, right))
                : (right == null ? @operator.Suffix(left, token) : @operator.Infix(left, token, right));
        }
    }
}