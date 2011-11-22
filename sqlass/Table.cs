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
using System.Data.Common;
using System.Linq.Expressions;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using JetBrains.Annotations;

namespace HWClassLibrary.sqlass
{
    public sealed class Table<T> : Dumpable, IQueryable<T>, IMetaUpdateTableProvider
        where T : ISQLSupportProvider
    {
        readonly Context _context;
        readonly string _sqlTableName;

        public Table(Context context, string sqlTableName)
        {
            _context = context;
            _sqlTableName = sqlTableName;
        }

        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<T>) this).GetEnumerator(); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            NotImplementedMethod();
            return default(IEnumerator<T>);
        }

        Expression IQueryable.Expression
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        Type IQueryable.ElementType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        IQueryProvider IQueryable.Provider
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [UsedImplicitly]
        public void Add(T newElement) { _context.AddPendingChange(new Insert<T>(newElement)); }
    }

    public interface IMetaUpdateTableProvider
    {}
}