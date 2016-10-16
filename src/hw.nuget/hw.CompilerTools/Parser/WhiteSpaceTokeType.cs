using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Parser
{
    public sealed class WhiteSpaceTokeType : DumpableObject, IScannerType
    {
        public IParserTokenFactory ParserTokenFactory { get; } = null;
    }
}