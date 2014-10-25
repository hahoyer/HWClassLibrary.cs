using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class And : TokenClass, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, SourcePart token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);

            return left.Associative(this, token, right);
        }

        [DisableDump]
        bool IAssociative.IsVariablesProvider { get { return true; } }

        [DisableDump]
        ParsedSyntax IAssociative.Empty { get { return TrueSyntax.Instance; } }

        AssociativeSyntax IAssociative.Syntax(SourcePart token, Set<ParsedSyntax> x) { return new AndSyntax(this, token, x); }
        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) { return null; }
        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax) { return parsedSyntax is TrueSyntax; }
        string IAssociative.SmartDump(Set<ParsedSyntax> set) { return SmartDump(this, set); }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst) { return isFirst ? "" : " & "; }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return false; } }
    }
}