using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Proof.TokenClasses
{
    abstract class PairToken : ParserTokenType, IPair, ISmartDumpToken
    {
        [DisableDump]
        bool IPair.IsVariablesProvider => true;

        string IPair.SmartDump(ParsedSyntax left, ParsedSyntax right) => SmartDump(left, right);

        ParsedSyntax IPair.IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right)
            => IsolateClause(variable, left, right);

        ParsedSyntax IPair.Pair(ParsedSyntax left, ParsedSyntax right) => Syntax(left, null, right);

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation => false;

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            NotImplementedMethod(parsedSyntax, isFirst);
            return null;
        }

        protected virtual ParsedSyntax IsolateClause
            (string variable, ParsedSyntax left, ParsedSyntax right) => null;

        string SmartDump(ParsedSyntax left, ParsedSyntax right)
            => "(" + left.SmartDump(this) + " " + Id + " " + right.SmartDump(this) + ")";
    }
}