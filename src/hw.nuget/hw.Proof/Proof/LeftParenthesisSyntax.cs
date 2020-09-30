using hw.Parser;

namespace hw.Proof
{
    sealed class LeftParenthesisSyntax : ParsedSyntax
    {
        internal readonly int Level;
        internal readonly ParsedSyntax Right;

        public LeftParenthesisSyntax(int level, IToken token, ParsedSyntax right)
            : base(token)
        {
            Level = level;
            Right = right;
        }
    }
}