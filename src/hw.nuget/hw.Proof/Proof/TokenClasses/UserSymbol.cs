using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class UserSymbol : TokenClass
    {
        readonly string _id;

        public UserSymbol(string id) { _id = id; }
        
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);
            return new VariableSyntax(token, Id);
        }
        
        public override string Id { get { return _id; } }
    }
}