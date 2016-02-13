using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class PrioParser<TTreeItem> : DumpableObject, IParser<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        sealed class PrioParserWorker
        {
            readonly Stack<OpenItem<TTreeItem>> _stack;
            readonly int _startLevel;
            readonly PrioParser<TTreeItem> _parent;

            TTreeItem _left;
            Item<TTreeItem> _current;

            public PrioParserWorker(PrioParser<TTreeItem> parent, Stack<OpenItem<TTreeItem>> stack)
            {
                _parent = parent;
                _stack = stack;
                _startLevel = stack.Count;
                _current = stack.Peek().Current;
                if(Trace)
                    Tracer.Line(_parent._prioTable.Title);
            }

            bool Trace { get { return _parent.Trace; } }

            public TTreeItem Execute(SourcePosn sourcePosn)
            {
                do
                {
                    _current = ReadNextToken(sourcePosn);
                    TraceNewItem(sourcePosn);

                    _left = null;
                    do
                    {
                        TraceBeginInnerLoop();
                        var result = Step();
                        TraceEndInnerLoop();
                        if(result != null)
                            return result;
                    } while(_left != null);
                } while(true);
            }

            Item<TTreeItem> ReadNextToken(SourcePosn sourcePosn)
            {
                TraceNextToken(sourcePosn);
                var newContext = _parent._prioTable.NextContext(_current);
                var result = _parent._scanner.NextToken(sourcePosn);

                if(result.Type?.NextParser == null)
                    return new Item<TTreeItem>(result, newContext);

                TraceSubParserStart(result);
                var subType = result.Type.NextParser.Execute(sourcePosn, _stack);
                TraceSubParserEnd(result);

                return new Item<TTreeItem>(subType, result.Token, newContext);
            }

            TTreeItem Step()
            {
                var relation = _parent.Relation(_current, _stack.Peek().Item);
                TraceRelation(relation);

                if(relation != '+')
                {
                    _left = _stack.Pop().Create(_left);
                    TracePop();

                    if(_startLevel > _stack.Count)
                        return _current.Create(_left, null);
                }

                if(relation == '-')
                    return null;

                if(Trace)
                    Tracer.Line("");
                _stack.Push(new OpenItem<TTreeItem>(_left, _current));
                _left = null;
                return null;
            }

            void TraceRelation(char relation)
            {
                if(!Trace)
                    return;
                Tracer.Line(("" + relation).Repeat(16));
            }

            void TraceNextToken(SourcePosn sourcePosn)
            {
                if(!Trace)
                    return;
                Tracer.Line("\n== NextToken ====>");
                Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
            }

            void TraceNewItem(SourcePosn sourcePosn)
            {
                if(!Trace)
                    return;
                Tracer.Line(_current.Token.SourcePart.GetDumpAroundCurrent(50));
                Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
                Tracer.Line("Depth = " + _current.Depth);
                Tracer.Line("=================>");
            }

            void TraceBeginInnerLoop()
            {
                if(!Trace)
                    return;
                Tracer.IndentStart();
                Tracer.Line("\n======================>");
                Tracer.Line("begin of inner loop ==>");
                Tracer.Line("======================>");
                Tracer.IndentStart();
                TraceItemLine("_current", _current);
                Tracer.Line("_left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
                Tracer.IndentEnd();
            }

            void TraceEndInnerLoop()
            {
                if(!Trace)
                    return;
                Tracer.IndentStart();
                Tracer.Line("");
                TraceItemLine("_current", _current);
                Tracer.Line("left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
                Tracer.IndentEnd();
                Tracer.Line("\n<======================");
                Tracer.Line("end of inner loop <==");
                Tracer.Line("<======================\n");
                Tracer.IndentEnd();
            }

            void TraceSubParserStart(Scanner<TTreeItem>.Item item)
            {
                if(!Trace)
                    return;
                Tracer.Line("\n======================>");
                Tracer.Line("begin of Subparser  ==>");
                Tracer.Line("triggerd by " + item.Token.SourcePart.GetDumpAroundCurrent(50));
                Tracer.Line("======================>");
                Tracer.IndentStart();
            }

            void TraceSubParserEnd(Scanner<TTreeItem>.Item item)
            {
                if(!Trace)
                    return;
                Tracer.IndentEnd();
                Tracer.Line("\n======================>");
                Tracer.Line("end of Subparser    ==>");
                Tracer.Line("triggerd by " + item.Token.SourcePart.GetDumpAroundCurrent(50));
                Tracer.Line("======================>");
            }

            void TracePop()
            {
                if(!Trace)
                    return;
                Tracer.Line("<<<<<<");
                Tracer.IndentStart();
                Tracer.Line("_left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
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
                return "stack: " + stack.Count + " items" + ("\n" + result).Indent();
            }

            void TraceItemLine(string title, Item<TTreeItem> item)
            {
                if(!Trace)
                    return;

                if(item == null)
                {
                    Tracer.Line(title + " = null");
                    return;
                }

                var typeDump = item.Type == null
                    ? "null"
                    : item.Type.PrioTableId
                        + " Type = "
                        + item.Type.GetType().PrettyName();
                Tracer.Line(title + " = " + typeDump + " Depth=" + item.Context.Depth);
            }

            static string TreeDump(OpenItem<TTreeItem> value)
            {
                return Extension.TreeDump(value.Left) + " "
                    + (value.Type == null
                        ? "null"
                        : value.Type.PrioTableId + " Depth=" + value.Current.Depth);
            }
        }

        readonly PrioTable _prioTable;
        readonly IScanner<TTreeItem> _scanner;
        readonly IType<TTreeItem> StartType;
        public bool Trace { get; set; }

        public PrioParser(PrioTable prioTable, IScanner<TTreeItem> scanner, IType<TTreeItem> startType = null)
        {
            _prioTable = prioTable;
            _scanner = scanner;
            StartType = startType;
        }


        TTreeItem IParser<TTreeItem>.Execute
            (SourcePosn start, Stack<OpenItem<TTreeItem>> initialStack)
        {
            StartMethodDump(Trace, start.GetDumpAroundCurrent(50), initialStack);
            try
            {
                return ReturnMethodDump(CreateWorker(start, initialStack).Execute(start));
            }
            finally
            {
                EndMethodDump();
            }
        }

        char Relation(PrioTable.IItem newType, PrioTable.IItem topType)
        {
            return _prioTable.Relation(newType, topType);
        }

        PrioParserWorker CreateWorker
            (SourcePosn start, Stack<OpenItem<TTreeItem>> initialStack)
        {
            Stack<OpenItem<TTreeItem>> stack;
            if(initialStack == null)
            {
                stack = new Stack<OpenItem<TTreeItem>>();
                var openItem = OpenItem<TTreeItem>
                    .StartItem(new ScannerToken(start.Span(0), null), PrioTable.StartContext, StartType);
                stack.Push(openItem);
            }
            else
                stack = initialStack;
            return new PrioParserWorker(this, stack);
        }
    }
}