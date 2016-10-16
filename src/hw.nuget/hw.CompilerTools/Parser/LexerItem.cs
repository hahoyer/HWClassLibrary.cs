using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class LexerItem : DumpableObject
    {
        public readonly IScannerTokenType ScannerTokenType;
        public readonly Func<SourcePosn, int?> Match;

        public LexerItem(IScannerTokenType scannerTokenType, Func<SourcePosn, int?> match)
        {
            ScannerTokenType = scannerTokenType;
            Match = match;
        }
    }
}