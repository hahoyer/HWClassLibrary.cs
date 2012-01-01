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
using System.Data.Common;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData.SQLCompact
{
    public sealed class TableClass : IReaderInitialize
    {
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String TABLE_TYPE;
        public Guid TABLE_GUID;
        public String DESCRIPTION;
        public Int32 TABLE_PROPID;
        public DateTime DATE_CREATED;
        public DateTime DATE_MODIFIED;

        public static TableClass[] Initialize(Context context)
        {
            return context
                .Execute<TableClass>("SELECT * FROM INFORMATION_SCHEMA.Tables")
                .ToArray();
        }

        public void Initialize(DbDataReader reader)
        {
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<String>();
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<String>();
            TABLE_NAME = reader["TABLE_NAME"].Convert<String>();
            TABLE_TYPE = reader["TABLE_TYPE"].Convert<String>();
            TABLE_GUID = reader["TABLE_GUID"].Convert<Guid>();
            DESCRIPTION = reader["DESCRIPTION"].Convert<String>();
            TABLE_PROPID = reader["TABLE_PROPID"].Convert<Int32>();
            DATE_CREATED = reader["DATE_CREATED"].Convert<DateTime>();
            DATE_MODIFIED = reader["DATE_MODIFIED"].Convert<DateTime>();
        }

        public override string ToString()
        {
            return
                TABLE_CATALOG
                + " " + TABLE_SCHEMA
                + " " + TABLE_NAME
                + " " + TABLE_TYPE
                + " " + TABLE_GUID
                + " " + DESCRIPTION
                + " " + TABLE_PROPID
                + " " + DATE_CREATED
                + " " + DATE_MODIFIED;
        }
    }

    public sealed class ColumnClass : IReaderInitialize
    {
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String COLUMN_NAME;
        public Guid COLUMN_GUID;
        public Int32 COLUMN_PROPID;
        public Int32 ORDINAL_POSITION;
        public Boolean COLUMN_HASDEFAULT;
        public String COLUMN_DEFAULT;
        public Int32 COLUMN_FLAGS;
        public String IS_NULLABLE;
        public String DATA_TYPE;
        public Guid TYPE_GUID;
        public Int32 CHARACTER_MAXIMUM_LENGTH;
        public Int32 CHARACTER_OCTET_LENGTH;
        public Int16 NUMERIC_PRECISION;
        public Int16 NUMERIC_SCALE;
        public Int32 DATETIME_PRECISION;
        public String CHARACTER_SET_CATALOG;
        public String CHARACTER_SET_SCHEMA;
        public String CHARACTER_SET_NAME;
        public String COLLATION_CATALOG;
        public String COLLATION_SCHEMA;
        public String COLLATION_NAME;
        public String DOMAIN_CATALOG;
        public String DOMAIN_SCHEMA;
        public String DOMAIN_NAME;
        public String DESCRIPTION;
        public Int64 AUTOINC_MIN;
        public Int64 AUTOINC_MAX;
        public Int64 AUTOINC_NEXT;
        public Int64 AUTOINC_SEED;
        public Int64 AUTOINC_INCREMENT;

        public static ColumnClass[] Initialize(Context context)
        {
            return context
                .Execute<ColumnClass>("SELECT * FROM INFORMATION_SCHEMA.Columns")
                .ToArray();
        }

        public void Initialize(DbDataReader reader)
        {
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<String>();
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<String>();
            TABLE_NAME = reader["TABLE_NAME"].Convert<String>();
            COLUMN_NAME = reader["COLUMN_NAME"].Convert<String>();
            COLUMN_GUID = reader["COLUMN_GUID"].Convert<Guid>();
            COLUMN_PROPID = reader["COLUMN_PROPID"].Convert<Int32>();
            ORDINAL_POSITION = reader["ORDINAL_POSITION"].Convert<Int32>();
            COLUMN_HASDEFAULT = reader["COLUMN_HASDEFAULT"].Convert<Boolean>();
            COLUMN_DEFAULT = reader["COLUMN_DEFAULT"].Convert<String>();
            COLUMN_FLAGS = reader["COLUMN_FLAGS"].Convert<Int32>();
            IS_NULLABLE = reader["IS_NULLABLE"].Convert<String>();
            DATA_TYPE = reader["DATA_TYPE"].Convert<String>();
            TYPE_GUID = reader["TYPE_GUID"].Convert<Guid>();
            CHARACTER_MAXIMUM_LENGTH = reader["CHARACTER_MAXIMUM_LENGTH"].Convert<Int32>();
            CHARACTER_OCTET_LENGTH = reader["CHARACTER_OCTET_LENGTH"].Convert<Int32>();
            NUMERIC_PRECISION = reader["NUMERIC_PRECISION"].Convert<Int16>();
            NUMERIC_SCALE = reader["NUMERIC_SCALE"].Convert<Int16>();
            DATETIME_PRECISION = reader["DATETIME_PRECISION"].Convert<Int32>();
            CHARACTER_SET_CATALOG = reader["CHARACTER_SET_CATALOG"].Convert<String>();
            CHARACTER_SET_SCHEMA = reader["CHARACTER_SET_SCHEMA"].Convert<String>();
            CHARACTER_SET_NAME = reader["CHARACTER_SET_NAME"].Convert<String>();
            COLLATION_CATALOG = reader["COLLATION_CATALOG"].Convert<String>();
            COLLATION_SCHEMA = reader["COLLATION_SCHEMA"].Convert<String>();
            COLLATION_NAME = reader["COLLATION_NAME"].Convert<String>();
            DOMAIN_CATALOG = reader["DOMAIN_CATALOG"].Convert<String>();
            DOMAIN_SCHEMA = reader["DOMAIN_SCHEMA"].Convert<String>();
            DOMAIN_NAME = reader["DOMAIN_NAME"].Convert<String>();
            DESCRIPTION = reader["DESCRIPTION"].Convert<String>();
            AUTOINC_MIN = reader["AUTOINC_MIN"].Convert<Int64>();
            AUTOINC_MAX = reader["AUTOINC_MAX"].Convert<Int64>();
            AUTOINC_NEXT = reader["AUTOINC_NEXT"].Convert<Int64>();
            AUTOINC_SEED = reader["AUTOINC_SEED"].Convert<Int64>();
            AUTOINC_INCREMENT = reader["AUTOINC_INCREMENT"].Convert<Int64>();
        }

        public override string ToString()
        {
            return
                TABLE_CATALOG
                + " " + TABLE_SCHEMA
                + " " + TABLE_NAME
                + " " + COLUMN_NAME
                + " " + COLUMN_GUID
                + " " + COLUMN_PROPID
                + " " + ORDINAL_POSITION
                + " " + COLUMN_HASDEFAULT
                + " " + COLUMN_DEFAULT
                + " " + COLUMN_FLAGS
                + " " + IS_NULLABLE
                + " " + DATA_TYPE
                + " " + TYPE_GUID
                + " " + CHARACTER_MAXIMUM_LENGTH
                + " " + CHARACTER_OCTET_LENGTH
                + " " + NUMERIC_PRECISION
                + " " + NUMERIC_SCALE
                + " " + DATETIME_PRECISION
                + " " + CHARACTER_SET_CATALOG
                + " " + CHARACTER_SET_SCHEMA
                + " " + CHARACTER_SET_NAME
                + " " + COLLATION_CATALOG
                + " " + COLLATION_SCHEMA
                + " " + COLLATION_NAME
                + " " + DOMAIN_CATALOG
                + " " + DOMAIN_SCHEMA
                + " " + DOMAIN_NAME
                + " " + DESCRIPTION
                + " " + AUTOINC_MIN
                + " " + AUTOINC_MAX
                + " " + AUTOINC_NEXT
                + " " + AUTOINC_SEED
                + " " + AUTOINC_INCREMENT;
        }
    }

    public sealed class OneOfKeyColumnUsageClass : IReaderInitialize
    {
        public String CONSTRAINT_CATALOG;
        public String CONSTRAINT_SCHEMA;
        public String CONSTRAINT_NAME;
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String COLUMN_NAME;
        public Guid COLUMN_GUID;
        public Int32 COLUMN_PROPID;
        public Int32 ORDINAL_POSITION;

        public static OneOfKeyColumnUsageClass[] Initialize(Context context)
        {
            return context
                .Execute<OneOfKeyColumnUsageClass>("SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE")
                .ToArray();
        }

        public void Initialize(DbDataReader reader)
        {
            CONSTRAINT_CATALOG = reader["CONSTRAINT_CATALOG"].Convert<String>();
            CONSTRAINT_SCHEMA = reader["CONSTRAINT_SCHEMA"].Convert<String>();
            CONSTRAINT_NAME = reader["CONSTRAINT_NAME"].Convert<String>();
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<String>();
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<String>();
            TABLE_NAME = reader["TABLE_NAME"].Convert<String>();
            COLUMN_NAME = reader["COLUMN_NAME"].Convert<String>();
            COLUMN_GUID = reader["COLUMN_GUID"].Convert<Guid>();
            COLUMN_PROPID = reader["COLUMN_PROPID"].Convert<Int32>();
            ORDINAL_POSITION = reader["ORDINAL_POSITION"].Convert<Int32>();
        }

        public override string ToString()
        {
            return
                CONSTRAINT_CATALOG
                + " " + CONSTRAINT_SCHEMA
                + " " + CONSTRAINT_NAME
                + " " + TABLE_CATALOG
                + " " + TABLE_SCHEMA
                + " " + TABLE_NAME
                + " " + COLUMN_NAME
                + " " + COLUMN_GUID
                + " " + COLUMN_PROPID
                + " " + ORDINAL_POSITION;
        }
    }

    public sealed class ReferentialConstraintClass : IReaderInitialize
    {
        public String CONSTRAINT_CATALOG;
        public String CONSTRAINT_SCHEMA;
        public String CONSTRAINT_TABLE_NAME;
        public String CONSTRAINT_NAME;
        public String UNIQUE_CONSTRAINT_CATALOG;
        public String UNIQUE_CONSTRAINT_SCHEMA;
        public String UNIQUE_CONSTRAINT_TABLE_NAME;
        public String UNIQUE_CONSTRAINT_NAME;
        public String MATCH_OPTION;
        public String UPDATE_RULE;
        public String DELETE_RULE;
        public String DESCRIPTION;

        public static ReferentialConstraintClass[] Initialize(Context context)
        {
            return context
                .Execute<ReferentialConstraintClass>("SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS")
                .ToArray();
        }

        public void Initialize(DbDataReader reader)
        {
            CONSTRAINT_CATALOG = reader["CONSTRAINT_CATALOG"].Convert<String>();
            CONSTRAINT_SCHEMA = reader["CONSTRAINT_SCHEMA"].Convert<String>();
            CONSTRAINT_TABLE_NAME = reader["CONSTRAINT_TABLE_NAME"].Convert<String>();
            CONSTRAINT_NAME = reader["CONSTRAINT_NAME"].Convert<String>();
            UNIQUE_CONSTRAINT_CATALOG = reader["UNIQUE_CONSTRAINT_CATALOG"].Convert<String>();
            UNIQUE_CONSTRAINT_SCHEMA = reader["UNIQUE_CONSTRAINT_SCHEMA"].Convert<String>();
            UNIQUE_CONSTRAINT_TABLE_NAME = reader["UNIQUE_CONSTRAINT_TABLE_NAME"].Convert<String>();
            UNIQUE_CONSTRAINT_NAME = reader["UNIQUE_CONSTRAINT_NAME"].Convert<String>();
            MATCH_OPTION = reader["MATCH_OPTION"].Convert<String>();
            UPDATE_RULE = reader["UPDATE_RULE"].Convert<String>();
            DELETE_RULE = reader["DELETE_RULE"].Convert<String>();
            DESCRIPTION = reader["DESCRIPTION"].Convert<String>();
        }

        public override string ToString()
        {
            return
                CONSTRAINT_CATALOG
                + " " + CONSTRAINT_SCHEMA
                + " " + CONSTRAINT_TABLE_NAME
                + " " + CONSTRAINT_NAME
                + " " + UNIQUE_CONSTRAINT_CATALOG
                + " " + UNIQUE_CONSTRAINT_SCHEMA
                + " " + UNIQUE_CONSTRAINT_TABLE_NAME
                + " " + UNIQUE_CONSTRAINT_NAME
                + " " + MATCH_OPTION
                + " " + UPDATE_RULE
                + " " + DELETE_RULE
                + " " + DESCRIPTION;
        }
    }

    public sealed class TableConstraintClass : IReaderInitialize
    {
        public String CONSTRAINT_CATALOG;
        public String CONSTRAINT_SCHEMA;
        public String CONSTRAINT_NAME;
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String CONSTRAINT_TYPE;
        public Boolean IS_DEFERRABLE;
        public Boolean INITIALLY_DEFERRED;
        public String DESCRIPTION;

        public static TableConstraintClass[] Initialize(Context context)
        {
            return context
                .Execute<TableConstraintClass>("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS")
                .ToArray();
        }

        public void Initialize(DbDataReader reader)
        {
            CONSTRAINT_CATALOG = reader["CONSTRAINT_CATALOG"].Convert<String>();
            CONSTRAINT_SCHEMA = reader["CONSTRAINT_SCHEMA"].Convert<String>();
            CONSTRAINT_NAME = reader["CONSTRAINT_NAME"].Convert<String>();
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<String>();
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<String>();
            TABLE_NAME = reader["TABLE_NAME"].Convert<String>();
            CONSTRAINT_TYPE = reader["CONSTRAINT_TYPE"].Convert<String>();
            IS_DEFERRABLE = reader["IS_DEFERRABLE"].Convert<Boolean>();
            INITIALLY_DEFERRED = reader["INITIALLY_DEFERRED"].Convert<Boolean>();
            DESCRIPTION = reader["DESCRIPTION"].Convert<String>();
        }

        public override string ToString()
        {
            return
                CONSTRAINT_CATALOG
                + " " + CONSTRAINT_SCHEMA
                + " " + CONSTRAINT_NAME
                + " " + TABLE_CATALOG
                + " " + TABLE_SCHEMA
                + " " + TABLE_NAME
                + " " + CONSTRAINT_TYPE
                + " " + IS_DEFERRABLE
                + " " + INITIALLY_DEFERRED
                + " " + DESCRIPTION;
        }
    }
}