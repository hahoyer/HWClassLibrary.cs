using hw.DebugFormatter;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

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
        IParserTokenFactory IScannerTokenType.ParserTokenFactory => null;
        protected override string GetNodeDump() => Id;
    }
}