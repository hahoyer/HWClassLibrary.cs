#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    abstract class AssociativeSyntax : ParsedSyntax
    {
        internal readonly IAssociative Operator;
        internal readonly Set<ParsedSyntax> Set;

        protected AssociativeSyntax(IAssociative @operator, TokenData token, Set<ParsedSyntax> set)
            : base(token)
        {
            Operator = @operator;
            Set = set;
        }

        [DisableDump]
        internal override sealed Set<string> Variables
        {
            get
            {
                if(Operator.IsVariablesProvider)
                    return Set<string>.Create(Set.SelectMany(x => x.Variables).ToArray());
                return new Set<string>();
            }
        }

        internal override sealed bool IsDistinct(ParsedSyntax other) { return IsDistinct((AssociativeSyntax) other); }
        internal override string SmartDump(ISmartDumpToken @operator) { return Operator.SmartDump(Set); }

        bool IsDistinct(AssociativeSyntax other)
        {
            if(other.Operator != Operator)
                return true;
            return other.Set.IsDistinct(Set);
        }
    }

    interface ISmartDumpToken
    {
        string SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst);
        bool IsIgnoreSignSituation { get; }
    }

    interface IAssociative
    {
        bool IsVariablesProvider { get; }
        ParsedSyntax Empty { get; }
        string SmartDump(Set<ParsedSyntax> set);
        AssociativeSyntax Syntax(TokenData token, Set<ParsedSyntax> x);
        ParsedSyntax Combine(ParsedSyntax left, ParsedSyntax right);
        bool IsEmpty(ParsedSyntax parsedSyntax);
    }
}