using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Scanner
{
    public interface IScanner
    {
        IItem[] GetNextTokenGroup(SourcePosn sourcePosn);
    }

    public interface IItem
    {
        IScannerTokenType ScannerTokenType { get; }
        SourcePart SourcePart { get; }
    }

    public interface IScannerTokenType
    {
        IParserTokenFactory ParserTokenFactory { get; }
    }

    public interface IParserTokenFactory
    {
        IParserTokenType<TTreeItem> GetTokenType<TTreeItem>(string id)
            where TTreeItem : class, ISourcePart;
    }
}