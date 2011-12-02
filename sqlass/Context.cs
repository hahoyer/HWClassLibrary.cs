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

using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.sqlass.MetaData;

namespace HWClassLibrary.sqlass
{
    public class Context : Dumpable, IQueryProvider
    {
        DbConnection _connection;
        public bool IsSqlCeConnectionBug;
        List<IPendingChange> _pending;
        readonly SimpleCache<MetaData.MetaData> _sqlMetaData;

        public Context() { _sqlMetaData = new SimpleCache<MetaData.MetaData>(ObtainSQLMetaData); }

        MetaData.MetaData ObtainSQLMetaData() { return new MetaData.MetaData(this); }

        public void SaveChanges()
        {
            if(_pending == null)
                return;

            var transaction = Connection.BeginTransaction();
            try
            {
                foreach(var pendingChange in _pending)
                    pendingChange.Apply(this);
                transaction.Commit();
                _pending = null;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public DbConnection Connection
        {
            internal get { return _connection; }
            set
            {
                if(_connection != null)
                    _connection.Close();
                _connection = value;
                _connection.Open();
            }
        }


        internal void AddPendingChange(IPendingChange data)
        {
            if(_pending == null)
                _pending = new List<IPendingChange>();
            _pending.Add(data);
        }

        protected void UpdateDatabase(object container)
        {
            if(_pending != null)
                throw new UnsavedChangedException();

            var type = container.GetType();
            var target = typeof(Table<>);

            var modell = type
                .GetMembers()
                .Where(m => m.DeclaringType == type && m.MemberType == MemberTypes.Field)
                .Cast<FieldInfo>()
                .Where(fi => fi.FieldType.GetGenericTypeDefinition() == target)
                .Select(fi => ((IMetaDataProvider) fi.GetValue(container)).MetaData)
                .ToArray();

            var dataBase = _sqlMetaData.Value.Tables;

            var actions = modell.Merge(dataBase, kt => kt.Name, st => st.Name).ToArray();
            foreach(var action in actions)
                UpdateDatabase(action.Item2, action.Item3);
        }

        void UpdateDatabase(Table modell, Table dataBase)
        {
            if(dataBase == null)
            {
                ExecuteNonQuery(modell.CreateTable);
                return;
            }

            if(modell == null)
            {
                ExecuteNonQuery("drop table " + dataBase.Name);
                return;
            }

            var actions = modell
                .Columns
                .Merge(dataBase.Columns, mo => mo.Name, st => st.Name)
                .Where(a => a.Item2.DiffersFrom(a.Item3))
                .ToArray();
            if(actions.Length == 0)
                return;

            foreach(var action in actions)
            {
                var sql = modell.UpdateDefinition(action.Item2, action.Item3);
                ExecuteNonQuery(sql);
            }
        }

        internal void ExecuteNonQuery(string text)
        {
            Tracer.FlaggedLine(1, text);
            Connection.ToCommand(text).ExecuteNonQuery();
        }
        internal T[] Execute<T>(string text) where T : IReaderInitialize, new()
        {
            Tracer.FlaggedLine(1, text);
            return Connection.ToArray<T>(text);
        }
        public DataTable Schema(string text) { return Connection.Schema(text); }

        public DataTable SubSchema(string name) { return Connection.GetSchema(name); }

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

        internal IEnumerator<TElement> Enumerator<TElement>(Expression expression) { return new Enumerator<TElement>(Connection.ToDataReader(CreateSqlStatement(expression))); }

        string CreateSqlStatement(Expression expression) { return new StringVisitor().Visit(expression); }

        internal static T CreateObject<T>(object current)
        {
            DumpDataWithBreak("", current);
            return default(T);
        }
    }

    sealed class UnsavedChangedException : Exception
    {}
}