using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class And : ParserTokenType, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return left.Associative(this, token, right);
        }

        protected override string Id => "&";

        [DisableDump]
        bool IAssociative.IsVariablesProvider => true;

        [DisableDump]
        ParsedSyntax IAssociative.Empty => TrueSyntax.Instance;

        AssociativeSyntax IAssociative.Syntax(IToken token, Set<ParsedSyntax> x)
            => new AndSyntax(this, token, x);

        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) => null;
        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax) => parsedSyntax is TrueSyntax;
        string IAssociative.SmartDump(Set<ParsedSyntax> set) => SmartDump(this, set);

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
            => isFirst ? "" : " & ";

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation => false;
    }
}