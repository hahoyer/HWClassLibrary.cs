﻿using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace Net5Tester.CompilerTool.Util
{
    sealed class MainTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Parser
            = new MainTokenFactory().ParserInstance;

        MainTokenFactory() { }

        protected override PrioTable PrioTable
        {
            get
            {
                var target = PrioTable.Left(PrioTable.Any);
                target += PrioTable.Left("*");
                target += PrioTable.Left("+");
                target += PrioTable.Left(";");
                target += PrioTable.BracketParallels(LeftBrackets, RightBrackets);
                Tracer.FlaggedLine("\n" + target.Dump() + "\n");
                target.Title = Tracer.MethodHeader();
                return target;
            }
        }

        internal override IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses
            => new ParserTokenType<Syntax>[]
            {
                new SwitchToken(),
                new LeftParenthesis("("),
                new RightParenthesis(")"),
                new LeftParenthesis("{"),
                new RightParenthesis("}")
            };

        internal override ParserTokenType<Syntax> GetTokenClass(string name) => new MainToken(name);
    }
}