using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed partial class PrioParser<TSourcePart>
        : DumpableObject
            , IParser<TSourcePart>
        where TSourcePart : class
    {
        readonly PrioTable PrioTable;
        readonly IScanner Scanner;
        readonly IParserTokenType<TSourcePart> StartParserType;

        public PrioParser(PrioTable prioTable, IScanner scanner, IParserTokenType<TSourcePart> startParserType)
        {
            PrioTable = prioTable;
            Scanner = scanner;
            StartParserType = startParserType;
        }

        TSourcePart IParser<TSourcePart>.Execute(SourcePosition start, Stack<OpenItem<TSourcePart>> initialStack)
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

        public bool Trace { get; set; }

        PrioTable.Relation GetRelation(PrioTable.ITargetItem newType, PrioTable.ITargetItem topType)
            => PrioTable.GetRelation(newType, topType);

        PrioParserWorker CreateWorker(Stack<OpenItem<TSourcePart>> stack)
            => new PrioParserWorker(this, stack);
    }
}