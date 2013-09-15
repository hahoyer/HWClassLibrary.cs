// 
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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    static class ParsedSyntaxExtender
    {
        static readonly IComparer<IOrderedEnumerable<string>> _comparer = new ComparerClass();

        sealed class ComparerClass : IComparer<IOrderedEnumerable<string>>
        {
            public int Compare(IOrderedEnumerable<string> x, IOrderedEnumerable<string> y)
            {
                var xx = x.ToArray();
                var yy = y.ToArray();
                for(var i = 0; i < xx.Length && i < yy.Length; i++)
                {
                    var result = String.CompareOrdinal(xx[i], yy[i]);
                    if(result != 0)
                        return result;
                }
                return xx.Length.CompareTo(yy.Length);
            }
        }

        internal static bool IsDistinct(this IEnumerable<ParsedSyntax> x2, IEnumerable<ParsedSyntax> x1) { return x1.All(x => x2.All(xx => IsDistinct(x, xx))); }

        internal static bool IsDistinct(ParsedSyntax x, ParsedSyntax y)
        {
            if(x.GetType() != y.GetType())
                return true;
            return x.IsDistinct(y);
        }

        internal static ParsedSyntax CombineAssosiative<TOperation>(this TOperation operation, TokenData token, IEnumerable<ParsedSyntax> x) where TOperation : IAssociative
        {
            var xx = operation.ToList(x);
            switch(xx.Count())
            {
                case 0:
                    return operation.Empty;
                case 1:
                    return xx.First();
            }
            return operation.Syntax(token, xx);
        }

        internal static Set<ParsedSyntax> ToList(this IAssociative @operator, IEnumerable<ParsedSyntax> set)
        {
            var selectMany = set
                .SelectMany(parsedSyntax => ToList(@operator, parsedSyntax))
                .ToArray();
            var parsedSyntaxs = selectMany
                .OrderBy(parsedSyntax => parsedSyntax, ParsedSyntax.Comparer)
                .ToArray();
            var result = new List<ParsedSyntax>();
            var current = parsedSyntaxs.First();
            for(var i = 1; i < parsedSyntaxs.Count(); i++)
                current = ComineCheck(@operator, result, current, parsedSyntaxs[i]);
            if(!@operator.IsEmpty(current))
                result.Add(current);

            return result.ToArray().ToSet<ParsedSyntax>();
        }

        static ParsedSyntax ComineCheck(IAssociative @operator, ICollection<ParsedSyntax> result, ParsedSyntax current, ParsedSyntax parsedSyntax)
        {
            if(!(current.Variables & parsedSyntax.Variables).IsEmpty)
            {
                var newCurrent = @operator.Combine(current, parsedSyntax);
                if(newCurrent != null)
                    return newCurrent;
            }

            if(!@operator.IsEmpty(current))
                result.Add(current);
            return parsedSyntax;
        }

        internal static Set<ParsedSyntax> ToList(this IAssociative @operator, ParsedSyntax parsedSyntax)
        {
            var associativeSyntax = parsedSyntax as AssociativeSyntax;
            if(associativeSyntax != null && associativeSyntax.Operator == @operator)
                return @operator.ToList(associativeSyntax.Set);
            return parsedSyntax.ToSet();
        }

        internal static bool HasDistinctElements(this IEnumerable<ParsedSyntax> set)
        {
            var varses = set.Select(x => x.Variables).ToArray();
            var xx = varses.Aggregate("", (current, varse) => current + "(" + varse.Aggregate("", (x, y) => x + " " + y) + ")");
            var vars = Set<string>.Empty;
            foreach(var varse in varses)
                if((vars & varse).IsEmpty)
                    vars |= varse;
                else
                    return false;
            return true;
        }
    }
}