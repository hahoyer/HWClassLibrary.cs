using hw.DebugFormatter;
using JetBrains.Annotations;

namespace hw.Scanner
{
    [PublicAPI]
    public sealed class WhiteSpaceTokenType
        : DumpableObject
            , IScannerTokenType
    {
        public readonly string Id;
        public WhiteSpaceTokenType(string id) => Id = id;
        string IScannerTokenType.Id => Id;
        public IParserTokenFactory ParserTokenFactory { get; }
        protected override string GetNodeDump() => Id;
    }
}