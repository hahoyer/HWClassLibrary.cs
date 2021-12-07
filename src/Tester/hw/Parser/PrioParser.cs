using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser
{
    public sealed partial class PrioParser<TSourcePart>
        : DumpableObject
            , IParser<TSourcePart>
        where TSourcePart : class
    {
        PrioTable PrioTable { get; }
        IScanner Scanner { get; }
        IParserTokenType<TSourcePart> StartParserType { get; }

        public PrioParser(PrioTable prioTable, IScanner scanner, IParserTokenType<TSourcePart> startParserType)
        {
            PrioTable = prioTable;
            Scanner = scanner;
            StartParserType = startParserType;
        }

        TSourcePart IParser<TSourcePart>
            .Execute(SourcePosition start, Stack<OpenItem<TSourcePart>> initialStack)
        {
            StartMethodDump(Trace, start.GetDumpAroundCurrent(50), initialStack);
            try
            {
                return ReturnMethodDump(CreateWorker(initialStack, start, true).Execute());
            }
            finally
            {
                EndMethodDump();
            }
        }

        TSourcePart IParser<TSourcePart>.Execute(Source source)
        {
            StartMethodDump(Trace, source.GetDumpAfterCurrent(0, 50));
            try
            {
                return ReturnMethodDump(CreateWorker(null, source+0).Execute());
            }
            finally
            {
                EndMethodDump();
            }
        }

        public bool Trace { get; set; }

        PrioTable.Relation GetRelation(PrioTable.ITargetItem newType, PrioTable.ITargetItem topType)
            => PrioTable.GetRelation(newType, topType);

        PrioParserWorker CreateWorker(Stack<OpenItem<TSourcePart>> stack, SourcePosition sourcePosition, bool isSubParser = false)
            => new(this, stack, sourcePosition, isSubParser);
    }
}