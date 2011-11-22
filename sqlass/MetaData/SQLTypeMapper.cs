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

using System.Collections.Generic;
using System;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData
{
    sealed class SQLTypeMapper
    {
        readonly DictionaryEx<Type, string> _typeToString = new DictionaryEx<Type, string>();
        readonly DictionaryEx<string, Type> _stringToType = new DictionaryEx<string, Type>();
                                                                        
        public SQLTypeMapper()
        {
            Add(typeof(int), "int");
            Add(typeof(string), "nvarchar(4000)");
        }

        void Add(Type type, string sqlType)
        {
            _typeToString.Add(type, sqlType);
            _stringToType.Add(sqlType, type);
        }

        public static readonly SQLTypeMapper Instance = new SQLTypeMapper();
        public string Find(Type type) { return _typeToString[type]; }
        public Type Find(string value) { return _stringToType[value]; }
    }
}