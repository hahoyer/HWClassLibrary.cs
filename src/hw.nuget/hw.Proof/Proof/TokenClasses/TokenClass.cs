using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.PrioParser;

namespace hw.Proof.TokenClasses
{
    abstract class TokenClass : Parser.TokenClass
    {
        protected override IParsedSyntax Create(IParsedSyntax left, IPart<IParsedSyntax> part, IParsedSyntax right) { throw new NotImplementedException(); }

        protected virtual ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected static string SmartDump(ISmartDumpToken @operator, Set<ParsedSyntax> set)
        {
            var result = "";
            var isFirst = true;
            foreach(var parsedSyntax in set)
            {
                result += @operator.SmartDumpListDelim(parsedSyntax, isFirst);
                result += parsedSyntax.SmartDump(@operator);
                isFirst = false;
            }
            return "(" + result + ")";
        }
    }
}