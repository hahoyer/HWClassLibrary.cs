using hw.DebugFormatter;
using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Parser;

public sealed partial class PrioParser<TParserResult>
(
    PrioTable prioTable
    , IScanner scanner
    , IParserTokenType<TParserResult> startParserType
)
    : DumpableObject
        , IParser<TParserResult>
    where TParserResult : class
{
    PrioTable PrioTable { get; } = prioTable;
    IScanner Scanner { get; } = scanner;
    IParserTokenType<TParserResult> StartParserType { get; } = startParserType;

    TParserResult? IParser<TParserResult>
        .Execute(SourcePosition start, Stack<OpenItem<TParserResult>>? initialStack)
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

    TParserResult? IParser<TParserResult>.Execute(Source source)
    {
        StartMethodDump(Trace, source.GetDumpAfterCurrent(0, 50));
        try
        {
            return ReturnMethodDump(CreateWorker(null, source + 0).Execute());
        }
        finally
        {
            EndMethodDump();
        }
    }

    public bool Trace { get; set; }

    PrioTable.Relation GetRelation(PrioTable.ITargetItem newType, PrioTable.ITargetItem topType)
        => PrioTable.GetRelation(newType, topType);

    PrioParserWorker CreateWorker
        (Stack<OpenItem<TParserResult>>? stack, SourcePosition sourcePosition, bool isSubParser = false)
        => new(this, stack, sourcePosition, isSubParser);
}