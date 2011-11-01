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

namespace HWClassLibrary.sqlass.T4
{
    partial class SQLTable
    {
        readonly Context _context;
        readonly Type _type;
        readonly string _sqlTableName;
        internal SQLTable(Context context, Type type, string tableName)
        {
            _context = context;
            _type = type;
            _sqlTableName = tableName ?? _type.Name;
        }
        internal string ClassName { get { return _type.Name; } }
        internal string FileName { get { return ClassName + ".cs"; } }

        FieldInfo[] Fields { get { return _type.GetFields(); } }

        internal string SQLTableName { get { return _sqlTableName.Quote(); } }

        internal string TableTypeName { get { return "Table<" + ClassName + ">"; } }

        bool IsLastField(FieldInfo fi) { return Fields.Last() == fi; }

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
        
        static bool IsReferenceType(Type type) { return type.IsGenericType && type.Name.StartsWith("Reference"); }

        static string ScopeModifier(FieldInfo fi)
        {
            if(fi.IsPublic)
                return "public ";
            if(!fi.IsPrivate)
                return "internal ";
            return "";
        }

        static string ValueName(FieldInfo fi)
        {
            if (IsReferenceType(fi.FieldType))
                return fi.Name + ".Target";
            return fi.Name;
        }

        bool HasSQLKey { get { return Fields.Any(fi => fi.GetAttribute<KeyAttribute>(true) != null); } }

        string SQLKeyProvider
        {
            get
            {
                var keys = Fields
                    .Where(fi => fi.GetAttribute<KeyAttribute>(true) != null)
                    .Select(fi => NameMapper(fi.FieldType))
                    .ToArray();
                if(keys.Length == 0)
                    return "";
                return "ISQLKeyProvider<" + keys.Format(", ") + ">";
            }
        }

        string SQLKeyType
        {
            get
            {
                var keys = Fields
                    .Where(fi => fi.GetAttribute<KeyAttribute>(true) != null)
                    .Select(fi => NameMapper(fi.FieldType))
                    .ToArray();
                if (keys.Length == 1)
                    return keys[0];
                return "Tuple<" + keys.Format(", ") + ">";
            }
        }

        string SQLKeyValue
        {
            get
            {
                var keys = Fields
                    .Where(fi => fi.GetAttribute<KeyAttribute>(true) != null)
                    .Select(fi => fi.Name)
                    .ToArray();
                if (keys.Length == 1)
                    return keys[0];
                return "new " + SQLKeyType + "(" + keys.Format(", ") + ")";
            }
        }
    }

}