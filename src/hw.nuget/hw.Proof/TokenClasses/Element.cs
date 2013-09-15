using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    internal sealed class Element : PairToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);
            return new ElementSyntax(this, left, token, right);
        }
    }

    internal sealed class ElementSyntax : PairSyntax
    {
        public ElementSyntax(IPair @operator, ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(@operator, left, token, right) { }

        protected override ParsedSyntax IsolateClause(string variable) { return null; }
        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions) { return DefaultReplace(); }
    }
}