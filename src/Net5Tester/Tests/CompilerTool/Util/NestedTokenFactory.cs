﻿using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using Net5Tester.Tests.CompilerTool.Util;

// ReSharper disable CheckNamespace

namespace Net5Tester.CompilerTool.Util
{
    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Parser =
            new NestedTokenFactory().ParserInstance;

        NestedTokenFactory() { }

        protected override PrioTable PrioTable
        {
            get
            {
                var target = PrioTable.Left(PrioTable.Any);
                target += PrioTable.BracketParallels
                    (LeftBrackets, RightBrackets);
                Tracer.FlaggedLine("\n" + target.Dump() + "\n");
                target.Title = Tracer.MethodHeader();
                return target;
            }
        }

        internal override IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses
            => new ParserTokenType<Syntax>[]
            {
                SwitchToken.Instance,
                new LeftParenthesis("("),
                new RightParenthesis(")")
            };

        internal override ParserTokenType<Syntax> GetTokenClass(string name) => new NestedToken(name);
    }
}