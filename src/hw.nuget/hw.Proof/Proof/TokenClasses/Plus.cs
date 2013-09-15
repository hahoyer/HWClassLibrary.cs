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
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    sealed class Plus : TokenClass, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left != null);
            Tracer.Assert(right != null);
            return left.Associative(this, token, right);
        }

        bool IAssociative.IsVariablesProvider { get { return true; } }

        [DisableDump]
        ParsedSyntax IAssociative.Empty { get { return new NumberSyntax(0); } }

        string IAssociative.SmartDump(Set<ParsedSyntax> set) { return SmartDump(this, set); }
        AssociativeSyntax IAssociative.Syntax(TokenData token, Set<ParsedSyntax> set) { return new PlusSyntax(this, token, set); }
        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) { return left.CombineForPlus(right); }

        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax)
        {
            var numberSyntax = parsedSyntax as NumberSyntax;
            return numberSyntax != null && numberSyntax.Value == 0;
        }

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation { get { return true; } }

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
        {
            if(parsedSyntax.IsNegative)
                return (isFirst ? "" : " ") + "- ";
            return isFirst ? "" : " + ";
        }
    }
}