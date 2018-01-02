using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : DumpableObject, ITokenFactory<Syntax>
    {
        internal static string[] RightBrackets => new[] {")", "}", PrioTable.EndOfText};
        internal static string[] LeftBrackets => new[] {"(", "{", PrioTable.BeginOfText};

        readonly Lexer Lexer = new Lexer();
        readonly BeginOfText StartParserType = new BeginOfText();

        IParserTokenType<Syntax> ITokenFactory<Syntax>.BeginOfText => StartParserType;
        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();
        LexerItem[] ITokenFactory.Classes => Classes;

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new SyntaxError(IssueId.UnexpectedSyntaxError);

        protected LexerItem[] Classes => new[]
        {
            new LexerItem(new WhiteSpaceTokenType("Space"), Lexer.Space),
            new LexerItem(new WhiteSpaceTokenType("Comment"), Lexer.Comment),
            new LexerItem(new Any(this), Lexer.Any)
        };

        internal abstract IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses {get;}

        internal PrioParser<Syntax> ParserInstance
            => new PrioParser<Syntax>
            (
                PrioTable,
                new TwoLayerScanner(new CachingTokenFactory<Syntax>(this)),
                StartParserType
            );

        protected abstract PrioTable PrioTable {get;}

        internal abstract ParserTokenType<Syntax> GetTokenClass(string name);
    }

    sealed class Any : PredefinedTokenFactory<Syntax>
    {
        readonly TokenFactory Parent;
        public Any(TokenFactory parent) => Parent = parent;

        protected override IParserTokenType<Syntax> GetTokenClass(string name)
            => Parent.GetTokenClass(name);

        protected override IEnumerable<IParserTokenType<Syntax>> GetPredefinedTokenClasses()
            => Parent.PredefinedTokenClasses;
    }
}