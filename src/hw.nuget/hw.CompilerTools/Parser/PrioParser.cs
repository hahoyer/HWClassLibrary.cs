using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
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
                    TraceNextToken(sourcePosn);
                    var item = NextToken(sourcePosn, stack);
                    TraceNewItem(sourcePosn, item);

                    TTreeItem result = null;
                    do
                    {
                        TraceBeginInnerLoop(stack, result);
                        var relation = Relation(stack, item.Type);
                        TraceRelation(relation, stack);

                        if(relation == '-')
                        {
                            result = stack.Pop().Create(result);
                            TracePop(stack, result);
                        }

                        if(startLevel > stack.Count)
                            return ReturnMethodDump(item.Create(result, null));

                        if(relation == '+')
                        {
                            stack.Push(new OpenItem<TTreeItem>(result, item));
                            item = null;
                            TracePush(stack);
                        }
                    } while(item != null);
                } while(true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        void TracePush(Stack<OpenItem<TTreeItem>> stack)
        {
            if(!Trace)
                return;
            Tracer.Line(">>>>>>");
            Tracer.IndentStart();
            Tracer.Line("Level = " + stack.Count);
            Tracer.Line(FormatStackForTrace(stack));
            Tracer.IndentEnd();
        }

        static string FormatStackForTrace(Stack<OpenItem<TTreeItem>> stack)
        {
            var count = stack.Count;
            if(count == 0)
                return "stack empty";
            const int maxLines = 5;

            var isBig = count > maxLines;
            var result =
                stack.Take(maxLines - (isBig ? 1 : 0))
                    .Select((item, i) => i.ToString() + ": " + TreeDump(item))
                    .Stringify("\n");
            if(isBig)
                result += "\n...";
            return "stack="+("\n"+result).Indent();
        }

        void TraceMatch(Stack<OpenItem<TTreeItem>> stack, IType<TTreeItem> itemType)
        {
            if(!Trace)
                return;
            Tracer.Line("======");
            Tracer.IndentStart();
            Tracer.Line("Level = " + stack.Count);
            Tracer.Line("itemType = " + itemType.GetType().PrettyName());
            Tracer.Line(FormatStackForTrace(stack));
            Tracer.IndentEnd();
        }
        void TracePop(Stack<OpenItem<TTreeItem>> stack, TTreeItem result)
        {
            if(!Trace)
                return;
            Tracer.Line("<<<<<<");
            Tracer.IndentStart();
            Tracer.Line("Level = " + stack.Count);
            Tracer.Line("result = " + Extension.TreeDump(result));
            Tracer.Line(FormatStackForTrace(stack));
            Tracer.IndentEnd();
        }
        void TraceRelation(char relation, Stack<OpenItem<TTreeItem>> stack)
        {
            if(!Trace)
                return;
            Tracer.Line((("" + relation).Repeat(4) + " ").Repeat(stack.Count));
        }
        void TraceBeginInnerLoop(Stack<OpenItem<TTreeItem>> stack, TTreeItem result)
        {
            if(!Trace)
                return;
            Tracer.Line("begin of inner loop ==>");
            Tracer.IndentStart();
            Tracer.Line("Level = " + stack.Count);
            Tracer.Line("result = " + Extension.TreeDump(result));
            Tracer.Line(FormatStackForTrace(stack));
            Tracer.IndentEnd();
        }
        void TraceNewItem(SourcePosn sourcePosn, ParserItem<TTreeItem> item)
        {
            if(!Trace)
                return;
            Tracer.Line(item.Part.GetDumpAroundCurrent(50));
            Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
            Tracer.Line("=================>");
            Tracer.IndentStart();
            TraceItemLine(item);
            Tracer.IndentEnd();
        }
        static void TraceItemLine(ParserItem<TTreeItem> item)
        {
            Tracer.Line
                (
                    "item="
                        + item.Name
                        + " ObjectÍd= "
                        + item.ObjectId
                        + " Type= "
                        + item.Type.GetType().PrettyName()
                        + "\n"
                );
        }
        void TraceNextToken(SourcePosn sourcePosn)
        {
            if(!Trace)
                return;
            Tracer.Line("\n== NextToken ====>");
            Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
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

        char Relation(Stack<OpenItem<TTreeItem>> stack, IType<TTreeItem> itemType)
        {
            return stack.Peek().Relation(itemType.PrioTableName, _prioTable);
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