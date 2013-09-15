#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

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
        ParsedSyntax IPair.IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right) { return IsolateClause(variable, left, right); }
        ParsedSyntax IPair.Pair(ParsedSyntax left, ParsedSyntax right) { return Syntax(left, null, right); }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return false; } }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            NotImplementedMethod(parsedSyntax, isFirst);
            return null;
        }

        protected virtual ParsedSyntax IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right) { return null; }

        string SmartDump(ParsedSyntax left, ParsedSyntax right) { return "(" + left.SmartDump(this) + " " + Name + " " + right.SmartDump(this) + ")"; }
    }
}