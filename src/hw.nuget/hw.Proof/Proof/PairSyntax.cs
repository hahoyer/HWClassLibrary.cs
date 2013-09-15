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

namespace hw.Proof
{
    abstract class PairSyntax : ParsedSyntax, IComparableEx<PairSyntax>
    {
        internal readonly IPair Operator;
        internal readonly ParsedSyntax Left;
        internal readonly ParsedSyntax Right;

        internal PairSyntax(IPair @operator, ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(token)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        [DisableDump]
        internal override Set<string> Variables
        {
            get
            {
                if(Operator.IsVariablesProvider)
                    return Left.Variables | Right.Variables;
                return new Set<string>();
            }
        }

        int IComparableEx<PairSyntax>.CompareToEx(PairSyntax other)
        {
            var result = Left.CompareTo(other.Left);
            if(result == 0)
                result = Right.CompareTo(other.Right);
            return result;
        }

        internal override bool IsDistinct(ParsedSyntax other) { return IsDistinct((PairSyntax) other); }
        bool IsDistinct(PairSyntax other) { return other.Operator != Operator || ParsedSyntaxExtender.IsDistinct(other.Left, Left) || ParsedSyntaxExtender.IsDistinct(other.Right, Right); }
        internal override string SmartDump(ISmartDumpToken @operator) { return Operator.SmartDump(Left, Right); }

        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            var definitionArray = definitions.ToArray();
            var leftResults = Left.Replace(definitionArray);
            var rightResults = Right.Replace(definitionArray);
            var result = leftResults.SelectMany(left => rightResults.Select(syntax => left.Pair(Operator, syntax))).ToSet();
            return result;
        }
    }

    interface IPair
    {
        bool IsVariablesProvider { get; }
        string SmartDump(ParsedSyntax left, ParsedSyntax right);
        ParsedSyntax IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right);
        ParsedSyntax Pair(ParsedSyntax left, ParsedSyntax right);
    }
}