#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace hw.Helper
{
    public static class AdoExtender
    {
        public static DataTable Schema(this DbConnection connection, string text) { return connection.ToDataReader(text).Schema(); }
        public static DbDataReader ToDataReader(this DbConnection connection, string text) { return connection.ToCommand(text).ExecuteReader(); }
        public static T[] XToArray<T>(this DbConnection connection, string text) where T : IReaderInitialize, new() { return connection.ToDataReader(text).XToArray<T>(); }

        public static DataTable Schema(this DbDataReader reader)
        {
            var schemaTable = reader.GetSchemaTable();
            return schemaTable;
        }

        public static DbCommand ToCommand(this DbConnection connection, string text)
        {
            var command = connection.CreateCommand();
            command.CommandText = text;
            return command;
        }

        public static T[] XToArray<T>(this DbDataReader reader) where T : IReaderInitialize, new()
        {
            var result = new List<T>();
            while(reader.Read())
            {
                var t = new T();
                t.Initialize(reader);
                result.Add(t);
            }
            return result.ToArray();
        }

        public static T[] SelectFromReader<T>(this DbDataReader reader, Func<DbDataRecord, T> converter)
        {
            var result = new List<T>();
            var e = reader.GetEnumerator();
            while(e.MoveNext())
                result.Add(converter((DbDataRecord) e.Current));
            return result.ToArray();
        }

        public static string[] Columns(this DbDataReader reader)
        {
            var result = new string[reader.FieldCount];
            for(var i = 0; i < reader.FieldCount; i++)
                result[i] = reader.GetName(i);
            return result;
        }
    }

    public interface IReaderInitialize
    {
        void Initialize(DbDataReader reader);
    }
}