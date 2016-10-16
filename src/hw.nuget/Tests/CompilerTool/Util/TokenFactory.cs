using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : DumpableObject, ITokenFactory
    {
        internal static string[] RightBrackets => new[] {")", "}", PrioTable.EndOfText};
        internal static string[] LeftBrackets => new[] {"(", "{", PrioTable.BeginOfText};

        readonly Lexer Lexer = new Lexer();

        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();
        LexerItem[] ITokenFactory.Classes => Classes;

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new SyntaxError(IssueId.UnexpectedSyntaxError);

        protected LexerItem[] Classes => new[]
        {
            new LexerItem(new WhiteSpaceTokenType(), Lexer.WhiteSpace),
            new LexerItem(new WhiteSpaceTokenType(), Lexer.Comment),
            new LexerItem(new Any(this), Lexer.Any)
        };

        internal abstract ParserTokenType<Syntax> GetTokenClass(string name);
        internal abstract IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses { get; }

        internal PrioParser<Syntax> ParserInstance
            => new PrioParser<Syntax>
            (
                PrioTable,
                new Parser.Scanner(new CachingTokenFactory(this)),
                new BeginOfText()
            );

        protected abstract PrioTable PrioTable { get; }
    }

    sealed class Any : PredefinedTokenFactory<Syntax>
    {
        readonly TokenFactory Parent;
        public Any(TokenFactory parent) { Parent = parent; }

        protected override IParserTokenType<Syntax> GetTokenClass(string name)
            => Parent.GetTokenClass(name);

        protected override IEnumerable<IParserTokenType<Syntax>> GetPredefinedTokenClasses()
            => Parent.PredefinedTokenClasses;
    }
}