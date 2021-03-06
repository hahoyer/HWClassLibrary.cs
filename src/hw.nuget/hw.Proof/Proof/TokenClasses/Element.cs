using System.Collections.Generic;
using hw.Parser;
// ReSharper disable CheckNamespace

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

        protected override string Id => "elem";
    }

    sealed class ElementSyntax : PairSyntax
    {
        public ElementSyntax(IPair @operator, ParsedSyntax left, IToken token, ParsedSyntax right)
            : base(@operator, left, token, right) { }

        protected override ParsedSyntax IsolateClause(string variable) => null;

        internal override Set<ParsedSyntax> Replace
            (IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions) => DefaultReplace();
    }
}