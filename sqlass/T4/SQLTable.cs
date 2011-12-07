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
        internal Table Table { get { return _table; } }
        internal string ClassName { get { return _table.TableName.Name; } }
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

        bool IsLast(Column column) { return Columns.Last() == column; }
        internal string TableTypeName { get { return "Table<{0}, {1}>".ReplaceArgs(ClassName, KeyType); } }
        bool HasSQLKey { get { return Columns.Any(column => column.IsKey); } }
        string KeyProvider { get
        {
            //Tracer.LaunchDebugger();
            return _table.KeyProvider.PrettyName();
        } }
        string KeyType { get { return _table.KeyType.PrettyName(); } }
        string KeyValue { get { return _table.KeyValue; } }
        Column[] Columns { get { return _table.Columns; } }
        internal string SQLTableName { get { return _table.SQLTableName; } }
    }
}