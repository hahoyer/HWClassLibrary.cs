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
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof.TokenClasses
{
    sealed class Caret : PairToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, SourcePart token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);
            return new PowerSyntax(this, left, token, right);
        }
    }

    sealed class PowerSyntax : PairSyntax
    {
        public PowerSyntax(IPair @operator, ParsedSyntax left, SourcePart token, ParsedSyntax right)
            : base(@operator, left, token, right) { }

        internal int CompareTo(PowerSyntax other)
        {
            NotImplementedMethod(other);
            return 0;
        }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue) { return other.CombineForPlus(this, otherValue); }
        internal override ParsedSyntax CombineForPlus(ParsedSyntax other) { return other.CombineForPlus(this); }
        internal override ParsedSyntax CombineForPlus(PowerSyntax other, BigRational thisValue)
        {
            if(Left.CompareTo(other.Left) == 0 && Right.CompareTo(other.Right) == 0)
                return Times(thisValue + 1);
            return null;
        }
    }
}