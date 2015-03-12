using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class UserSymbol : TokenClass
    {
        readonly string _value;

        public UserSymbol(string value) { _value = value; }
        
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            if(left != null || right != null)
                return base.Syntax(left, token, right);
            return new VariableSyntax(token, Value);
        }
        
        public override string Value { get { return _value; } }
    }
}