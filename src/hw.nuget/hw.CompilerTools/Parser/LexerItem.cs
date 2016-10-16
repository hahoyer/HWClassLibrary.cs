using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class LexerItem : DumpableObject
    {
        public readonly IScannerType ScannerType;
        public readonly Func<SourcePosn, int?> Match;

        public LexerItem(IScannerType scannerType, Func<SourcePosn, int?> match)
        {
            ScannerType = scannerType;
            Match = match;
        }
    }
}