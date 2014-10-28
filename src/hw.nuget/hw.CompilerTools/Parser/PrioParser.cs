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
        TTreeItem IParser<TTreeItem>.Execute(SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> initialStack)
        {


            var trace = true;
            StartMethodDump(trace, sourcePosn, initialStack);
            try
            {
                int indent = initialStack == null ? 1 : 0;
                var stack = InitializeStack(sourcePosn, initialStack);
                var startLevel = stack.Count;

                do
                {
                    if (trace)
                    {
                        Tracer.FlaggedLine("begin of outer poop");
                        Tracer.Line("=====================================================");
                        Tracer.IndentStart();
                        Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
                        Tracer.IndentEnd();
                    }

                    var item = NextToken(sourcePosn, stack);
                    Dump("item", item); 

                    TTreeItem result = null;
                    do
                    {
                        if(trace)
                        {
                            Tracer.FlaggedLine("begin of inner poop");
                            Tracer.Line("--------------------------------------------------");
                            Tracer.IndentStart();
                            Tracer.Line("Level = " + stack.Count );
                            Tracer.Line("item = " + item.DebuggerDumpString);
                            Tracer.Line("result = " + Tracer.Dump(result));
                            Tracer.IndentEnd();
                        }

                        var relation = Relation(stack, item);
                        Dump("relation", relation);

                        if(relation != '+')
                        {
                            result = stack.Pop().Create(result, relation == '=');
                            indent--;
                            Tracer.IndentEnd();
                            Dump("Pop result", result); 
                        }

                        if(relation == '-')
                        {
                            Dump("Continue inner loop, because of relation", relation);
                            continue;
                        }

                        if(startLevel > stack.Count)
                        {
                            while(indent > 0)
                            {
                                Tracer.IndentEnd();
                                --indent;
                            }
                            return ReturnMethodDump(item.Create(result, null, relation == '='), false);
                        }

                        if(relation == '=')
                        {
                            Dump("Continue inner loop, because of relation", relation);
                            continue;
                        }

                        Tracer.IndentStart();
                        indent++;
                        var openItem = new OpenItem<TTreeItem>(result, item);
                        Dump("Push openItem", openItem);
                        stack.Push(openItem);
                        item = null;
                        Dump("leave inner loop since item", item);
                    } while (item != null);
                } while(true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        static Stack<OpenItem<TTreeItem>> InitializeStack(SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> initialStack)
        {
            Stack<OpenItem<TTreeItem>> stack;
            if(initialStack == null)
            {
                stack = new Stack<OpenItem<TTreeItem>>();
                var openItem = OpenItem<TTreeItem>.StartItem(sourcePosn);
                stack.Push(openItem);
                Tracer.IndentStart();
            }
            else
                stack = initialStack;
            return stack;
        }

        char Relation(Stack<OpenItem<TTreeItem>> stack, ParserItem<TTreeItem> item)
        {
            return stack.Peek().Relation(item.Name, _prioTable);
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