using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed partial class PrioParser<TSourcePart>
    {
        sealed class PrioParserWorker : DumpableObject
        {
            readonly Stack<OpenItem<TSourcePart>> Stack;
            readonly int StartLevel;
            readonly PrioParser<TSourcePart> Parent;

            TSourcePart Left;
            Item<TSourcePart> Current;

            public PrioParserWorker(PrioParser<TSourcePart> parent, Stack<OpenItem<TSourcePart>> stack)
            {
                Parent = parent;
                Stack = stack ?? new Stack<OpenItem<TSourcePart>>();
                StartLevel = Stack.Count;
                if(Trace)
                    Tracer.Line(Parent.PrioTable.Title ?? "");
            }

            bool Trace => Parent.Trace;

            public TSourcePart Execute(SourcePosn sourcePosn)
            {
                Current = CreateStartItem(sourcePosn, Parent.PrioTable);
                TraceNewItem(sourcePosn);

                while(Current.GetRightDepth() > 0)
                {
                    Left = null;
                    do
                    {
                        TraceBeginPhase("inner loop");
                        Step();
                        TraceEndPhase("inner loop");
                        Tracer.Assert(!IsBaseLevel);
                    } while(Left != null);

                    Current = ReadNextToken(sourcePosn, Current.GetRightContext());
                    TraceNewItem(sourcePosn);
                }

                while(!IsBaseLevel)
                {
                    TraceBeginPhase("end phase");
                    Step(false);
                    TraceEndPhase("end phase");
                }

                Tracer.Assert(IsBaseLevel);

                if(Current.GetRightDepth() > 0)
                    NotImplementedMethod(sourcePosn);

                return Current.Create(Left);
            }

            Item<TSourcePart> CreateStartItem(SourcePosn sourcePosn, PrioTable prioTable)
                =>
                sourcePosn.Position == 0
                    ? Item<TSourcePart>.CreateStart
                        (sourcePosn.Source, prioTable, Parent.StartParserType)
                    : ReadNextToken(sourcePosn, prioTable.BracketContext);

            Item<TSourcePart> ReadNextToken(SourcePosn sourcePosn, BracketContext context)
            {
                TraceNextToken(sourcePosn);
                var result = Item<TSourcePart>.Create
                    (Parent.Scanner.GetNextTokenGroup(sourcePosn), context);

                var nextParser = (result.Type as ISubParserProvider)?.NextParser;
                if(nextParser == null)
                    return result;

                TraceSubParserStart(result);
                var subType = nextParser.Execute(sourcePosn, Stack);
                TraceSubParserEnd(result);

                return result.RecreateWith(newType: subType, newContext: context);
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
                    Stack.Push(new OpenItem<TSourcePart>(Left, Current));
                    Left = null;
                }

                if(!relation.IsBracket)
                    return;

                Left = Current.Create(Left);

                Tracer.Assert(other != null);

                if(relation.IsMatch)
                    Current = Item<TSourcePart>.Create
                    (
                        new IItem[0],
                        ((IBracketMatch<TSourcePart>) Current.Type).Value,
                        Current.Characters.End.Span(0),
                        other.BracketItem.LeftContext,
                        Current.GetRightContext().IsBracketAndLeftBracket("")
                    );
                else
                    Current = Current.RecreateWith(newContext: other.BracketItem.LeftContext);

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

                Tracer.Line(Current.Characters.GetDumpAroundCurrent(50));
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
                TraceItemLine(nameof(Current), Current);
                Tracer.Line(nameof(Left) + " = " + Extension.TreeDump(Left));
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
                TraceItemLine(nameof(Current), Current);
                Tracer.Line(nameof(Left) + " = " + Extension.TreeDump(Left));
                Tracer.Line(FormatStackForTrace(Stack));
                Tracer.IndentEnd();
            }

            void TraceEndPhase(string tag)
            {
                if(!Trace)
                    return;

                Tracer.IndentStart();
                TraceItemLine(nameof(Current), Current);
                Tracer.Line(nameof(Left) + " = " + Extension.TreeDump(Left));
                Tracer.Line(FormatStackForTrace(Stack));
                if(StartLevel > Stack.Count)
                    Tracer.Line("*** End reached ***");
                Tracer.IndentEnd();
                Tracer.Line("\n<======================");
                Tracer.Line("end of " + tag + " <==");
                Tracer.Line("<======================\n");
                Tracer.IndentEnd();
            }

            void TraceSubParserStart(Item<TSourcePart> item)
            {
                if(!Trace)
                    return;

                Tracer.Line("\n======================>");
                Tracer.Line("begin of Subparser  ==>");
                Tracer.Line("triggerd by " + item.Characters.GetDumpAroundCurrent(50));
                Tracer.Line("======================>");
                Tracer.IndentStart();
            }

            void TraceSubParserEnd(Item<TSourcePart> item)
            {
                if(!Trace)
                    return;

                Tracer.IndentEnd();
                Tracer.Line("\n======================>");
                Tracer.Line("end of Subparser    ==>");
                Tracer.Line("triggerd by " + item.Characters.GetDumpAroundCurrent(50));
                Tracer.Line("======================>");
            }

            static string FormatStackForTrace(Stack<OpenItem<TSourcePart>> stack)
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

            void TraceItemLine(string title, Item<TSourcePart> item)
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

            static string TreeDump(OpenItem<TSourcePart> value)
                => Extension.TreeDump(value.Left) + " "
                + (value.Type == null
                    ? "null"
                    : value.Type.PrioTableId + " NextDepth=" + value.NextDepth);
        }

        internal interface ISubParserProvider
        {
            ISubParser<TSourcePart> NextParser { get; }
        }
    }
}