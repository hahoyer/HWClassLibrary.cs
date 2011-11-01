using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.DataBase
{
    public abstract class DataBase: Dumpable
    {
        private DbConnection _connection;
        private readonly string _dbPath;

        protected DataBase(string dbPath) { _dbPath = dbPath; }

        private DbConnection Connection
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

        protected T[] Select<T>() where T : new()
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
                return new T[0];
            }
            var result = new List<T>();
            foreach(var dataReader in reader)
            {
                var @object = new T();
                SQLGenerator<T>.SetValues(@object, reader);
                result.Add(@object);
            }
            return result.ToArray();
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

        private void CreateTable<T>()
        {
            var command = Connection.CreateCommand();
            command.CommandText = SQLGenerator<T>.CreateTableCommand;
            command.ExecuteNonQuery();
        }
    }
}