using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace HWClassLibrary.DataBase
{
    internal static class SQLGenerator<T>
    {
        public static string SelectCommand { get { return "select * from " + TableName; } }

        public static string CreateTableCommand
        {
            get
            {
                var result = "create table " + TableName;
                var columns = Columns;
                for (var i = 0; i < columns.Length; i++)
                {
                    result += i == 0 ? "(" : ", ";
                    result += columns[i].CreateTableCommand;
                }
                return result + ")";
            }
        }

        public static string TableName { get { return typeof (T).Name; } }

        public static TableColumn[] Columns
        {
            get
            {
                var fields = typeof (T).GetFields();
                var result = new List<TableColumn>();
                foreach (var fieldInfo in fields)
                    result.Add(new TableColumn(fieldInfo));
                return result.ToArray();
            }
        }

        public static string InsertCommand(T newObject)
        {
            var result = "insert into " + TableName;
            var columns = Columns;
            var valueList = "values";
            for (var i = 0; i < columns.Length; i++)
            {
                result += i == 0 ? "(" : ", ";
                result += columns[i].Name;
                valueList += i == 0 ? "(" : ", ";
                valueList += columns[i].ValueAsLiteral(newObject);
            }
            return result + ")" + valueList + ")";
        }

        public static string InsertCommand(T[] newObjects)
        {
            var result = "";
            foreach (var newObject in newObjects)
                result += InsertCommand(newObject) + " ";
            return result;
        }

        public static void SetValues(object o, SQLiteDataReader reader) {
            var columns = Columns;
            for (var i = 0; i < columns.Length; i++)
                columns[i].Value(o, reader.GetValue(i));
        }
    }
}