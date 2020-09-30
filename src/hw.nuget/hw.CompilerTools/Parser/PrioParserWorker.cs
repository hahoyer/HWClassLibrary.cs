using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed partial class PrioParser<TSourcePart>
    {
        internal interface ISubParserProvider
        {
            ISubParser<TSourcePart> NextParser { get; }
        }

        sealed class PrioParserWorker : DumpableObject
        {
            Item<TSourcePart> Current;

            TSourcePart Left;
            readonly PrioParser<TSourcePart> Parent;
            readonly Stack<OpenItem<TSourcePart>> Stack;
            readonly int StartLevel;

            public PrioParserWorker(PrioParser<TSourcePart> parent, Stack<OpenItem<TSourcePart>> stack)
            {
                Parent = parent;
                Stack = stack ?? new Stack<OpenItem<TSourcePart>>();
                StartLevel = Stack.Count;
                if(Trace)
                    Tracer.Line(Parent.PrioTable.Title ?? "");
            }

            bool Trace => Parent.Trace;

            bool IsBaseLevel => Stack.Count == StartLevel;

            public TSourcePart Execute(SourcePosition sourcePosition)
            {
                Current = CreateStartItem(sourcePosition, Parent.PrioTable);
                TraceNewItem(sourcePosition);

                while(Current.GetRightDepth() > 0)
                {
                    Left = null;
                    do
                    {
                        TraceBeginPhase("inner loop");
                        Step();
                        TraceEndPhase("inner loop");
                        Tracer.Assert(!IsBaseLevel);
                    }
                    while(Left != null);

                    Current = ReadNextToken(sourcePosition, Current.GetRightContext());
                    TraceNewItem(sourcePosition);
                }

                while(!IsBaseLevel)
                {
                    TraceBeginPhase("end phase");
                    Step(false);
                    TraceEndPhase("end phase");
                }

                Tracer.Assert(IsBaseLevel);

                if(Current.GetRightDepth() > 0)
                    NotImplementedMethod(sourcePosition);

                return Current.Create(Left);
            }

            Item<TSourcePart> CreateStartItem(SourcePosition sourcePosition, PrioTable prioTable)
                =>
                    sourcePosition.Position == 0
                        ? Item<TSourcePart>.CreateStart
                            (sourcePosition.Source, prioTable, Parent.StartParserType)
                        : ReadNextToken(sourcePosition, prioTable.BracketContext);

            Item<TSourcePart> ReadNextToken(SourcePosition sourcePosition, BracketContext context)
            {
                TraceNextToken(sourcePosition);
                var result = Item<TSourcePart>.Create
                    (Parent.Scanner.GetNextTokenGroup(sourcePosition), context);

                var nextParser = (result.Type as ISubParserProvider)?.NextParser;
                if(nextParser == null)
                    return result;

                TraceSubParserStart(result);
                var subType = nextParser.Execute(sourcePosition, Stack);
                TraceSubParserEnd(result);

                return result.RecreateWith(newType: subType, newContext: context);
            }

            void Step(bool canPush = true)
            {
                var other = IsBaseLevel? null : Stack.Peek();

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
                        ((IBracketMatch<TSourcePart>)Current.Type).Value,
                        Current.Characters.End.Span(0),
                        other.BracketItem.LeftContext,
                        Current.GetRightContext().IsBracketAndLeftBracket("")
                    );
                else
                    Current = Current.RecreateWith(newContext: other.BracketItem.LeftContext);

                TraceMatchPhase();
            }

            void TraceRelation(PrioTable.Relation relation)
            {
                if(!Trace)
                    return;

                Tracer.Line("---- " + relation + " ----");
            }

            void TraceNextToken(SourcePosition sourcePosition)
            {
                if(!Trace)
                    return;

                Tracer.Line("\n== NextToken ====>");
                Tracer.Line(sourcePosition.GetDumpAroundCurrent(50));
            }

            void TraceNewItem(SourcePosition sourcePosition)
            {
                if(!Trace)
                    return;

                Tracer.Line(Current.Characters.GetDumpAroundCurrent(50));
                Tracer.Line(sourcePosition.GetDumpAroundCurrent(50));
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
                Tracer.Line("begin of Sub-parser  ==>");
                Tracer.Line("triggered by " + item.Characters.GetDumpAroundCurrent(50));
                Tracer.Line("======================>");
                Tracer.IndentStart();
            }

            void TraceSubParserEnd(Item<TSourcePart> item)
            {
                if(!Trace)
                    return;

                Tracer.IndentEnd();
                Tracer.Line("\n======================>");
                Tracer.Line("end of Sub-parser    ==>");
                Tracer.Line("triggered by " + item.Characters.GetDumpAroundCurrent(50));
                Tracer.Line("======================>");
            }

            static string FormatStackForTrace(Stack<OpenItem<TSourcePart>> stack)
            {
                var count = stack.Count;
                if(count == 0)
                    return "stack empty";

                const int maxLines = 5;

                var isBig = count > maxLines;
                var result =
                    stack.Take(maxLines - (isBig? 1 : 0))
                        .Select((item, i) => i + ": " + TreeDump(item))
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
    }
}