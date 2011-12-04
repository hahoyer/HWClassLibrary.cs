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
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData
{
    public sealed class Column : Dumpable
    {
        public string Name;                                                                    
        public Type Type;
        public bool IsKey;
        public bool IsNullable;
        public Table ReferencedTable;

        [DisableDump]
        public string FieldTypeName { get { return ReferencedTable != null ? ReferencedTable.Name : Type.PrettyName(); } }

        [DisableDump]
        public string CreateColumnSQL
        {
            get
            {
                var result = Name + " " + SQLType;
                if(IsNullable)
                    result += " null";
                else
                    result += " not null";
                return
                    result;
            }
        }

        [DisableDump]
        public string SQLType { get { return SQLTypeMapper.Instance.Find(Type); } set { Type = SQLTypeMapper.Instance.Find(value); } }

        public bool DiffersFrom(Column other)
        {
            if (Type != other.Type)
                return true;
            if (IsKey != other.IsKey)
                return true;
            if (IsNullable != other.IsNullable)
                return true;
            if (ReferencedTable == null)
                return other.ReferencedTable != null;
            return ReferencedTable.DiffersFrom(other.ReferencedTable);
        }
    }
}