// 
//     Project HWClassLibrary
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

using System.Collections;
using System.Linq.Expressions;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.sqlass.MetaData;
using JetBrains.Annotations;

namespace HWClassLibrary.sqlass
{
    public sealed class Table<T, TKey>
        : Dumpable
          , IQueryable<T>
          , IMetaDataProvider
          , IExpressionVisitorConstant<string>
        where T : ISQLSupportProvider, ISQLKeyProvider<TKey>
        where TKey : struct
    {
        readonly TableContext _context;
        readonly DictionaryEx<TKey, T> _cache;

        public Table(Context context, Table metaData)
        {
            _context = new TableContext(context, metaData);
            _cache = new DictionaryEx<TKey, T>(FindRecent);
        }

        T FindRecent(TKey key)
        {
            var sql = _context.MetaData.SelectString + " where Id = " + key;
            return _context.ExecuteSQLString<T>(sql).Single();
        }

        Expression IQueryable.Expression { get { return Expression.Constant(this); } }
        IQueryProvider IQueryable.Provider { get { return _context; } }
        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<T>) this).GetEnumerator(); }
        Table IMetaDataProvider.MetaData { get { return _context.MetaData; } }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            NotImplementedMethod();
            return (IEnumerator<T>) new T[0].GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get
            {
                NotImplementedMethod();
                return typeof(T);
            }
        }

        [UsedImplicitly]
        public void Add(T newElement) { _context.AddPendingChange(new Insert<T>(newElement)); }

        string IExpressionVisitorConstant<string>.Qualifier { get { return _context.MetaData.SelectString; } }

        public T Find(TKey? key)
        {
            if(key == null)
                return default(T);
            return _cache.Find(key.Value);
        }
    }

    public interface IMetaDataProvider
    {
        Table MetaData { get; }
    }

    public sealed class Reference<T>
    {}
}