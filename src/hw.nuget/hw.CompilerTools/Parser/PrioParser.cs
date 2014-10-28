using System;
using System.Collections.Generic;
using hw.Helper;
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
        public bool Trace { get; set; }

        public PrioParser(PrioTable prioTable, IScanner<TTreeItem> scanner, ITokenFactory<TTreeItem> tokenFactory)
        {
            _prioTable = prioTable;
            _scanner = scanner;
            _tokenFactory = tokenFactory;
        }
        TTreeItem IParser<TTreeItem>.Execute(SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> initialStack)
        {
            StartMethodDump(Trace, sourcePosn.GetDumpAroundCurrent(50), initialStack);
            try
            {
                var stack = InitializeStack(sourcePosn, initialStack);
                var startLevel = stack.Count;

                do
                {
                    if(Trace)
                    {
                        Tracer.Line("\n== NextToken ====>");
                        Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
                    }

                    var item = NextToken(sourcePosn, stack);
                    if(Trace)
                    {
                        Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
                        Tracer.Line("=================>\n");
                        Tracer.IndentStart();
                        Tracer.Line("item=" + item.Name + " ObjectÍd= " + item.ObjectId + " TokenClass= " + item.Type.GetType().PrettyName());
                        Tracer.IndentEnd();
                    }

                    TTreeItem result = null;
                    do
                    {
                        if(Trace)
                        {
                            Tracer.Line("begin of inner loop ==>");
                            Tracer.IndentStart();
                            Tracer.Line("Level = " + stack.Count);
                            Tracer.Line("result = " + Extension.TreeDump(result));
                            Tracer.Line("top = " + TreeDump(stack.Peek()));
                            Tracer.IndentEnd();
                        }

                        var relation = Relation(stack, item);
                        if(Trace)
                            Tracer.Line
                                ("" + relation + relation + relation + relation + relation + relation + relation + relation + relation + relation);

                        if(relation != '+')
                        {
                            result = stack.Pop().Create(result, relation == '=');
                            if(Trace)
                            {
                                Tracer.Line("<<<<<<");
                                Tracer.IndentStart();
                                Tracer.Line("Level = " + stack.Count);
                                Tracer.Line("result = " + Extension.TreeDump(result));
                                Tracer.Line("top = " + TreeDump(stack.Peek()));
                                Tracer.IndentEnd();
                            }
                        }

                        if(relation == '-')
                            continue;

                        if(startLevel > stack.Count)
                            return ReturnMethodDump(item.Create(result, null, relation == '='));

                        if(relation == '=')
                            continue;

                        stack.Push(new OpenItem<TTreeItem>(result, item));
                        item = null;
                        if(Trace)
                        {
                            Tracer.Line(">>>>>>");
                            Tracer.IndentStart();
                            Tracer.Line("Level = " + stack.Count);
                            Tracer.Line("top = " + TreeDump(stack.Peek()));
                            Tracer.IndentEnd();
                        }
                    } while(item != null);
                } while(true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        static string TreeDump(OpenItem<TTreeItem> value) { return Extension.TreeDump(value.Left) + " " + value.Item.Name; }

        static Stack<OpenItem<TTreeItem>> InitializeStack(SourcePosn sourcePosn, Stack<OpenItem<TTreeItem>> initialStack)
        {
            Stack<OpenItem<TTreeItem>> stack;
            if(initialStack == null)
            {
                stack = new Stack<OpenItem<TTreeItem>>();
                var openItem = OpenItem<TTreeItem>.StartItem(sourcePosn);
                stack.Push(openItem);
            }
            else
                stack = initialStack;
            return stack;
        }

        char Relation(Stack<OpenItem<TTreeItem>> stack, ParserItem<TTreeItem> item) { return stack.Peek().Relation(item.Name, _prioTable); }

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