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

using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.sqlass.T4;


namespace HWClassLibrary.sqlass.MetaData
{
    public sealed class Table
    {
        public string Catalog;
        public string Schema;
        public string Name;
        public Column[] Columns;
        public string CreateTable { get { throw new NotImplementedException(); } }

        public static Table FromMetaType(Type metaType)
        {
            return
                new Table
                {
                    Name = metaType.Name,
                    Schema = SchemaAttribute.Get(metaType),
                    Catalog = CatalogAttribute.Get(metaType),
                    Columns = ObtainColumns(metaType)
                };
        }

        public Type ToMetaType { get { return null; } }

        static Column[] ObtainColumns(Type metaType)
        {
            return metaType
                .GetFields()
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

        public string NameSpace
        {
            get
            {
                if(Catalog == null && Schema == null)
                    return "";
                return Catalog + "." + Schema;
            }
        }

        static Table ReferencedTable(Type metaColumnType)
        {
            if (!IsReferenceType(metaColumnType))
                return null;
            var keyType = metaColumnType.GetGenericArguments()[0];
            return FromMetaType(keyType);
        }

        static string NativeColumnType(Type metaColumnType)
        {
            if(IsReferenceType(metaColumnType))
                return KeyType(metaColumnType).FullName;
            return metaColumnType.FullName;
        }

        internal static Type KeyType(Type metaColumnType)
        {
            var keyType = metaColumnType.GetGenericArguments()[0];
            throw new NotImplementedException();
        }

        static bool IsNullableField(FieldInfo fi) { return fi.GetAttribute<NullableAttribute>(true) != null; }
        static bool IsKeyField(FieldInfo fi) { return fi.GetAttribute<KeyAttribute>(true) != null; }

        static bool IsReferenceType(Type metaColumnType) { return metaColumnType.IsGenericType && metaColumnType.GetGenericTypeDefinition() == typeof(Reference<>); }

        internal string[] KeyTypes
        {
            get
            {
                var keys = Columns
                    .Where(c => c.IsKey)
                    .Select(c => c.Type)
                    .ToArray();
                return keys;
            }
        }

        internal string SQLTableName { get { return Catalog + "." + Schema + "." + Name; } }
    }
}