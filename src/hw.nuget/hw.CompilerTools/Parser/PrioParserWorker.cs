using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed partial class PrioParser<TTreeItem>
    {
        sealed class PrioParserWorker : DumpableObject
        {
            readonly Stack<OpenItem<TTreeItem>> Stack;
            readonly int StartLevel;
            readonly PrioParser<TTreeItem> Parent;

            TTreeItem Left;
            Item<TTreeItem> Current;

            public PrioParserWorker(PrioParser<TTreeItem> parent, Stack<OpenItem<TTreeItem>> stack)
            {
                Parent = parent;
                Stack = stack ?? new Stack<OpenItem<TTreeItem>>();
                StartLevel = Stack.Count;
                if(Trace)
                    Tracer.Line(Parent.PrioTable.Title ?? "");
            }

            bool Trace => Parent.Trace;

            public TTreeItem Execute(SourcePosn sourcePosn)
            {
                Current = CreateStartItem(sourcePosn, Parent.PrioTable);
                TraceNewItem(sourcePosn);

                while(Current.RightDepth() > 0)
                {
                    Left = null;
                    do
                    {
                        TraceBeginPhase("inner loop");
                        Step();
                        TraceEndPhase("inner loop");
                        Tracer.Assert(!IsBaseLevel);
                    } while(Left != null);

                    Current = ReadNextToken(sourcePosn, Current.RightCOntext());
                    TraceNewItem(sourcePosn);
                }

                while(!IsBaseLevel)
                {
                    TraceBeginPhase("end phase");
                    Step(false);
                    TraceEndPhase("end phase");
                }

                Tracer.Assert(IsBaseLevel);

                if(Current.RightDepth() > 0)
                    NotImplementedMethod(sourcePosn);

                return Current.Create(Left);
            }

            Item<TTreeItem> CreateStartItem(SourcePosn sourcePosn, PrioTable prioTable)
                =>
                    sourcePosn.Position == 0
                        ? Item<TTreeItem>.CreateStart
                            (sourcePosn.Source, prioTable, Parent.StartType)
                        : ReadNextToken(sourcePosn, prioTable.BracketContext);

            Item<TTreeItem> ReadNextToken(SourcePosn sourcePosn, BracketContext context)
            {
                TraceNextToken(sourcePosn);
                var result = Parent.Scanner.NextToken(sourcePosn);
                if(result.Type.NextParser == null)
                    return Item<TTreeItem>.Create(result, context);

                TraceSubParserStart(result);
                var subType = result.Type.NextParser.Execute(sourcePosn, Stack);
                TraceSubParserEnd(result);

                var token = Token.Create(result.Token, context);
                return Item<TTreeItem>.Create(subType, token, context);
            }

            void Step(bool canPush = true)
            {
                var other = IsBaseLevel ? null : Stack.Peek();

                var relation = other == null
                    ? PrioTable.Relation.Push
                    : Parent.GetRelation(Current, other.BracketItem);

                TraceRelation(relation);
                if(!canPush)
                    Tracer.Assert(!relation.IsPush);

                if(relation.IsPull)
                    Left = Stack.Pop().Create(Left);
                if(relation.IsPush)
                {
                    Stack.Push(new OpenItem<TTreeItem>(Left, Current));
                    Left = null;
                }

                if(!relation.IsBracket)
                    return;

                Left = Current.Create(Left);

                Tracer.Assert(other != null);
                var newType = Current.Type;

                if (relation.IsMatch)
                    newType = ((IBracketMatch<TTreeItem>)newType).Value;

                var token = Current.Token;
                if (relation.IsMatch)
                    token = Token.Create(new ScannerToken(token.SourcePart.End.Span(0), null), Current.RightCOntext());

                Current = Item<TTreeItem>
                    .Create(newType, token, other.BracketItem.LeftContext);

                TraceMatchPhase();
            }

            bool IsBaseLevel => Stack.Count == StartLevel;

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
                Tracer.Line(Current.Token.SourcePart.GetDumpAroundCurrent(50));
                Tracer.Line(sourcePosn.GetDumpAroundCurrent(50));
                Tracer.Line("Depth = " + Current.Depth);
                Tracer.Line("=================>");
            }

            void TraceMatchPhase()
            {
                if(!Trace)
                    return;
                Tracer.Line("\n======================>");
                Tracer.Line("bracket matching======>");
                Tracer.Line("======================>");
                Tracer.IndentStart();
                TraceItemLine("_current", Current);
                Tracer.Line("_left = " + Extension.TreeDump(Left));
                Tracer.Line(FormatStackForTrace(Stack));
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
                TraceItemLine("_current", Current);
                Tracer.Line("_left = " + Extension.TreeDump(Left));
                Tracer.Line(FormatStackForTrace(Stack));
                Tracer.IndentEnd();
            }

            void TraceEndPhase(string tag)
            {
                if(!Trace)
                    return;
                Tracer.IndentStart();
                TraceItemLine("_current", Current);
                Tracer.Line("left = " + Extension.TreeDump(Left));
                Tracer.Line(FormatStackForTrace(Stack));
                if(StartLevel > Stack.Count)
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
                => Extension.TreeDump(value.Left) + " "
                    + (value.Type == null
                        ? "null"
                        : value.Type.PrioTableId + " NextDepth=" + value.NextDepth);
        }
    }
}