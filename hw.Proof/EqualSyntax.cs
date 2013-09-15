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
using Reni.Parser;

namespace Reni.Proof
{
    sealed class EqualSyntax : PairSyntax, IComparableEx<EqualSyntax>
    {
        public EqualSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
            : base(Main.TokenFactory.Equal, left, token, right) { }

        int IComparableEx<EqualSyntax>.CompareToEx(EqualSyntax other)
        {
            var result = Left.CompareTo(other.Right);
            if(result == 0)
                return Right.CompareTo(other.Left);

            result = Left.CompareTo(other.Left);
            if(result == 0)
                return Right.CompareTo(other.Right);

            return result;
        }

        protected override ParsedSyntax IsolateClause(string variable)
        {
            if(Left.Variables.Contains(variable))
            {
                Tracer.Assert(!Right.Variables.Contains(variable));
                return Left.IsolateFromEquation(variable, Right);
            }
            Tracer.Assert(Right.Variables.Contains(variable));
            return Right.IsolateFromEquation(variable, Left);
        }
    }
}