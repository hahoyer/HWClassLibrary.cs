using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class SyntaxError : ParserTokenType<ParsedSyntax>
    {
        [EnableDump]
        readonly IssueId IssueId;
        public SyntaxError(IssueId issueId) { IssueId = issueId; }

        protected override ParsedSyntax Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return default!;
        }

        public override string Id => PrioTable.Error;
    }
}