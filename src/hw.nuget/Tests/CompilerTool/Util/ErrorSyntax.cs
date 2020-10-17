using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util
{
    sealed class ErrorSyntax : TreeSyntax
    {
        readonly IssueId Message;

        public ErrorSyntax(Syntax left, IToken token, Syntax right, IssueId message)
            : base(left, token, right) { Message = message; }

        public override string TokenClassName => "?" + Message;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }
}