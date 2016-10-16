using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Plus : ParserTokenType, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);
            return left.Associative(this, token, right);
        }

        protected override string Id => "+";

        bool IAssociative.IsVariablesProvider => true;

        [DisableDump]
        ParsedSyntax IAssociative.Empty => new NumberSyntax(0);

        string IAssociative.SmartDump(Set<ParsedSyntax> set) => SmartDump(this, set);

        AssociativeSyntax IAssociative.Syntax(IToken token, Set<ParsedSyntax> set)
            => new PlusSyntax(this, token, set);

        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right)
            => left.CombineForPlus(right);

        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax)
        {
            var numberSyntax = parsedSyntax as NumberSyntax;
            return numberSyntax != null && numberSyntax.Value == 0;
        }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation => true;

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            if(parsedSyntax.IsNegative)
                return (isFirst ? "" : " ") + "- ";

            return isFirst ? "" : " + ";
        }
    }
}