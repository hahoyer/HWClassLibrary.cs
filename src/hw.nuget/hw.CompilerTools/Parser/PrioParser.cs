using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
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
            ScannerItem<TTreeItem> _current;
            TTreeItem _result;

            public PrioParserWorker(PrioParser<TTreeItem> parent, Stack<OpenItem<TTreeItem>> stack)
            {
                _parent = parent;
                _stack = stack;
                _startLevel = stack.Count;
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
                        Step();
                        TraceEndInnerLoop();
                    } while(_result == null && _current.Type != null);
                } while(_result == null);
                return _result;
            }

            ScannerItem<TTreeItem> ReadNextToken(SourcePosn sourcePosn)
            {
                TraceNextToken(sourcePosn);
                var result = _parent._scanner.NextToken(sourcePosn);
                if(result.Type == null || result.Type.NextParser == null)
                    return result;

                var subType = result.Type.NextParser.Execute(sourcePosn, _stack);
                var token = new ScannerToken(result.Token.Start.Span(sourcePosn), null);
                return new ScannerItem<TTreeItem>(subType, token);
            }

            void Step()
            {
                var relation = _parent.Relation(_current.Type, _stack.Peek().Type);
                TraceRelation(relation);

                if(relation != '+')
                {
                    _left = _stack.Pop().Create(_left);
                    TracePop();

                    if(_startLevel > _stack.Count)
                    {
                        _result = _current.Create(_left, null);
                        return;
                    }
                }

                if(relation == '-')
                    return;

                var matchedItemType = relation == '=' ? _current.Type.NextTypeIfMatched : null;
                TraceItemLine("matchedItemType", matchedItemType);
                if(matchedItemType == null)
                {
                    _stack.Push(new OpenItem<TTreeItem>(_left, _current));
                    _left = null;
                }
                else
                    _left = _current.Create(_left, null);
                _current = new ScannerItem<TTreeItem>(matchedItemType, _current.Token);
            }

            void TraceRelation(char relation)
            {
                if(!Trace)
                    return;
                Tracer.Line(("" + relation).Repeat(16));
            }

            void TraceNewItem(SourcePosn sourcePosn)
            {
                if(!Trace)
                    return;
                Tracer.Line(_current.Token.SourcePart.GetDumpAroundCurrent(50));
                Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
                Tracer.Line("=================>");
                TraceItemLine("_type", _current.Type);
            }

            void TraceNextToken(SourcePosn sourcePosn)
            {
                if(!Trace)
                    return;
                Tracer.Line("\n== NextToken ====>");
                Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
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
                Tracer.Line("left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
                Tracer.IndentEnd();
            }

            void TraceEndInnerLoop()
            {
                if(!Trace)
                    return;
                Tracer.IndentStart();
                Tracer.Line
                    (
                        "itemType = "
                            + (_current.Type == null ? "null" : _current.Type.GetType().PrettyName()));
                Tracer.Line("left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
                Tracer.IndentEnd();
                Tracer.Line("\n<======================");
                Tracer.Line("end of inner loop <==");
                Tracer.Line("<======================\n");
                Tracer.IndentEnd();
            }

            void TracePop()
            {
                if(!Trace)
                    return;
                Tracer.Line("<<<<<<");
                Tracer.IndentStart();
                Tracer.Line("left = " + Extension.TreeDump(_left));
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

            void TraceItemLine(string title, IType<TTreeItem> item)
            {
                if(!Trace)
                    return;

                Tracer.IndentStart();
                var itemDump = item == null
                    ? "null"
                    : item.PrioTableId
                        + " Type = "
                        + item.GetType().PrettyName();
                Tracer.Line(title + " = " + itemDump + "\n");
                Tracer.IndentEnd();
            }

            static string TreeDump(OpenItem<TTreeItem> value)
            {
                return Extension.TreeDump(value.Left) + " "
                    + (value.Type == null ? "null" : value.Type.PrioTableId);
            }
        }

        readonly PrioTable _prioTable;
        readonly IScanner<TTreeItem> _scanner;
        public bool Trace { get; set; }

        public PrioParser(PrioTable prioTable, IScanner<TTreeItem> scanner)
        {
            _prioTable = prioTable;
            _scanner = scanner;
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

        char Relation(IType<TTreeItem> newType, IType<TTreeItem> topType)
        {
            var newTokenName = newType == null ? PrioTable.EndOfText : newType.PrioTableId;
            var recentTokenName = topType == null ? PrioTable.BeginOfText : topType.PrioTableId;
            return _prioTable.Relation(newTokenName, recentTokenName);
        }

        PrioParserWorker CreateWorker
            (SourcePosn start, Stack<OpenItem<TTreeItem>> initialStack)
        {
            Stack<OpenItem<TTreeItem>> stack;
            if(initialStack == null)
            {
                stack = new Stack<OpenItem<TTreeItem>>();
                var openItem = OpenItem<TTreeItem>.StartItem(new ScannerToken(start.Span(0), null));
                stack.Push(openItem);
            }
            else
                stack = initialStack;
            return new PrioParserWorker(this, stack);
        }
    }
}