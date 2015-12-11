using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class List : TokenClass, IAssociative, ISmartDumpToken
    {
        readonly string _value;

        public List(string value) { _value = value; }
        
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null)
                return right ?? TrueSyntax.Instance;
            if(right == null)
                return left;
            return left.Associative(this, token, right);
        }
        
        public override string Value { get { return _value; } }

        [DisableDump]
        bool IAssociative.IsVariablesProvider { get { return true; } }

        [DisableDump]
        ParsedSyntax IAssociative.Empty { get { return TrueSyntax.Instance; } }

        string IAssociative.SmartDump(Set<ParsedSyntax> set)
        {
            var i = 0;
            var resultList =
                set.Aggregate("", (s, x) => s + "\n[" + i++ + "] " + SmartDump(x, false)).Indent();
            return "Clauses:" + resultList;
        }

        AssociativeSyntax IAssociative.Syntax(IToken token, Set<ParsedSyntax> set)
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