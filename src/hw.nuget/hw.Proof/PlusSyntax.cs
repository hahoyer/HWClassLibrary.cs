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
using Reni.Proof.TokenClasses;

namespace Reni.Proof
{
    sealed class PlusSyntax : AssociativeSyntax, IComparableEx<PlusSyntax>
    {
        public PlusSyntax(Plus @operator, TokenData token, Set<ParsedSyntax> set)
            : base(@operator, token, set) { Tracer.Assert(set.All(x => !(x is PlusSyntax))); }

        internal override ParsedSyntax IsolateFromEquation(string variable, ParsedSyntax otherSite)
        {
            var contains = Set.Where(x => x.Variables.Contains(variable)).ToArray();
            var notContains = Set.Where(x => !x.Variables.Contains(variable));
            if(contains.Count() != 1)
                return null;

            return contains.First().IsolateFromSum(variable, otherSite.Minus(notContains));
        }

        internal override ParsedSyntax Times(BigRational value) { return Operator.CombineAssosiative(Token, Set.Select(x => x.Times(value))); }

        protected override ParsedSyntax Normalize()
        {
            var factor = Set.First().Factor;
            if(factor == 1)
                return this;
            return Times(1 / factor);
        }

        int IComparableEx<PlusSyntax>.CompareToEx(PlusSyntax other)
        {
            if(other == null)
                return 1;

            var result = Set.Count().CompareTo(other.Set.Count());
            if(result != 0)
                return result;

            var s1 = Set.OrderBy(x => x, Comparer).ToArray();
            var s2 = other.Set.OrderBy(x => x, Comparer).ToArray();

            for(var i = 0; i < s1.Length; i++)
            {
                result = s1[i].CompareTo(s2[i]);
                if(result != 0)
                    return result;
            }

            return result;
        }

        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            var replaced = Set
                .Select(summand => summand.Replace(definitions));
            var results = replaced
                .Aggregate(new[] {Set<ParsedSyntax>.Empty}, (result, variant) => result.SelectMany(soFar => variant.Select(newElement => soFar | newElement)).ToArray())
                .Select(x => Operator.CombineAssosiative(Token, x))
                .ToSet();
            return results;
        }
    }
}