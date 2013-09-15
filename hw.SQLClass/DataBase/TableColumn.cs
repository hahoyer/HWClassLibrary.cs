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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;

namespace HWClassLibrary.DataBase
{
    sealed class TableColumn : Dumpable
    {
        readonly FieldInfo _fieldInfo;
        public string Name { get { return _fieldInfo.Name; } }
        public Type Type { get { return _fieldInfo.FieldType; } }

        public TableColumn(FieldInfo fieldInfo) { _fieldInfo = fieldInfo; }

        public string CreateTableCommand { get { return Name + " " + SQLType; } }

        string SQLType
        {
            get
            {
                if(Type == typeof(string))
                    return "TEXT";
                throw new NotImplementedException();
            }
        }

        public string ValueAsLiteral(object o)
        {
            var value = _fieldInfo.GetValue(o);
            var result = value.ToString();
            if(SQLType == "TEXT")
                result = "'" + result.Replace("'", "''") + "'";
            return result;
        }

        public void Value(object o, object value) { _fieldInfo.SetValue(o, value); }
    }
}