﻿using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Parser;

[PublicAPI]
public sealed partial class PrioParser<TSourcePart>
{
    public interface ISubParserProvider
    {
        ISubParser<TSourcePart> NextParser { get; }
    }

    sealed class PrioParserWorker : DumpableObject
    {
        SourcePosition SourcePosition { get; }
        PrioParser<TSourcePart> Parent { get; }
        Stack<OpenItem<TSourcePart>> Stack { get; }
        int StartLevel { get; }
        bool IsSubParser { get; }
        Item<TSourcePart> Current { get; set; }
        TSourcePart Left { get; set; }

        public PrioParserWorker
        (
            PrioParser<TSourcePart> parent, Stack<OpenItem<TSourcePart>> stack, SourcePosition sourcePosition
            , bool isSubParser
        )
        {
            SourcePosition = sourcePosition;
            IsSubParser = isSubParser;
            (IsSubParser || SourcePosition.Position == 0).Assert();
            Parent = parent;
            Stack = stack ?? new Stack<OpenItem<TSourcePart>>();
            StartLevel = Stack.Count;
            if(Trace)
                (Parent.PrioTable.Title ?? "").Log();
        }

        bool Trace => Parent.Trace;

        bool IsBaseLevel => Stack.Count == StartLevel;

        public TSourcePart Execute()
        {
            Current = CreateStartItem(SourcePosition);
            TraceNewItem(SourcePosition);

            while(BracketContext.GetRightDepth(Current) > 0)
            {
                Left = null;
                do
                {
                    TraceBeginPhase("inner loop");
                    Step();
                    TraceEndPhase("inner loop");
                }
                while(Left != null);

                Current = ReadNextToken(SourcePosition, BracketContext.GetRightContext(Current));
                TraceNewItem(SourcePosition);
            }

            while(!IsBaseLevel)
            {
                TraceBeginPhase("end phase");
                Step(false);
                TraceEndPhase("end phase");
            }

            IsBaseLevel.Assert();

            if(BracketContext.GetRightDepth(Current) > 0)
                NotImplementedMethod(SourcePosition);

            return Current.Create(Left);
        }

        Item<TSourcePart> CreateStartItem(SourcePosition sourcePosition)
        {
            var bracketContext = Parent.PrioTable.BracketContext;
            if(IsSubParser)
                return ReadNextToken(sourcePosition, bracketContext);

            (sourcePosition.Position == 0).Assert();
            return Item<TSourcePart>.CreateStart(sourcePosition.Source, Parent.StartParserType, bracketContext);
        }

        Item<TSourcePart> ReadNextToken(SourcePosition sourcePosition, BracketContext context)
        {
            TraceNextToken(sourcePosition);
            var nextTokenGroup = Parent.Scanner.GetNextTokenGroup(sourcePosition);
            var result = Item<TSourcePart>
                .Create(nextTokenGroup, sourcePosition, context, IsSubParser);

            // ReSharper disable once SuspiciousTypeConversion.Global
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
                (!relation.IsPush).Assert();

            if(relation.IsPull)
                Left = Stack.Pop().Create(Left);
            if(relation.IsPush)
            {
                Stack.Push(new(Left, Current));
                Left = null;
            }

            if(!relation.IsBracket)
                return;

            if(relation.IsMatch)
                Left = Current.Create(Left);

            (other != null).Assert();

            Current = relation.IsMatch
                ? Current.CreateMatch(other)
                : Current.RecreateWith(newContext: other.BracketItem.LeftContext);

            TraceMatchPhase();
        }

        void TraceRelation(PrioTable.Relation relation)
        {
            if(!Trace)
                return;

            ("---- " + relation + " ----").Log();
        }

        void TraceNextToken(SourcePosition sourcePosition)
        {
            if(!Trace)
                return;

            "\n== NextToken ====>".Log();
            sourcePosition.GetDumpAroundCurrent(50).Log();
        }

        void TraceNewItem(SourcePosition sourcePosition)
        {
            if(!Trace)
                return;

            Current.Characters.GetDumpAroundCurrent(50).Log();
            sourcePosition.GetDumpAroundCurrent(50).Log();
            ("Depth = " + Current.Depth).Log();
            "=================>".Log();
        }

        void TraceMatchPhase()
        {
            if(!Trace)
                return;

            "\n======================>".Log();
            "bracket matching======>".Log();
            "======================>".Log();
            Tracer.IndentStart();
            TraceItemLine(nameof(Current), Current);
            (nameof(Left) + " = " + Extension.TreeDump(Left)).Log();
            FormatStackForTrace(Stack).Log();
            Tracer.IndentEnd();
        }

        void TraceBeginPhase(string tag)
        {
            if(!Trace)
                return;

            Tracer.IndentStart();
            "\n======================>".Log();
            ("begin of " + tag + " ==>").Log();
            "======================>".Log();
            Tracer.IndentStart();
            TraceItemLine(nameof(Current), Current);
            (nameof(Left) + " = " + Extension.TreeDump(Left)).Log();
            FormatStackForTrace(Stack).Log();
            Tracer.IndentEnd();
        }

        void TraceEndPhase(string tag)
        {
            if(!Trace)
                return;

            Tracer.IndentStart();
            TraceItemLine(nameof(Current), Current);
            (nameof(Left) + " = " + Extension.TreeDump(Left)).Log();
            FormatStackForTrace(Stack).Log();
            if(StartLevel > Stack.Count)
                "*** End reached ***".Log();
            Tracer.IndentEnd();
            "\n<======================".Log();
            ("end of " + tag + " <==").Log();
            "<======================\n".Log();
            Tracer.IndentEnd();
        }

        void TraceSubParserStart(Item<TSourcePart> item)
        {
            if(!Trace)
                return;

            "\n======================>".Log();
            "begin of Sub-parser  ==>".Log();
            ("triggered by " + item.Characters.GetDumpAroundCurrent(50)).Log();
            "======================>".Log();
            Tracer.IndentStart();
        }

        void TraceSubParserEnd(Item<TSourcePart> item)
        {
            if(!Trace)
                return;

            Tracer.IndentEnd();
            "\n======================>".Log();
            "end of Sub-parser    ==>".Log();
            ("triggered by " + item.Characters.GetDumpAroundCurrent(50)).Log();
            "======================>".Log();
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
            return $"stack: {stack.Count} items{("\n" + result).Indent()}";
        }

        void TraceItemLine(string title, Item<TSourcePart> item)
        {
            if(!Trace)
                return;

            if(item == null)
            {
                (title + " = null").Log();
                return;
            }

            var typeDump = $"{item.Type.PrioTableId} Type = {item.Type.GetType().PrettyName()}";
            $"{title} = {typeDump} Depth={item.Context.Depth}".Log();
        }

        static string TreeDump(OpenItem<TSourcePart> value)
            => Extension.TreeDump(value.Left) +
                " " +
                (value.Type == null
                    ? "null"
                    : $"{value.Type.PrioTableId} NextDepth={value.NextDepth}");
    }
}