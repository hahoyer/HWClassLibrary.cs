using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Instance = new PrioParser<Syntax>
            (PrioTable, Scanner(new NestedTokenFactory()), new BeginOfText());

        NestedTokenFactory() { }

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.BracketParallels
                    (MainTokenFactory.LeftBrackets, MainTokenFactory.RightBrackets);
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected IEnumerable<TokenClass<Syntax>> GetPredefinedTokenClasses()
            => new TokenClass<Syntax>[]
            {
                SwitchToken.Instance,
                new LeftParenthesis("("),
                new RightParenthesis(")")
            };

        protected TokenClass<Syntax> GetTokenClass(string name) => new NestedToken(name);

        protected TokenClass<Syntax> GetNumber() { throw new NotImplementedException(); }
        protected TokenClass<Syntax> GetText() { throw new NotImplementedException(); }
        protected override ILexerItem[] GetClasses() { throw new NotImplementedException(); }
    }
}