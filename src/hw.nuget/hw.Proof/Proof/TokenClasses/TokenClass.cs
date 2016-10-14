using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    abstract class TokenClass
        : DumpableObject, IScannerType, IUniqueIdProvider, IParserType<ParsedSyntax>
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

        ParsedSyntax IParserType<ParsedSyntax>.Create
            (ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            return Syntax(left, token, right);
        }

        string IParserType<ParsedSyntax>.PrioTableId => Value;

        protected virtual IParserType<ParsedSyntax> Match => null;


        ISubParser<ParsedSyntax> NextParser
        {
            get { return Next; }
        }

        IParserType<ParsedSyntax> ParserType
        {
            get { return this; }
        }


        protected virtual ISubParser<ParsedSyntax> Next { get { return null; } }
        public abstract string Value { get; }
        bool IScannerType.IsGroupToken => true;
    }
}