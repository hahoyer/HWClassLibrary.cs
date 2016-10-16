using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : DumpableObject,Parser.CachingTokenFactory
    {
        protected static IScanner Scanner(TokenFactory t)
            => new Parser.Scanner(t);

        protected override IScannerType GetEndOfText() => new EndOfText();

        internal sealed class SyntaxError : IScannerType
        {
            [EnableDump]
            readonly IssueId IssueId;
            public SyntaxError(IssueId issueId) { IssueId = issueId; }
            public override string Id => "<error>";

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
                => new ErrorSyntax(left, token, right, IssueId);

            public IParserTokenFactory ParserTokenFactory { get { throw new NotImplementedException(); } }
        }

        protected override IScannerType GetInvalidCharacterError()
            => new SyntaxError(IssueId.UnexpectedSyntaxError);
    }
}