using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class RightParenthesis
        : ParserTokenType<Syntax>
            , IBracketMatch<Syntax>
    {
        sealed class Matched : ParserTokenType<Syntax>
        {
            public override string Id => "()";

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
                => right == null? left : new NamelessSyntax(left, token, right);
        }

        public override string Id { get; }
        public RightParenthesis(string id) => Id = id;

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            if(left != null)
                return left.RightParenthesis(Id, token, right);

            // ReSharper disable once ExpressionIsAlwaysNull
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}