using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Proof
{
    sealed class Lexer : DumpableObject
    {
        readonly Match _any;
        readonly IMatch _number;
        readonly Match _whiteSpaces;

        public Lexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();
            var identifier = (alpha + alpha.Else(Match.Digit).Repeat()).Else(symbol.Repeat(1));
            _any = symbol1.Else(identifier);
            _whiteSpaces = Match.WhiteSpace.Repeat(1);
            _number = Match.Digit.Repeat(1);
        }

        internal int? Space(SourcePosn sourcePosn) => sourcePosn.Match(_whiteSpaces);
        internal int? Number(SourcePosn sourcePosn) => sourcePosn.Match(_number);
        internal int? Any(SourcePosn sourcePosn) => sourcePosn.Match(_any);
    }
}