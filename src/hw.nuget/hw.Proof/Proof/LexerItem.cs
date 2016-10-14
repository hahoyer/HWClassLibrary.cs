using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class LexerItem : DumpableObject, ILexerItem
    {
        readonly IScannerType ScannerType;
        readonly Func<SourcePosn, int?> MatchFunction;

        public LexerItem(IScannerType scannerType, Func<SourcePosn, int?> matchFunction)
        {
            ScannerType = scannerType;
            MatchFunction = matchFunction;
        }

        int? ILexerItem.Match(SourcePosn sourcePosn) => MatchFunction(sourcePosn);
        IScannerType ILexerItem.ScannerType => ScannerType;
    }
}