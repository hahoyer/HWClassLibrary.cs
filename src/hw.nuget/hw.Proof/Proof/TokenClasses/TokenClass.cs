using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    abstract class TokenClass : DumpableObject, IType<ParsedSyntax>, INameProvider
    {
        string _name;

        protected virtual ParsedSyntax Syntax(ParsedSyntax left, SourcePart token, ParsedSyntax right)
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
        ParsedSyntax IType<ParsedSyntax>.Create(ParsedSyntax left, SourcePart part, ParsedSyntax right)
        {
            return Syntax(left, part, right);
        }

        string IType<ParsedSyntax>.PrioTableName { get { return _name; } }
        ISubParser<ParsedSyntax> IType<ParsedSyntax>.NextParser { get { return Next; } }
        string INameProvider.Name { set { _name = value; } }
        IType<ParsedSyntax> IType<ParsedSyntax>.NextTypeIfMatched { get { return null; } }

        protected virtual ISubParser<ParsedSyntax> Next { get { return null; } }
        public string Name { get { return _name ; } }
    }
}