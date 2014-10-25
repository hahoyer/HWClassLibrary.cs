using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace hw.Proof.TokenClasses
{
    class PairToken : TokenClass, IPair, ISmartDumpToken
    {
        [DisableDump]
        bool IPair.IsVariablesProvider { get { return true; } }

        string IPair.SmartDump(ParsedSyntax left, ParsedSyntax right) { return SmartDump(left, right); }
        ParsedSyntax IPair.IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right)
        {
            return IsolateClause(variable, left, right);
        }
        ParsedSyntax IPair.Pair(ParsedSyntax left, ParsedSyntax right) { return Syntax(left, null, right); }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return false; } }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            NotImplementedMethod(parsedSyntax, isFirst);
            return null;
        }

        protected virtual ParsedSyntax IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right) { return null; }

        string SmartDump(ParsedSyntax left, ParsedSyntax right)
        {
            return "(" + left.SmartDump(this) + " " + Name + " " + right.SmartDump(this) + ")";
        }
    }
}