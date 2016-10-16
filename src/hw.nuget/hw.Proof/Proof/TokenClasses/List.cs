using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class List : ParserTokenType,
        IAssociative,
        ISmartDumpToken
    {
        public List(string id) { Id = id; }

        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null)
                return right ?? TrueSyntax.Instance;
            if(right == null)
                return left;

            return left.Associative(this, token, right);
        }

        protected override string Id { get; }

        [DisableDump]
        bool IAssociative.IsVariablesProvider => true;

        [DisableDump]
        ParsedSyntax IAssociative.Empty => TrueSyntax.Instance;

        string IAssociative.SmartDump(Set<ParsedSyntax> set)
        {
            var i = 0;
            var resultList =
                set.Aggregate("", (s, x) => s + "\n[" + i++ + "] " + SmartDump(x, false)).Indent();
            return "Clauses:" + resultList;
        }

        AssociativeSyntax IAssociative.Syntax(IToken token, Set<ParsedSyntax> set)
            => new ClauseSyntax(this, token, set);

        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) => null;
        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax) => parsedSyntax is TrueSyntax;

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            NotImplementedMethod(parsedSyntax, isFirst);
            return null;
        }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation => false;

        string SmartDump(ParsedSyntax x, bool isWatched)
        {
            var result = x.SmartDump(this);
            if(isWatched)
                result += ("\n" + x.Dump() + "\n").Indent(3);
            return result;
        }
    }
}