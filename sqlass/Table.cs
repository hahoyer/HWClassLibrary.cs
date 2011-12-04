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
using System.Linq.Expressions;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.sqlass.MetaData;
using JetBrains.Annotations;

namespace HWClassLibrary.sqlass
{
    public sealed class Table<T>
        : Dumpable
          , IQueryable<T>
          , IExpressionVisitorConstant<IQualifier<string>>
          , IMetaDataProvider
          , IExpressionVisitorConstant<string>
        where T : ISQLSupportProvider
    {
        readonly TableContext _context;

        public Table(Context context, Table metaData) { _context = new TableContext(context, metaData); }

        Expression IQueryable.Expression { get { return Expression.Constant(this); } }
        IQueryProvider IQueryable.Provider { get { return _context; } }
        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<T>) this).GetEnumerator(); }
        Table IMetaDataProvider.MetaData { get { return _context.MetaData; } }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            NotImplementedMethod();
            return default(IEnumerator<T>);
        }

        Type IQueryable.ElementType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [UsedImplicitly]
        public void Add(T newElement) { _context.AddPendingChange(new Insert<T>(newElement)); }

        string IExpressionVisitorConstant<string>.Qualifier { get { return _context.MetaData.SelectString; } }
        IQualifier<string> IExpressionVisitorConstant<IQualifier<string>>.Qualifier { get { return _context.MetaData; } }
        
        public T Find(int key)
        {
            NotImplementedMethod(key);
            return default(T);
        }
    }

    public interface IMetaDataProvider
    {
        Table MetaData { get; }
    }

    public sealed class Reference<T>
    {
        
    }
}