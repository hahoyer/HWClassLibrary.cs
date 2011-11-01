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

namespace HWClassLibrary.sqlass
{
    public class Context : Dumpable
    {
        DbConnection _connection;
        List<IPendingChange> _pending;

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

        public void SetSqlCeConnection (string dataSource) { Connection = new SqlCeConnection(@"Data Source=" + dataSource + ";"); }

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
            var metaDataSupports = type
                .GetMembers()
                .Where(m => m.DeclaringType == type && m.MemberType == MemberTypes.Field)
                .Cast<FieldInfo>()
                .Where(fi => fi.FieldType.GetGenericTypeDefinition() == target)
                .Select(fi => metaDataSupport[fi.FieldType.GetGenericArguments()[0]])
                .ToArray();

            var sqlMetaData = new MetaData.MetaData(_connection);
            var columns = sqlMetaData.Columns;

            NotImplementedMethod(container, metaDataSupport);
            foreach(var meta in metaDataSupports)
            {
                var tableName = meta.TableName;
            }
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
