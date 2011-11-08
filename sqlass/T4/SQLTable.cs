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

using System.Reflection;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.sqlass.MetaData;

namespace HWClassLibrary.sqlass.T4
{
    partial class SQLTable
    {
        readonly Context _context;
        readonly Table _table;
        internal SQLTable(Context context, Table table)
        {
            _context = context;
            _table = table;
        }
        internal string ClassName { get { return _table.Name; } }
        internal string FileName { get { return ClassName + ".cs"; } }

        internal string NameSpaceSuffix
        {
            get
            {
                var nameSpace = _table.NameSpace;
                if(nameSpace == "")
                    return "";
                return "." + nameSpace;
            }
        }

        static Type KeyType(Type type) { return Table.KeyType(type); }
        bool IsLast(Column column) { return Columns.Last() == column; }

        internal string TableTypeName { get { return "Table<" + ClassName + ">"; } }

        string NameMapper(Type type)
        {
            if(type == typeof(int))
                return "int";
            if(type == typeof(string))
                return "string";
            if(IsReferenceType(type))
                return type.GetGenericArguments()[0].Name;
            return type.FullName;
        }

        string SQLTypeMapper(Type type)
        {
            if(type == typeof(int))
                return "int";
            if(type == typeof(string))
                return "nvarchar(4000)";
            if(IsReferenceType(type))
                return SQLTypeMapper(KeyType(type));
            return type.FullName;
        }

        static bool IsReferenceType(Type type) { return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Reference<>); }

        static string ValueName(FieldInfo fi)
        {
            if(IsReferenceType(fi.FieldType))
                return fi.Name + ".Target";
            return fi.Name;
        }

        bool HasSQLKey { get { return Columns.Any(column => column.IsKey); } }

        string SQLKeyProvider
        {
            get
            {
                var keys = KeyTypes;
                if(keys.Length == 0)
                    return "";
                return "ISQLKeyProvider<" + keys.Format(", ") + ">";
            }
        }
        string[] KeyTypes { get { return _table.KeyTypes; } }

        string SQLKeyType
        {
            get
            {
                var keys = KeyTypes;
                if(keys.Length == 1)
                    return keys[0];
                return "Tuple<" + keys.Format(", ") + ">";
            }
        }

        string SQLKeyValue
        {
            get
            {
                var keys = Columns
                    .Where(c => c.IsKey)
                    .Select(c => c.Name)
                    .ToArray();
                if(keys.Length == 1)
                    return keys[0];
                return "new " + SQLKeyType + "(" + keys.Format(", ") + ")";
            }
        }

        Column[] Columns { get { return _table.Columns; } }
        internal string SQLTableName { get { return _table.SQLTableName; } }
    }
}