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

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.DataBase
{
    static class SQLGenerator<T>
    {
        public static string SelectCommand { get { return "select * from " + TableName; } }

        public static string CreateTableCommand
        {
            get
            {
                var result = "create table " + TableName;
                var columns = Columns;
                for(var i = 0; i < columns.Length; i++)
                {
                    result += i == 0 ? "(" : ", ";
                    result += columns[i].CreateTableCommand;
                }
                return result + ")";
            }
        }

        public static string TableName { get { return typeof(T).Name; } }
        public static TableColumn[] Columns
        {
            get
            {
                return typeof(T)
                    .GetFields()
                    .Select(fieldInfo => new TableColumn(fieldInfo))
                    .ToArray();
            }
        }

        public static string InsertCommand(T newObject)
        {
            var result = "insert into " + TableName;
            var columns = Columns;
            var valueList = "values";
            for(var i = 0; i < columns.Length; i++)
            {
                result += i == 0 ? "(" : ", ";
                result += columns[i].Name;
                valueList += i == 0 ? "(" : ", ";
                valueList += columns[i].ValueAsLiteral(newObject);
            }
            return result + ")" + valueList + ")";
        }

        public static string InsertCommand(T[] newObjects) { return newObjects.Aggregate("", (current, newObject) => current + (InsertCommand(newObject) + " ")); }

        public static void SetValues(object o, DbDataReader reader)
        {
            var columns = Columns;
            for(var i = 0; i < columns.Length; i++)
                columns[i].Value(o, reader.GetValue(i));
        }
    }
}