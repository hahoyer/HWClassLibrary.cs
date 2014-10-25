using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class List : TokenClass, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, SourcePart token, ParsedSyntax right)
        {
            if(left == null)
                return right ?? TrueSyntax.Instance;
            if(right == null)
                return left;
            return left.Associative(this, token, right);
        }

        [DisableDump]
        bool IAssociative.IsVariablesProvider { get { return true; } }

        [DisableDump]
        ParsedSyntax IAssociative.Empty { get { return TrueSyntax.Instance; } }

        string IAssociative.SmartDump(Set<ParsedSyntax> set)
        {
            var i = 0;
            var resultList = set.Aggregate("", (s, x) => s + "\n[" + i++ + "] " + SmartDump(x, false)).Indent();
            return "Clauses:" + resultList;
        }

        AssociativeSyntax IAssociative.Syntax(SourcePart token, Set<ParsedSyntax> set)
        {
            return new ClauseSyntax(this, token, set);
        }
        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) { return null; }
        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax) { return parsedSyntax is TrueSyntax; }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            NotImplementedMethod(parsedSyntax, isFirst);
            return null;
        }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return false; } }

        string SmartDump(ParsedSyntax x, bool isWatched)
        {
            var result = x.SmartDump(this);
            if(isWatched)
                result += ("\n" + x.Dump() + "\n").Indent(3);
            return result;
        }
    }
}