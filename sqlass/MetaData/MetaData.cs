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
using HWClassLibrary.sqlass.MetaData.SQLCompact;

namespace HWClassLibrary.sqlass.MetaData
{
    sealed class MetaData
    {
        readonly Context _context;
        readonly DictionaryEx<TableName, Table> _tableCache = new DictionaryEx<TableName, Table>();

        public MetaData(Context context) { _context = context; }

        TableColumn[] TableColumns
        {
            get
            {
                if(_context.IsSqlCeConnectionBug)
                {
                    var keys = OneOfKeyColumnUsageClass.Initialize(_context);
                    var primaryKeys = TableConstraintClass
                        .Initialize(_context)
                        .Where(tc => tc.CONSTRAINT_TYPE == "PRIMARY KEY")
                        .SelectMany(tc => keys.Where(k => k.CONSTRAINT_CATALOG == tc.CONSTRAINT_CATALOG && k.CONSTRAINT_SCHEMA == tc.CONSTRAINT_SCHEMA && k.CONSTRAINT_NAME == tc.CONSTRAINT_NAME))
                        .ToArray();
                    var constraint = ReferentialConstraintClass
                        .Initialize(_context)
                        .Select
                        (pc =>
                         new
                         {
                             MainTable = TableName.Find(pc.CONSTRAINT_CATALOG, pc.CONSTRAINT_SCHEMA, pc.CONSTRAINT_TABLE_NAME),
                             Reference = TableName.Find(pc.UNIQUE_CONSTRAINT_CATALOG, pc.UNIQUE_CONSTRAINT_SCHEMA, pc.UNIQUE_CONSTRAINT_TABLE_NAME),
                             ColumnName = keys.Single
                             (k =>
                              k.TABLE_CATALOG == pc.CONSTRAINT_CATALOG
                              && k.TABLE_SCHEMA == pc.CONSTRAINT_SCHEMA
                              && k.TABLE_NAME == pc.CONSTRAINT_TABLE_NAME
                              && k.CONSTRAINT_NAME == pc.CONSTRAINT_NAME)
                             .COLUMN_NAME
                         }
                        )
                        .ToArray();

                    var tableColumns = ColumnClass
                        .Initialize(_context)
                        .Select
                        (c =>
                         new TableColumn
                         {
                             Table = TableName.Find(c.TABLE_CATALOG, c.TABLE_SCHEMA, c.TABLE_NAME),
                             Name = c.COLUMN_NAME,
                             Type = ParseType(c),
                             IsKey = primaryKeys.Any
                                 (k
                                  => k.COLUMN_NAME == c.COLUMN_NAME
                                     && k.TABLE_CATALOG == c.TABLE_CATALOG
                                     && k.TABLE_SCHEMA == c.TABLE_SCHEMA
                                     && k.TABLE_NAME == c.TABLE_NAME
                                 ),
                             IsNullable = c.IS_NULLABLE == "YES"
                         }
                        ).ToArray();

                    foreach(var column in tableColumns)
                    {
                        var thisConstraint = constraint.SingleOrDefault(pc => pc.MainTable == column.Table && pc.ColumnName == column.Name);
                        if(thisConstraint != null)
                            column.Reference = thisConstraint.Reference;
                    }

                    return tableColumns;
                }

                return Ado.ColumnClass.Initialize(_context)
                    .Select
                    (c =>
                     new TableColumn
                     {
                         Table = TableName.Find(c.TABLE_CATALOG, c.TABLE_SCHEMA, c.TABLE_NAME),
                         Name = c.COLUMN_NAME,
                         Type = c.DATA_TYPE,
                         IsKey = c.PRIMARY_KEY,
                         IsNullable = c.IS_NULLABLE
                     }
                    )
                    .ToArray();
            }
        }

        string ParseType(ColumnClass c)
        {
            if(c.DATA_TYPE == "int")
                return c.DATA_TYPE;
            if(c.DATA_TYPE == "nvarchar")
                return c.DATA_TYPE + "(" + c.CHARACTER_MAXIMUM_LENGTH + ")";
            throw new NotImplementedException();
        }

        internal Table[] Tables
        {
            get
            {
                return TableColumns
                    .GroupBy(c => c.Table)
                    .Select(TableFound)
                    .ToArray();
            }
        }

        Column[] CreateTableColumns(IEnumerable<TableColumn> tableColumns)
        {
            return tableColumns
                .Select
                (c =>
                 new Column
                 {
                     Name = c.Name,
                     SQLType = c.Type,
                     IsKey = c.IsKey,
                     IsNullable = c.IsNullable,
                     ReferencedTable = c.Reference == null ? null : _tableCache[c.Reference]
                 }
                )
                .ToArray();
        }

        Table TableFound(IGrouping<TableName, TableColumn> t)
        {
            var result = new Table(t.Key, () => CreateTableColumns(t), null);
            _tableCache.Add(result.TableName, result);
            return result;
        }
    }
}