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
using System.Text;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TextTemplating;

namespace HWClassLibrary.sqlass.T4
{
    public sealed class Context : HWClassLibrary.T4.Context
    {
        readonly List<SQLTable> _tables = new List<SQLTable>();
        readonly DictionaryEx<Type, SQLTable> _sqlTables;

        [UsedImplicitly]
        public static void Generate(StringBuilder text, ITextTemplatingEngineHost host, TextTransformation frame)
        {
            var context = new Context(text, host);
            foreach(var type in frame.GetType().GetNestedTypes())
                context.AddTable(type);
            context.ÂddContext();
        }

        Context(StringBuilder text, ITextTemplatingEngineHost host)
            : base(text, host) { _sqlTables = new DictionaryEx<Type, SQLTable>(ObtainSQLTable); }

        internal SQLTable SQLTable(Type type) { return _sqlTables.Find(type); }

        void AddTable(Type type)
        {
            var table = ObtainSQLTable(type);
            _tables.Add(table);
            File = table.FileName;
            AppendText(table.TransformText());
        }

        SQLTable ObtainSQLTable(Type type)
        {
            var attribute = type.GetAttribute<TableNameAttribute>(false);
            var tableName = attribute == null ? null : attribute.Name;
            return new SQLTable(this, type, tableName);
        }

        void ÂddContext()
        {
            File = null;
            AppendText(new SQLContext(this, _tables.ToArray()).TransformText());
            base.ProcessFiles();
        }
    }

    public sealed class Reference<T>
    {
        readonly T _target;
        public Reference(T target) { _target = target; }
    }
}