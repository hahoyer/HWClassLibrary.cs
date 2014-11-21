using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using hw.Proof.TokenClasses;
using hw.Scanner;

namespace hw.Proof
{
    sealed class TokenFactory : TokenFactory<TokenClasses.TokenClass , ParsedSyntax>
    {
        TokenFactory() { }

        internal static TokenFactory Instance { get { return new TokenFactory(); } }

        protected override TokenClasses.TokenClass GetTokenClass(string name) { return new UserSymbol(); }

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

                x = x.ParenthesisLevelLeft(new[] {"(", "[", "{", PrioTable.BeginOfText}, new[] {")", "]", "}", PrioTable.EndOfText});
                //Tracer.FlaggedLine("\n"+x+"\n");
                return x;
            }
        }

        protected override FunctionCache<string, TokenClasses.TokenClass> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClasses.TokenClass>
            {
                {"{", new LeftParenthesis(1)},
                {"[", new LeftParenthesis(2)},
                {"(", new LeftParenthesis(3)},
                {"}", new RightParenthesis(1)},
                {"]", new RightParenthesis(2)},
                {")", new RightParenthesis(3)},
                {",", new List()},
                {";", new List()},
                {"=", new Equal()},
                {"-", new Minus()},
                {"&", new And()},
                {"+", new Plus()},
                {"^", new Caret()},
                {"Integer", new Integer()},
                {"gcd", new GreatesCommonDenominator()},
                {"elem", new Element()}
            };
            return result;
        }
        protected override TokenClasses.TokenClass GetEndOfText() { return new RightParenthesis(0); }
        protected override TokenClasses.TokenClass GetNumber() { return new Number(); }

        internal Minus Minus { get { return (Minus) TokenClass("-"); } }
        internal Equal Equal { get { return (Equal) TokenClass("="); } }
        internal Plus Plus { get { return (Plus) TokenClass("+"); } }

        protected override TokenClass GetSyntaxError(Match.IError message) { return new SyntaxError(message); }

        sealed class SyntaxError : TokenClasses.TokenClass
        {
            readonly Match.IError _message;
            public SyntaxError(Match.IError message) { _message = message; }
        }
    }
}