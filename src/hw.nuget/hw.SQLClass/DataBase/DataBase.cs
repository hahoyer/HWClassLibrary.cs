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
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.DataBase
{
    public abstract class DataBase : Dumpable
    {
        DbConnection _connection;
        readonly string _dbPath;

        protected DataBase(string dbPath) { _dbPath = dbPath; }

        DbConnection Connection
        {
            get
            {
                if(_connection == null)
                {
                    _connection = CreateConnection(_dbPath);
                    _connection.Open();
                }
                return _connection;
            }
        }

        protected abstract DbConnection CreateConnection(string dbPath);

        protected IEnumerable<T> Select<T>() where T : new()
        {
            var command = Connection.CreateCommand();
            command.CommandText = SQLGenerator<T>.SelectCommand;
            DbDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch(Exception e)
            {
                var expectedMessage = "SQLite error\r\nno such table: " + SQLGenerator<T>.TableName;
                if(expectedMessage != e.Message)
                    throw;
                CreateTable<T>();
                yield break;
            }
            foreach(var @object in reader.Cast<object>().Select(dataReader => new T()))
            {
                SQLGenerator<T>.SetValues(@object, reader);
                yield return (@object);
            }
        }

        protected void Insert<T>(T[] newObjects)
        {
            var command = Connection.CreateCommand();
            command.CommandText = SQLGenerator<T>.InsertCommand(newObjects);
            var rowsAffected = command.ExecuteNonQuery();
            Tracer.Assert(rowsAffected == newObjects.Length);
        }

        protected void Insert<T>(T newObject)
        {
            var command = Connection.CreateCommand();
            command.CommandText = SQLGenerator<T>.InsertCommand(newObject);
            var rowsAffected = command.ExecuteNonQuery();
            Tracer.Assert(rowsAffected == 1);
        }

        void CreateTable<T>()
        {
            var command = Connection.CreateCommand();
            command.CommandText = SQLGenerator<T>.CreateTableCommand;
            command.ExecuteNonQuery();
        }
    }
}