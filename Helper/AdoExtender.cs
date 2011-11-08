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
using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    public static class AdoExtender
    {
        public static DataTable Schema(this DbConnection connection, string text) { return connection.ToDataReader(text).Schema(); }
        public static DbDataReader ToDataReader(this DbConnection connection, string text) { return connection.ToCommand(text).ExecuteReader(); }
        public static T[] ToArray<T>(this DbConnection connection, string text) where T : IReaderInitialize, new() { return connection.ToDataReader(text).ToArray<T>(); }

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

        public static T[] ToArray<T>(this DbDataReader reader) where T : IReaderInitialize, new()
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
    }

    public interface IReaderInitialize
    {
        void Initialize(DbDataReader reader);
    }
}