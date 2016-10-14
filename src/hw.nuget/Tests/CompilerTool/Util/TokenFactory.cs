using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : Parser.TokenFactory
    {
        protected static IScanner Scanner(TokenFactory t)
            => new Parser.Scanner(t);

        protected override IScannerType GetEndOfText() => new EndOfText();

        internal sealed class SyntaxError : TokenClass<Syntax>
        {
            [EnableDump]
            readonly IssueId IssueId;
            public SyntaxError(IssueId issueId) { IssueId = issueId; }
            public override string Id => "<error>";

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
                => new ErrorSyntax(left, token, right, IssueId);
        }

        protected override IScannerType GetInvalidCharacterError()
            => new SyntaxError(IssueId.UnexpectedSyntaxError);
    }
}