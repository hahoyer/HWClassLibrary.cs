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
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TextTemplating;

namespace HWClassLibrary.sqlass.T4
{
    public sealed class Context : HWClassLibrary.T4.Context
    {
        readonly List<SQLTable> _tables = new List<SQLTable>();

        internal Context(StringBuilder text, ITextTemplatingEngineHost host)
            : base(text, host)
        {
            
        }

        [UsedImplicitly]
        public string AddTable<T>(Func<string, string> getTableName = null)
        {
            var table = new SQLTable(this, typeof(T), getTableName);
            _tables.Add(table);
            File = table.FileName;
            return table.TransformText();
        }

        [UsedImplicitly]
        public new void ProcessFiles()
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