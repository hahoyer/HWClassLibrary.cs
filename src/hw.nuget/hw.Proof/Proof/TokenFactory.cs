using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using hw.Proof.TokenClasses;
using hw.Scanner;

namespace hw.Proof
{
    sealed class TokenFactory : TokenFactory<TokenClass, ParsedSyntax>
    {
        TokenFactory() { }

        internal static TokenFactory Instance { get { return new TokenFactory(); } }

        protected override TokenClass GetTokenClass(string name) { return new UserSymbol(); }

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

                x = x.ParenthesisLevelLeft
                    (new[] {"(", "[", "{", PrioTable.BeginOfText}, new[] {")", "]", "}", PrioTable.EndOfText});
                //Tracer.FlaggedLine("\n"+x+"\n");
                return x;
            }
        }

        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass>
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
        protected override TokenClass GetEndOfText() { return new RightParenthesis(0); }
        protected override TokenClass GetNumber() { return new Number(); }
        protected override TokenClass GetText() { throw new NotImplementedException(); }

        internal Minus Minus { get { return (Minus) TokenClass("-"); } }
        internal Equal Equal { get { return (Equal) TokenClass("="); } }
        internal Plus Plus { get { return (Plus) TokenClass("+"); } }

        protected override TokenClass GetError(Match.IError message) { return new SyntaxError(message); }

        sealed class SyntaxError : TokenClass
        {
            readonly Match.IError _message;
            public SyntaxError(Match.IError message) { _message = message; }
        }
    }
}