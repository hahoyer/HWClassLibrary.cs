using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal sealed class Plus : TokenClass, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);
            return left.Associative(this, token, right);
        }

        bool IAssociative.IsVariablesProvider { get { return true; } }

        [DisableDump]
        ParsedSyntax IAssociative.Empty { get { return new NumberSyntax(0); } }

        string IAssociative.SmartDump(Set<ParsedSyntax> set) { return SmartDump(this, set); }
        AssociativeSyntax IAssociative.Syntax(TokenData token, Set<ParsedSyntax> set) { return new PlusSyntax(this, token, set); }
        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) { return left.CombineForPlus(right); }

        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax)
        {
            var numberSyntax = parsedSyntax as NumberSyntax;
            return numberSyntax != null && numberSyntax.Value == 0;
        }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return true; } }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            if(parsedSyntax.IsNegative)
                return (isFirst ? "" : " ") + "- ";
            return isFirst ? "" : " + ";
        }
    }
}