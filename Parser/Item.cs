#region Copyright (C) 2013

//     Project HWClassLibrary
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Parser
{
    public sealed class Item<T>
        where T : class
    {
        public readonly IType<T> Type;
        public readonly IPart<T> Part;

        public Item(IType<T> type, IPart<T> part)
        {
            Type = type;
            Part = part;
        }

        public string Name { get { return Type == null ? PrioTable.BeginOfText : Type.PrioTableName; } }
        public bool IsEnd { get { return Type != null && Type.IsEnd; } }
        public T Create(T left, T right)
        {
            if(Type != null)
                return Type.Create(left, Part, right);
            Tracer.Assert(left == null);
            return right;
        }
    }
}