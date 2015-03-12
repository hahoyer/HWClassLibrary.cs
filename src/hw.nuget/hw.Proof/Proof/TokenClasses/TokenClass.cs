using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    abstract class TokenClass : DumpableObject, IType<ParsedSyntax>, IUniqueIdProvider
    {
        protected virtual ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
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
        ParsedSyntax IType<ParsedSyntax>.Create(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            return Syntax(left, token, right);
        }

        string IType<ParsedSyntax>.PrioTableId { get { return Value; } }
        ISubParser<ParsedSyntax> IType<ParsedSyntax>.NextParser { get { return Next; } }
        IType<ParsedSyntax> IType<ParsedSyntax>.NextTypeIfMatched { get { return null; } }

        protected virtual ISubParser<ParsedSyntax> Next { get { return null; } }
        public abstract string Value { get; }
    }
}