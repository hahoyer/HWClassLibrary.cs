using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    public sealed class WhiteSpaceTokenType : DumpableObject, IScannerTokenType
    {
        public IParserTokenFactory ParserTokenFactory { get; } = null;
    }
}