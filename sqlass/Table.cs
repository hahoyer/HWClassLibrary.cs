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

namespace HWClassLibrary.sqlass
{
    partial class Table
    {
        readonly Context _context;
        readonly Type _type;
        readonly string _sqlTableName;
        internal Table(Context context, Type type, Func<string, string> getTableName)
        {
            _context = context;
            _type = type;
            _sqlTableName = getTableName == null ? _type.Name : getTableName(_type.Name);
        }
        string ClassName { get { return _type.Name; } }
        internal string FileName { get { return ClassName + ".cs"; } }

        string Fields
        {
            get
            {
                return _type
                    .GetFields()
                    .Select(Field)
                    .Aggregate("", (s, n) => s + n);
            }
        }

        internal string FieldDeclaration
        {
            get
            {
                var result = "";
                result += "\n";
                result += "public ";
                result += TableTypeName;
                result += " ";
                result += ClassName;
                result += ";";
                return result;
            }
        }

        internal string FieldInitializer
        {
            get
            {
                var result = "";
                result += "\n";
                result += ClassName;
                result += " = new ";
                result += TableTypeName;
                result += "(this,";
                result += _sqlTableName.Quote();
                result += ");";
                return result;
            }
        }

        string TableTypeName { get { return "Table<" + ClassName + ">"; } }

        static string Field(FieldInfo fi)
        {
            var result = "";
            result += "\n";
            result += ScopeModifier(fi);
            result += NameMapper(fi.FieldType);
            result += " ";
            result += fi.Name;
            result += ";";
            return result;
        }

        static string NameMapper(Type type)
        {
            if(type == typeof(int))
                return "int";
            if(type == typeof(string))
                return "string";
            if(type.IsGenericType && type.Name.StartsWith("Reference"))
                return "Reference<" + type.GetGenericArguments()[0].Name + ">";
            return type.FullName;
        }

        static string ScopeModifier(FieldInfo fi)
        {
            if(fi.IsPublic)
                return "public ";
            if(!fi.IsPrivate)
                return "internal ";
            return "";
        }
    }
}