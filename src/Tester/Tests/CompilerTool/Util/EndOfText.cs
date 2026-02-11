using hw.Parser;

namespace Tester.Tests.CompilerTool.Util;

sealed class EndOfText : ParserTokenType<Syntax>, IBracketMatch<Syntax>
{
    public override string Id => PrioTable.EndOfText;

    protected override Syntax? Create(Syntax? left, IToken token, Syntax? right)
    {
        if(left != null)
            return left.RightParenthesis(Id, token, right);

        // ReSharper disable once ExpressionIsAlwaysNull
        NotImplementedMethod(left, token, right);
        return null;
    }

    sealed class Matched : ParserTokenType<Syntax>
    {
        protected override Syntax? Create(Syntax? left, IToken token, Syntax? right)
        {
            if(right == null)
            {
                var result = (ParenthesisSyntax?) left!;
                if(result.Right == null)
                    return result.Left;

                // ReSharper disable once ExpressionIsAlwaysNull
                NotImplementedMethod(left, token, right);
                return null;
            }

            NotImplementedMethod(left, token, right);
            return null;
        }

        public override string Id => "<source>";
    }

    IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
}