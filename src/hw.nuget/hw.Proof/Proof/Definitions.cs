using hw.DebugFormatter;
using hw.Parser;
using hw.Proof.TokenClasses;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class Definitions : DumpableObject
    {
        internal static Definitions Instance => new();

        static readonly Lexer Lexer = new();
        internal static readonly string[] LeftBrackets = ["(", "[", "{", PrioTable.BeginOfText];
        internal static readonly string[] RightBrackets = [")", "]", "}", PrioTable.EndOfText];
        internal static readonly Equal Equal = new();
        internal static readonly Plus Plus = new();

        internal static UserSymbol GetTokenClass(string name) => new(name);

        internal static IParserTokenType<ParsedSyntax>[] PredefinedTokenClasses
            =>
            [
                new LeftParenthesis(1),
                new LeftParenthesis(2),
                new LeftParenthesis(3),
                new RightParenthesis(1),
                new RightParenthesis(2),
                new RightParenthesis(3),
                new List(","),
                new List(";"),
                Equal,
                new Minus(),
                new And(),
                Plus,
                new Caret(),
                new Integer(),
                new GreatesCommonDenominator(),
                new Element()
            ];

        internal static PrioTable PrioTable
        {
            get
            {
                var target = PrioTable.Left(PrioTable.Any);

                target += PrioTable.Right("^");
                target += PrioTable.Left("*", "/", "\\", "gcd");
                target += PrioTable.Left("+", "-");

                target += PrioTable.Left("<", ">", "<=", ">=");
                target += PrioTable.Left("=", "<>");

                target += PrioTable.Left("elem");
                target += PrioTable.Left("&");
                target += PrioTable.Left("|");

                target += PrioTable.Right(",");
                target += PrioTable.Right(";");

                target += PrioTable.BracketParallels(LeftBrackets, RightBrackets);

                //Tracer.FlaggedLine("\n"+target+"\n");
                return target;
            }
        }


        sealed class TokenFactory : DumpableObject, ITokenFactory<ParsedSyntax>
        {
            IParserTokenType<ParsedSyntax> ITokenFactory<ParsedSyntax>.BeginOfText => new LeftParenthesis(0);
            IScannerTokenType ITokenFactory.EndOfText => new RightParenthesis(0);
            IScannerTokenType ITokenFactory.InvalidCharacterError
                => new SyntaxError(IssueId.UnexpectedSyntaxError);
            LexerItem[] ITokenFactory.Classes
                =>
                [
                    new(new WhiteSpaceTokenType("Space"), Lexer.Space),
                    new(new Number(), Lexer.Number),
                    new(new Any(), Lexer.Any)
                ];
        }

        internal readonly ITokenFactory<ParsedSyntax> ScannerTokenFactory
            = new CachingTokenFactory<ParsedSyntax>(new TokenFactory());

        Definitions() { }
    }
}