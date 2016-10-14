using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class MainTokenFactory : TokenFactory
    {
        public static readonly MainTokenFactory Instance = new MainTokenFactory();
        public static readonly IParser<Syntax> ParserInstance = new PrioParser<Syntax>
            (PrioTable, Scanner(Instance), new BeginOfText());

        MainTokenFactory() { }

        internal static string[] RightBrackets => new[] {")", "}", PrioTable.EndOfText};
        internal static string[] LeftBrackets => new[] {"(", "{", PrioTable.BeginOfText};

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.Left("*");
                x += PrioTable.Left("+");
                x += PrioTable.Left(";");
                x += PrioTable.BracketParallels(LeftBrackets, RightBrackets);
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected IEnumerable<TokenClass<Syntax>> GetPredefinedTokenClasses()
            => new TokenClass<Syntax>[]
            {
                new SwitchToken(),
                new LeftParenthesis("("),
                new RightParenthesis(")"),
                new LeftParenthesis("{"),
                new RightParenthesis("}")
            };

        protected TokenClass<Syntax> GetTokenClass(string name) => new MainToken(name);

        protected TokenClass<Syntax> GetNumber() { throw new NotImplementedException(); }
        protected TokenClass<Syntax> GetText() { throw new NotImplementedException(); }

        internal TokenClass<Syntax> CreateSyntaxError(IssueId issue) => new SyntaxError(issue);
        protected override ILexerItem[] GetClasses() { throw new NotImplementedException(); }
    }
}