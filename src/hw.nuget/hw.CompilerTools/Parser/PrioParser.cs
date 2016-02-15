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
        sealed class PrioParserWorker : DumpableObject
        {
            readonly Stack<OpenItem<TTreeItem>> _stack;
            readonly int _startLevel;
            readonly PrioParser<TTreeItem> _parent;

            TTreeItem _left;
            Item<TTreeItem> _current;

            public PrioParserWorker(PrioParser<TTreeItem> parent, Stack<OpenItem<TTreeItem>> stack)
            {
                _parent = parent;
                _stack = stack ?? new Stack<OpenItem<TTreeItem>>();
                _startLevel = _stack.Count;
                if(Trace)
                    Tracer.Line(_parent._prioTable.Title ?? "");
            }

            bool Trace => _parent.Trace;

            public TTreeItem Execute(SourcePosn sourcePosn)
            {
                _current = sourcePosn.Position == 0
                    ? CreateStartItem(sourcePosn)
                    : ReadNextToken(sourcePosn, BracketContext.Empty);
                TraceNewItem(sourcePosn);

                while(_current.NextContext.Depth > 0)
                {
                    _left = null;
                    do
                    {
                        TraceBeginPhase("inner loop");
                        Step();
                        TraceEndPhase("inner loop");
                        Tracer.Assert(!IsBaseLevel);
                    } while(_left != null);

                    _current = ReadNextToken(sourcePosn, _current.NextContext);
                    TraceNewItem(sourcePosn);
                }

                while(!IsBaseLevel)
                {
                    TraceBeginPhase("end phase");
                    TraceRelation(PrioTable.Relation.Pull);
                    _left = _stack.Pop().Create(_left);
                    TraceEndPhase("end phase");
                }

                Tracer.Assert(IsBaseLevel);

                if(_current.NextContext.Depth > 0)
                    NotImplementedMethod(sourcePosn);

                return _current.Type.Create(_left, _current.Token, null);
            }

            Item<TTreeItem> CreateStartItem(SourcePosn sourcePosn)
                => new Item<TTreeItem>
                    (
                    null,
                    new Token(null, sourcePosn.Span(0)),
                    BracketContext.Empty,
                    _parent._prioTable.NextContext(BracketContext.Empty, PrioTable.BeginOfText)
                    );

            Item<TTreeItem> ReadNextToken(SourcePosn sourcePosn, BracketContext context)
            {
                TraceNextToken(sourcePosn);
                var result = _parent._scanner.NextToken(sourcePosn);
                var nextContext = _parent._prioTable.NextContext
                    (context, result.Type.Type.PrioTableId);

                if(result.Type?.NextParser == null)
                    return new Item<TTreeItem>(result, context, nextContext);

                TraceSubParserStart(result);
                var subType = result.Type.NextParser.Execute(sourcePosn, _stack);
                TraceSubParserEnd(result);

                var token = new Token(result.Token);
                return new Item<TTreeItem>(subType, token, context, nextContext);
            }

            void Step()
            {
                do
                {
                    var other = IsBaseLevel ? null : _stack.Peek();

                    var relation = other == null
                        ? PrioTable.Relation.Push
                        : _parent.GetRelation(_current, other.BracketItem);

                    TraceRelation(relation);

                    if(relation.IsPull)
                        _left = _stack.Pop().Create(_left);
                    if(relation.IsPush)
                    {
                        _stack.Push(new OpenItem<TTreeItem>(_left, _current));
                        _left = null;
                    }

                    if(!relation.IsBracket)
                        return;

                    _left = _current.Type.Create(_left, _current.Token, null);

                    Tracer.Assert(other != null);
                    _current = _current.GetBracketMatch(relation.IsMatch, other);

                    TraceMatchPhase();
                } while(_current.Type != null);

                NotImplementedMethod();
            }

            bool IsBaseLevel => _stack.Count == _startLevel;

            void TraceRelation(PrioTable.Relation relation)
            {
                if(!Trace)
                    return;
                Tracer.Line("---- " + relation + " ----");
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

            void TraceMatchPhase()
            {
                if(!Trace)
                    return;
                Tracer.Line("\n======================>");
                Tracer.Line("match ================>");
                Tracer.Line("======================>");
                Tracer.IndentStart();
                TraceItemLine("_current", _current);
                Tracer.Line("_left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
                Tracer.IndentEnd();
            }

            void TraceBeginPhase(string tag)
            {
                if(!Trace)
                    return;
                Tracer.IndentStart();
                Tracer.Line("\n======================>");
                Tracer.Line("begin of " + tag + " ==>");
                Tracer.Line("======================>");
                Tracer.IndentStart();
                TraceItemLine("_current", _current);
                Tracer.Line("_left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
                Tracer.IndentEnd();
            }

            void TraceEndPhase(string tag)
            {
                if(!Trace)
                    return;
                Tracer.IndentStart();
                TraceItemLine("_current", _current);
                Tracer.Line("left = " + Extension.TreeDump(_left));
                Tracer.Line(FormatStackForTrace(_stack));
                if(_startLevel > _stack.Count)
                    Tracer.Line("*** End reached ***");
                Tracer.IndentEnd();
                Tracer.Line("\n<======================");
                Tracer.Line("end of " + tag + " <==");
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

            static string FormatStackForTrace(Stack<OpenItem<TTreeItem>> stack)
            {
                var count = stack.Count;
                if(count == 0)
                    return "stack empty";
                const int MaxLines = 5;

                var isBig = count > MaxLines;
                var result =
                    stack.Take(MaxLines - (isBig ? 1 : 0))
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
                        : value.Type.PrioTableId + " NextDepth=" + value.BracketItem.NextDepth);
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
                return ReturnMethodDump(CreateWorker(initialStack).Execute(start));
            }
            finally
            {
                EndMethodDump();
            }
        }

        PrioTable.Relation GetRelation(PrioTable.ITargetItem newType, PrioTable.ITargetItem topType)
            => _prioTable.GetRelation(newType, topType);

        PrioParserWorker CreateWorker(Stack<OpenItem<TTreeItem>> stack)
            => new PrioParserWorker(this, stack);
    }
}