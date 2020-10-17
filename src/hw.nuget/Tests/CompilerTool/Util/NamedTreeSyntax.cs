using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util
{
    sealed class NamedTreeSyntax : TreeSyntax
    {
        readonly NamedToken TokenClass;

        public NamedTreeSyntax(Syntax left, NamedToken tokenClass, IToken part, Syntax right)
            : base(left, part, right)
        {
            TokenClass = tokenClass;
        }

        public override string TokenClassName => TokenClass.Name;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
            => new RightParenthesisSyntax(id, this, token, right);
    }
}