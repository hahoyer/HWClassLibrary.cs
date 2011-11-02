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
using System.Data.SqlServerCe;
using System.Reflection;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.sqlass.MetaData;

namespace HWClassLibrary.sqlass
{
    public class Context : Dumpable
    {
        DbConnection _connection;
        List<IPendingChange> _pending;
        readonly SimpleCache<MetaData.MetaData> _sqlMetaData;

        public Context() { _sqlMetaData = new SimpleCache<MetaData.MetaData>(ObtainSQLMetaData); }

        MetaData.MetaData ObtainSQLMetaData() { return new MetaData.MetaData(_connection); }

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

        public void SetSqlCeConnection(string dataSource) { Connection = new SqlCeConnection(@"Data Source=" + dataSource + ";"); }

        public DbConnection Connection
        {
            private get { return _connection; }
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

        protected void UpdateDatabase(object container, DictionaryEx<Type, MetaDataSupport> metaDataSupport)
        {
            if(_pending != null)
                throw new UnsavedChangedException();

            var type = container.GetType();
            var target = typeof(Table<>);

            var knownTables = type
                .GetMembers()
                .Where(m => m.DeclaringType == type && m.MemberType == MemberTypes.Field)
                .Cast<FieldInfo>()
                .Where(fi => fi.FieldType.GetGenericTypeDefinition() == target)
                .Select(fi => metaDataSupport[fi.FieldType.GetGenericArguments()[0]])
                .ToArray();

            var sqlTables = _sqlMetaData.Value.Tables;

            var actions = knownTables.Merge(sqlTables, kt => kt.TableName, st => st.TABLE_NAME).ToArray();
            foreach(var action in actions)
                UpdateDatabase(action.Item2, action.Item3);
        }

        void UpdateDatabase(MetaDataSupport container, Table metaDataSupport)
        {
            if(metaDataSupport == null)
            {
                ExecuteNonQuery(container.CreateTable);
                return;
            }
            if (container == null)
            {
                ExecuteNonQuery("drop table " + metaDataSupport.TABLE_NAME);
                return;
            }
            NotImplementedMethod(container, metaDataSupport);
        }

        internal void ExecuteNonQuery(string text)
        {
            using(var command = Connection.CreateCommand())
            {
                command.CommandText = text;
                command.ExecuteNonQuery();
            }
        }

        public DataRow[] Schema { get { return Connection.GetSchema().Select(); } }
        public DataTable SubSchema(string name) { return Connection.GetSchema(name); }
    }

    sealed class UnsavedChangedException : Exception
    {}
}