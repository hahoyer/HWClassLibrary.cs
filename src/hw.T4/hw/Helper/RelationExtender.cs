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

namespace hw.Helper
{
    public static class RelationExtender
    {
        public static IEnumerable<TType> Sort<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents)
        {
            var xx = x.ToArray();
            Tracer.Assert(xx.IsCircuidFree(immediateParents));
            return null;
        }

        public static IEnumerable<TType> Closure<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents)
        {
            var types = x.ToArray();
            var targets = types;
            while(true)
            {
                targets = targets.SelectMany(immediateParents).Except(types).ToArray();
                if(!targets.Any())
                    return types;
                types = types.Union(targets).ToArray();
            }
        }

        public static bool IsCircuidFree<TType>(this TType x, Func<TType, IEnumerable<TType>> immediateParents) { return immediateParents(x).Closure(immediateParents).All(xx => !xx.Equals(x)); }
        public static bool IsCircuidFree<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents) { return x.All(xx => xx.IsCircuidFree(immediateParents)); }

        public static IEnumerable<TType> Circuids<TType>(this IEnumerable<TType> x, Func<TType, IEnumerable<TType>> immediateParents) { return x.Where(xx => !xx.IsCircuidFree(immediateParents)); }
    }
}