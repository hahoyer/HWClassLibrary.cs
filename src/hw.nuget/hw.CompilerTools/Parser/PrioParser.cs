using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed partial class PrioParser<TTreeItem> : DumpableObject, IParser<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        readonly PrioTable PrioTable;
        readonly IScanner<TTreeItem> Scanner;
        readonly IType<TTreeItem> StartType;
        public bool Trace { get; set; }

        public PrioParser(PrioTable prioTable, IScanner<TTreeItem> scanner, IType<TTreeItem> startType)
        {
            PrioTable = prioTable;
            Scanner = scanner;
            StartType = startType;
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
            => PrioTable.GetRelation(newType, topType);

        PrioParserWorker CreateWorker(Stack<OpenItem<TTreeItem>> stack)
            => new PrioParserWorker(this, stack);
    }
}