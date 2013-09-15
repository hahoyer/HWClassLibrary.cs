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
using HWClassLibrary.Parser;
using Reni.Parser;

namespace Reni.Proof.TokenClasses
{
    abstract class TokenClass : Parser.TokenClass
    {
        protected override IParsedSyntax Create(IParsedSyntax left, IPart<IParsedSyntax> part, IParsedSyntax right) { throw new NotImplementedException(); }
        protected IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { return Syntax((ParsedSyntax)left, token, (ParsedSyntax)right); }

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