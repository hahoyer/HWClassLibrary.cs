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
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class VariableSyntax : ParsedSyntax, IComparableEx<VariableSyntax>
    {
        internal readonly string Name;

        public VariableSyntax(SourcePart token, string name)
            : base(token) { Name = name; }

        int IComparableEx<VariableSyntax>.CompareToEx(VariableSyntax other) { return String.CompareOrdinal(Name, other.Name); }

        [DisableDump]
        internal override Set<string> Variables { get { return new Set<string> {Name}; } }

        internal override bool IsDistinct(ParsedSyntax other) { return IsDistinct((VariableSyntax) other); }
        internal override ParsedSyntax IsolateFromEquation(string variable, ParsedSyntax otherSite) { return Equal(Token, otherSite); }
        internal override ParsedSyntax IsolateFromSum(string variable, ParsedSyntax other)
        {
            if(Name == variable)
                return other;
            return null;
        }
        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            var result = definitions.Where(d => d.Key == Name).Select(d => d.Value).ToSet();
            result.Add(this);
            return result;
        }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other) { return other.CombineForPlus(this); }
        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue) { return other.CombineForPlus(this, otherValue); }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue, BigRational thisValue) { return other.CombineForPlus(this, thisValue, otherValue); }

        internal override ParsedSyntax CombineForPlus(VariableSyntax other, BigRational thisValue)
        {
            if(Name == other.Name)
                return Times(thisValue + 1);
            return null;
        }

        internal override ParsedSyntax CombineForPlus(VariableSyntax other, BigRational otherValue, BigRational thisValue)
        {
            if(Name == other.Name)
                return Times(thisValue + otherValue);
            return null;
        }

        internal override ParsedSyntax CombineForPlus(VariableSyntax other)
        {
            if(Name == other.Name)
                return Times(2);
            return null;
        }


        internal override string SmartDump(ISmartDumpToken @operator) { return Name; }

        bool IsDistinct(VariableSyntax other) { return Name != other.Name; }
    }
}