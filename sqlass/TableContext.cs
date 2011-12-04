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

using System.Data.Common;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.sqlass.MetaData;

namespace HWClassLibrary.sqlass
{
    sealed class TableContext : Dumpable, IQueryProvider
    {
        readonly Context _context;
        readonly Table _metaData;

        public TableContext(Context context, Table metaData)
        {
            _context = context;
            _metaData = metaData;
        }

        internal Table MetaData { get { return _metaData; } }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            NotImplementedMethod(expression);
            return null;
        }

        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression) { return new Query<T>(this, expression); }

        object IQueryProvider.Execute(Expression expression)
        {
            NotImplementedMethod(expression);
            return null;
        }

        T IQueryProvider.Execute<T>(Expression expression)
        {
            var mce = expression as MethodCallExpression;
            if(mce != null)
                return Execute<T>(mce.Method, mce.Arguments.ToArray());
            NotImplementedMethod(expression);
            return default(T);
        }
        T Execute<T>(MethodInfo method, Expression[] arguments)
        {
            var methodInfo = typeof(Handler<T>).GetMethod(method.Name);
            if(methodInfo == null)
                throw new MissingMethodException(method.Name);
            return (T) methodInfo.Invoke(this, arguments.Cast<object>().ToArray());
        }

        internal void AddPendingChange(IPendingChange data) { _context.AddPendingChange(data); }

        internal IEnumerator<TElement> Enumerator<TElement>(Expression expression) { return new Enumerator<TElement>(_context.ToDataReader(expression), this); }
        internal object CreateObject(DbDataRecord current) { return _metaData.CreateObject(current, _context); }
    }
}