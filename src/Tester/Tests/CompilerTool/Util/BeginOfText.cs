using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool.Util
{
    sealed class BeginOfText : ParserTokenType<Syntax>
    {
        public override string Id => PrioTable.BeginOfText;

        protected override Syntax Create(Syntax? left, IToken token, Syntax? right)
            => new LeftParenthesisSyntax(left, token, right);
    }
}