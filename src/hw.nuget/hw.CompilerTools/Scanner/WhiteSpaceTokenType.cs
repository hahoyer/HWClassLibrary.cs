using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Scanner
{
    public sealed class WhiteSpaceTokenType : DumpableObject, IScannerTokenType
    {
        public readonly string Id;
        public WhiteSpaceTokenType(string id) {Id = id;}
        public IParserTokenFactory ParserTokenFactory { get; } = null;
        string IScannerTokenType.Id => Id;
        protected override string GetNodeDump() => Id;
    }
}