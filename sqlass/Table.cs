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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace HWClassLibrary.sqlass
{
    public sealed class Table<T> : Dumpable, IQueryable<T>, IMetaUpdateTableProvider
        where T : ISQLSupportProvider
    {
        private readonly Context _context;
        private readonly string _sqlTableName;

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

        Expression IQueryable.Expression { get { return Expression.Constant(this); } }

        Type IQueryable.ElementType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        IQueryProvider IQueryable.Provider { get { return new Provider(); } }

        [UsedImplicitly]
        public void Add(T newElement) { _context.AddPendingChange(new Insert<T>(newElement)); }
    }

    internal class Provider : IQueryProvider
    {
        IQueryable IQueryProvider.CreateQuery(Expression expression) { throw new NotImplementedException(); }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression) { return new Query<TElement>(this, expression);}

        object IQueryProvider.Execute(Expression expression) { throw new NotImplementedException(); }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            var mce = expression as MethodCallExpression;
            if(mce != null)
            return Activator.CreateInstance() mce.Method.I
            throw new NotImplementedException();
        }
    }

    internal class Query<T> : IQueryable<T>
    {
        private readonly Provider _provider;
        private readonly Expression _expression;

        public Query(Provider provider, Expression expression)
        {
            _provider = provider;
            _expression = expression;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { throw new NotImplementedException(); }

        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<T>)this).GetEnumerator(); }

        Expression IQueryable.Expression { get { return _expression; } }

        Type IQueryable.ElementType { get { throw new NotImplementedException(); } }

        IQueryProvider IQueryable.Provider { get { return _provider; } }
    }

    public interface IMetaUpdateTableProvider
    {
    }
}