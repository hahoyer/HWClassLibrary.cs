using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Proof.TokenClasses;

namespace hw.Proof
{
    sealed class TokenFactory : Parser.TokenFactory
    {
        public static readonly string[] LeftBrackets = {"(", "[", "{", PrioTable.BeginOfText};
        public static readonly string[] RightBrackets = {")", "]", "}", PrioTable.EndOfText};
        static readonly ReniLexer Lexer = new ReniLexer();

        TokenFactory() { }

        internal static TokenFactory Instance => new TokenFactory();

        protected static TokenClass GetTokenClass(string name) => new UserSymbol(name);

        internal static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);

                x += PrioTable.Right("^");
                x += PrioTable.Left("*", "/", "\\", "gcd");
                x += PrioTable.Left("+", "-");

                x += PrioTable.Left("<", ">", "<=", ">=");
                x += PrioTable.Left("=", "<>");

                x += PrioTable.Left("elem");
                x += PrioTable.Left("&");
                x += PrioTable.Left("|");

                x += PrioTable.Right(",");
                x += PrioTable.Right(";");

                x += PrioTable.BracketParallels(LeftBrackets, RightBrackets);

                //Tracer.FlaggedLine("\n"+x+"\n");
                return x;
            }
        }

        protected override IScannerType GetInvalidCharacterError()
            => CreateSyntaxError(IssueId.UnexpectedSyntaxError);

        protected IEnumerable<TokenClass> GetPredefinedTokenClasses() => new TokenClass[]
        {
            new LeftParenthesis(1),
            new LeftParenthesis(2),
            new LeftParenthesis(3),
            new RightParenthesis(1),
            new RightParenthesis(2),
            new RightParenthesis(3),
            new List(","),
            new List(";"),
            new Equal(),
            new Minus(),
            new And(),
            new Plus(),
            new Caret(),
            new Integer(),
            new GreatesCommonDenominator(),
            new Element()
        };

        protected override IScannerType GetEndOfText() => new RightParenthesis(0);

        protected override ILexerItem[] GetClasses() => new ILexerItem[]
        {
            new LexerItem(new WhiteSpace(), Lexer.WhiteSpace),
            new LexerItem(new Number(), Lexer.Number),
            new LexerItem(new Any(), Lexer.Any)
        };

        internal Minus Minus => (Minus) GetTokenClass("-");
        internal Equal Equal => (Equal) GetTokenClass("=");
        internal Plus Plus => (Plus) GetTokenClass("+");

        sealed class SyntaxError : TokenClass
        {
            readonly IssueId IssueId;

            public SyntaxError(IssueId issueId) { IssueId = issueId; }
            public override string Value => IssueId.ToString();
        }

        internal TokenClass CreateSyntaxError(IssueId issue) => new SyntaxError(issue);
    }

    sealed class Any : DumpableObject, IScannerType, IParserType<ParsedSyntax>
    {
        bool IScannerType.IsGroupToken => true;
        ParsedSyntax IParserType<ParsedSyntax>.Create(ParsedSyntax left, IToken token, ParsedSyntax right) { throw new NotImplementedException(); }
        string IParserType<ParsedSyntax>.PrioTableId { get { throw new NotImplementedException(); } }
    }

    sealed class WhiteSpace : DumpableObject, IScannerType
    {
        bool IScannerType.IsGroupToken => false;
    }
}