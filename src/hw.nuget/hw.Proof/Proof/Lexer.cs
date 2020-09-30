using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Proof
{
    sealed class Lexer : DumpableObject
    {
        readonly Match AnyMatch;
        readonly IMatch NumberMatch;
        readonly Match WhiteSpaces;

        public Lexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();
            var identifier = (alpha + alpha.Else(Match.Digit).Repeat()).Else(symbol.Repeat(1));
            AnyMatch = symbol1.Else(identifier);
            WhiteSpaces = Match.WhiteSpace.Repeat(1);
            NumberMatch = Match.Digit.Repeat(1);
        }

        internal int? Space(SourcePosition sourcePosition) => sourcePosition.Match(WhiteSpaces);
        internal int? Number(SourcePosition sourcePosition) => sourcePosition.Match(NumberMatch);
        internal int? Any(SourcePosition sourcePosition) => sourcePosition.Match(AnyMatch);
    }
}