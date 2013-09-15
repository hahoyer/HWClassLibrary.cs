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

namespace hw.PrioParser
{
    public static class Extension
    {
        public static T Parse<T>(this IPosition<T> current, PrioTable prioTable, Stack<OpenItem<T>> stack = null) where T : class
        {
            if(stack == null)
            {
                stack = new Stack<OpenItem<T>>();
                stack.Push(OpenItem<T>.StartItem(current));
            }

            do
            {
                var item = current.GetItemAndAdvance(stack);
                T result = null;
                do
                {
                    var topItem = stack.Peek();
                    var relation = topItem.Relation(item.Name, prioTable);

                    if(relation != '+')
                        result = stack.Pop().Create(result);

                    if(relation == '-')
                        continue;

                    if(item.IsEnd)
                        return result;

                    stack.Push(new OpenItem<T>(result, item));
                    result = null;
                } while(result != null);
            } while(true);
        }

        public static T Operation<T>(this IOperator<T> @operator, T left, IOperatorPart token, T right) where T : class { return left == null ? (right == null ? @operator.Terminal(token) : @operator.Prefix(token, right)) : (right == null ? @operator.Suffix(left, token) : @operator.Infix(left, token, right)); }
    }
}