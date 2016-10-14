using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IScanner
    {
        IItem[] GetNextTokenGroup(SourcePosn sourcePosn);
    }

    public interface IItem
    {
        IScannerType ScannerType { get; }
        SourcePart SourcePart { get; }
    }

    public interface IScannerType
    {
    }

    public interface ILexerItem
    {
        int? Match(SourcePosn sourcePosn);
        IScannerType ScannerType { get; }
    }
}