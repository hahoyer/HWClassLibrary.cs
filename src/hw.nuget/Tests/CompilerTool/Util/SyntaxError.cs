using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class SyntaxError : ParserTokenType<Syntax>
    {
        [EnableDump]
        readonly IssueId IssueId;
        public SyntaxError(IssueId issueId) { IssueId = issueId; }
        public override string Id => PrioTable.Error;

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => new ErrorSyntax(left, token, right, IssueId);
    }
}