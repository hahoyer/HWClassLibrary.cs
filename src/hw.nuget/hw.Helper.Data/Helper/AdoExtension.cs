using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Helper
{
    [PublicAPI]
    public static class AdoExtension
    {
        public static DataTable GetSchemaTable(this DbConnection connection, string text) => connection
            .ToDataReader(text)
            .GetSchemaTable();

        public static DbDataReader ToDataReader(this DbConnection connection, string text) => connection
            .ToCommand(text)
            .ExecuteReader();

        public static T[] ToArray<T>(this DbConnection connection, string text)
            where T : IReaderInitialize, new()
            => connection
                .ToDataReader(text)
                .ToArray<T>();

        public static DbCommand ToCommand(this DbConnection connection, string text)
        {
            var command = connection.CreateCommand();
            command.CommandText = text;
            return command;
        }

        public static T[] ToArray<T>(this DbDataReader reader)
            where T : IReaderInitialize, new()
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
                result.Add(converter((DbDataRecord)e.Current));
            return result.ToArray();
        }

        public static string[] GetColumns(this DbDataReader reader)
        {
            var result = new string[reader.FieldCount];
            for(var i = 0; i < reader.FieldCount; i++)
                result[i] = reader.GetName(i);
            return result;
        }

        public static DateTime EnsureSqlDateTime(this DateTime dateTime) => new SqlDateTime(dateTime).Value;

        public static SqlConnection ToConnection
            (this string serverName, string dataBase = null) => ConnectionString(serverName, dataBase)
            .ToConnection();

        public static string SQLFormat(this string data) => SqlFormat(data);
        public static string SqlFormat(this string data) => "'" + data.Replace("'", "''") + "'";

        internal static SqlConnection ToConnection(this SqlConnectionStringBuilder connectionString)
        {
            var connection = new SqlConnection(connectionString.ConnectionString);
            connection.Open();
            return connection;
        }

        static SqlConnectionStringBuilder ConnectionString
            (string serverName, string dataBase) => new SqlConnectionStringBuilder
        {
            DataSource = serverName, IntegratedSecurity = true, MultipleActiveResultSets = true
            , InitialCatalog = dataBase
        };
    }
}