using System.Diagnostics;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util
{
    [DebuggerDisplay(value: "{NodeDump}")]
    sealed class LeftParenthesisSyntax : TreeSyntax
    {
        public LeftParenthesisSyntax
            (Syntax? left, IToken part, Syntax? right)
            : base(left, part, right)
        {
        }

        public override string TokenClassName => "?(?";

        public override Syntax RightParenthesis(string id, IToken token, Syntax? right)
        {
            if(right == null &&
               Left == null)
                return new ParenthesisSyntax(Token, Right, token);

            NotImplementedMethod(id, token, right);
            return null!;
        }
    }
}