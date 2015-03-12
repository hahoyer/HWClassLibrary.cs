using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Element : PairToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);
            return new ElementSyntax(this, left, token, right);
        }
        public override string Id { get { return "elem"; } }
    }

    sealed class ElementSyntax : PairSyntax
    {
        public ElementSyntax(IPair @operator, ParsedSyntax left, IToken token, ParsedSyntax right)
            : base(@operator, left, token, right) {}

        protected override ParsedSyntax IsolateClause(string variable) { return null; }
        internal override Set<ParsedSyntax> Replace
            (IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            return DefaultReplace();
        }
    }
}