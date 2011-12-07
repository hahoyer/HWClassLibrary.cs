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

using System.Data.Common;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;


namespace HWClassLibrary.sqlass.MetaData
{
    public sealed class Table : Dumpable, IExpressionVisitorContext, IQualifier<string>
    {
        internal readonly TableName TableName;
        readonly SimpleCache<Column[]> _columnsCache;
        readonly Func<DbDataRecord, Context, object> _createObjectFunction;

        public Table(TableName tableName, Func<Column[]> getColumns, Func<DbDataRecord, Context, object> createObjectFunction)
        {
            TableName = tableName;
            _createObjectFunction = createObjectFunction;
            _columnsCache = new SimpleCache<Column[]>(getColumns);
        }

        internal Column[] Columns { get { return _columnsCache.Value; } }

        [DisableDump]
        internal string CreateTable
        {
            get
            {
                var result
                    = "create table "
                      + SQLTableName
                      + " (\n"
                      + Columns.Select(c => c.CreateColumnSQL).Format(",\n");
                if(HasKeys)
                    result
                        += ", primary key("
                           + KeyNames
                           + ")";
                result
                    += ")";
                return result;
            }
        }


        internal static Table FromMetaType(Type metaType)
        {
            return
                new Table
                    (
                    TableName.Find(CatalogAttribute.Get(metaType), SchemaAttribute.Get(metaType), metaType.Name)
                    , () => ObtainColumns(metaType)
                    , null
                    );
        }

        static bool IsNullableField(FieldInfo fi) { return fi.GetAttribute<NullableAttribute>(true) != null; }
        static bool IsKeyField(FieldInfo fi) { return fi.GetAttribute<KeyAttribute>(true) != null; }
        static bool IsReferenceType(Type metaColumnType) { return metaColumnType.IsGenericType && metaColumnType.GetGenericTypeDefinition() == typeof(T4.Reference<>); }

        static Column[] ObtainColumns(Type metaType)
        {
            var fieldInfos = metaType.GetFields();
            if(fieldInfos.Length > 0)
            {
                var fi = fieldInfos[0];
                var isKey = IsKeyField(fi);
            }
            return fieldInfos
                .Select
                (
                    fi =>
                    new Column
                    {
                        Name = fi.Name,
                        Type = NativeColumnType(fi.FieldType),
                        IsKey = IsKeyField(fi),
                        ReferencedTable = ReferencedTable(fi.FieldType),
                        IsNullable = IsNullableField(fi)
                    }
                )
                .ToArray();
        }

        static Table ReferencedTable(Type metaColumnType)
        {
            if(!IsReferenceType(metaColumnType))
                return null;
            var keyType = metaColumnType.GetGenericArguments()[0];
            return FromMetaType(keyType);
        }

        static Type NativeColumnType(Type metaColumnType)
        {
            if(IsReferenceType(metaColumnType))
                return GetKeyType(metaColumnType);
            return metaColumnType;
        }

        internal static Type GetKeyType(Type metaColumnType) { return ReferencedTable(metaColumnType).KeyType; }

        [DisableDump]
        internal string NameSpace { get { return TableName.NameSpace; } }

        [DisableDump]
        Type[] KeyTypes
        {
            get
            {
                return Columns
                    .Where(c => c.IsKey)
                    .Select(c => c.Type)
                    .ToArray();
            }
        }

        [DisableDump]
        internal string SQLTableName { get { return TableName.SQLTableName; } }

        [DisableDump]
        internal Type KeyType
        {
            get
            {
                var keys = KeyTypes;
                if(keys.Length == 1)
                    return keys[0];
                return typeof(Tuple<>).MakeGenericType(keys);
            }
        }

        string KeyNames
        {
            get
            {
                return Columns
                    .Where(c => c.IsKey)
                    .Select(c => c.Name)
                    .Format(", ");
            }
        }
        [DisableDump]
        internal string KeyValue
        {
            get
            {
                var keys = Columns
                    .Where(c => c.IsKey)
                    .Select(c => c.Name)
                    .ToArray();
                if(keys.Length == 1)
                    return keys[0];
                return "new " + KeyType + "(" + keys.Format(", ") + ")";
            }
        }

        [DisableDump]
        internal Type KeyProvider
        {
            get
            {
                if(HasKeys)
                    return typeof(ISQLKeyProvider<>).MakeGenericType(KeyTypes);
                return null;
            }
        }

        [DisableDump]
        bool HasKeys { get { return Columns.Any(c => c.IsKey); } }
        [DisableDump]
        public string Name { get { return TableName.Name; } }
        [DisableDump]
        public string Schema { get { return TableName.Schema; } }
        [DisableDump]
        public string Catalog { get { return TableName.Catalog; } }
        [DisableDump]
        public string FullName { get { return NameSpace + "." + Name; } }

        public string UpdateDefinition(Column modell, Column dataBase)
        {
            if(modell != null && dataBase != null)
            {
                Tracer.Assert(modell.Name == dataBase.Name);

                if(modell.Type != dataBase.Type)
                {
                    NotImplementedMethod(modell, dataBase);
                    return null;
                }

                if(modell.IsKey != dataBase.IsKey)
                {
                    NotImplementedMethod(modell, dataBase);
                    return null;
                }

                if(modell.IsNullable != dataBase.IsNullable)
                {
                    NotImplementedMethod(modell, dataBase);
                    return null;
                }

                if(modell.ReferencedTable == null)
                {
                    NotImplementedMethod(modell, dataBase);
                    return null;
                }

                if(modell.ReferencedTable.DiffersFrom(dataBase.ReferencedTable))
                    return string.Format("alter table {0} add foreign key ({1}) references {2}({3})"
                                         , Name
                                         , modell.Name
                                         , modell.ReferencedTable.Name
                                         , modell.ReferencedTable.KeyNames
                        );
            }
            NotImplementedMethod(modell, dataBase);
            return null;
        }
        public bool DiffersFrom(Table other) { return other != null && TableName != other.TableName; }

        internal string SelectString
        {
            get
            {
                return
                    "select "
                    + Columns.Select(c => c.Name).Format(", ")
                    + " from "
                    + TableName.SQLTableName;
            }
        }

        public object CreateObject(DbDataRecord current, Context context) { return _createObjectFunction(current, context); }
        public bool KeyFilter(ISQLSupportProvider sqlSupportProvider, object key) { throw new NotImplementedException(); }
    }
}