// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2011 Harald Hoyer
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

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using HWClassLibrary.Debug;

namespace HWClassLibrary.sqlass
{
    sealed class Query<TElement> : Dumpable, IQueryable<TElement>
    {
        readonly Context _context;
        readonly Expression _expression;
        public Query(Context context, Expression expression)
        {
            _context = context;
            _expression = expression;
        }
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() { return _context.Enumerator<TElement>(_expression); }

        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<TElement>) this).GetEnumerator(); }
        Expression IQueryable.Expression { get { return _expression; } }
        IQueryProvider IQueryable.Provider { get { return _context; } }

        Type IQueryable.ElementType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
    }
}