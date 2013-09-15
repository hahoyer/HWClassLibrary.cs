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

using System.Numerics;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    sealed class NumberSyntax : ParsedSyntax, IComparableEx<NumberSyntax>
    {
        internal readonly BigRational Value;

        internal NumberSyntax(TokenData token)
            : base(token) { Value = BigInteger.Parse(token.Name); }

        internal NumberSyntax(BigRational value)
            : base(null) { Value = value; }

        [DisableDump]
        internal override Set<string> Variables { get { return new Set<string>(); } }

        internal override bool IsDistinct(ParsedSyntax other) { throw new NotImplementedException(); }
        internal override string SmartDump(ISmartDumpToken @operator) { return Value.ToString(); }
        internal override ParsedSyntax Times(BigRational value) { return new NumberSyntax(Value * value); }
        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions) { return DefaultReplace(); }
        public int CompareToEx(NumberSyntax other) { return Value.CompareTo(other.Value); }
    }
}