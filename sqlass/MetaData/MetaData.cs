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

namespace HWClassLibrary.sqlass.MetaData
{
    sealed class MetaData
    {
        readonly Context _context;
        public MetaData(Context context) { _context = context; }

        TableColumn[] TableColumns
        {
            get
            {
                if(_context.IsSqlCeConnectionBug)
                    return SQLCompact.ColumnClass.Initialize(_context)
                        .Select
                        (c =>
                         new TableColumn
                         {
                             Catalog = c.TABLE_CATALOG,
                             Schema = c.TABLE_SCHEMA,
                             TableName = c.TABLE_NAME,
                             Name = c.COLUMN_NAME,
                         }
                        )
                        .ToArray();

                return Ado.ColumnClass.Initialize(_context)
                    .Select
                    (c =>
                     new TableColumn
                     {
                         Catalog = c.TABLE_CATALOG,
                         Schema = c.TABLE_SCHEMA,
                         TableName = c.TABLE_NAME,
                         Name = c.COLUMN_NAME,
                     }
                    )
                    .ToArray();
            }
        }
        internal Table[] Tables
        {
            get
            {
                return TableColumns
                    .GroupBy(c => new {c.Catalog, c.Schema, c.TableName})
                    .Select
                    (
                        t =>
                        new Table
                        {
                            Catalog = t.Key.Catalog,
                            Schema = t.Key.Schema,
                            Name = t.Key.TableName,
                            Columns = t.Select
                                (
                                    c =>
                                    new Column
                                    {
                                        Name = c.Name,
                                    }
                                )
                                .ToArray()
                        }
                    )
                    .ToArray();
            }
        }
    }
}