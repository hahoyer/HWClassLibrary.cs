using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class PrioParser<TTreeItem> : DumpableObject, IParser<TTreeItem>
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
                        result = stack.Pop().Create(result, relation == '=');

                    if(relation == '-')
                        continue;

                    if(startLevel > stack.Count)
                        return item.Create(result, null, relation == '=');

                    if(relation == '=')
                        continue;
                    
                    stack.Push(new OpenItem<TTreeItem>(result, item));
                    item = null;
                } while(item != null);
            } while(true);
        }

        ParserItem<TTreeItem> NextToken(SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> stack)
        {
            var result = _scanner.NextToken(sourcePosn, _tokenFactory, stack);
            if(result.Type == null || result.Type.Next == null)
                return result.ToParserItem;

            var subType = result.Type.Next.Execute(sourcePosn, stack);
            var sourcePart = SourcePart.Span(result.Part.Start, sourcePosn);
            return new ParserItem<TTreeItem>(subType, sourcePart);
        }
    }
}