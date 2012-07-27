#region Copyright (C) 2012

//     Project HWClassLibrary
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData
{
    public sealed class TableName : Dumpable
    {
        public readonly string Catalog;
        public readonly string Schema;
        public readonly string Name;

        [DisableDump]
        internal string NameSpace
        {
            get
            {
                if(Catalog == "" && Schema == "")
                    return "";
                return Catalog + "." + Schema;
            }
        }

        [DisableDump]
        internal string SQLTableName
        {
            get
            {
                var result = NameSpace;
                if(result == "")
                    return Name;
                return NameSpace + "." + Name;
            }
        }

        static readonly DictionaryEx<string, DictionaryEx<string, DictionaryEx<string, TableName>>> _dictionary =
            new DictionaryEx<string, DictionaryEx<string, DictionaryEx<string, TableName>>>
                (c => new DictionaryEx<string, DictionaryEx<string, TableName>>
                          (s => new DictionaryEx<string, TableName>
                                    (n => new TableName(c, s, n))
                          )
                );

        TableName(string catalog, string schema, string name)
        {
            Catalog = catalog;
            Schema = schema;
            Name = name;
        }

        public static TableName Find(string catalog, string schema, string tableName)
        {
            return _dictionary
                [catalog ?? ""]
                [schema ?? ""]
                [tableName];
        }

        public override string ToString() { return SQLTableName; }
    }
}