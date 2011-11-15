
using System.Data.Common;
using System.Linq;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData.SQLCompact
{
    public sealed class TableClass : IReaderInitialize
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String TABLE_TYPE; 
        public System.Guid TABLE_GUID; 
        public System.String DESCRIPTION; 
        public System.Int32 TABLE_PROPID; 
        public System.DateTime DATE_CREATED; 
        public System.DateTime DATE_MODIFIED;

        static public TableClass[] Initialize(Context context) 
        {
            return context
                .Execute<TableClass>("SELECT * FROM INFORMATION_SCHEMA.Tables")
                .ToArray();
        }

        public void Initialize(DbDataReader reader) 
        { 
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<System.String>(); 
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<System.String>(); 
            TABLE_NAME = reader["TABLE_NAME"].Convert<System.String>(); 
            TABLE_TYPE = reader["TABLE_TYPE"].Convert<System.String>(); 
            TABLE_GUID = reader["TABLE_GUID"].Convert<System.Guid>(); 
            DESCRIPTION = reader["DESCRIPTION"].Convert<System.String>(); 
            TABLE_PROPID = reader["TABLE_PROPID"].Convert<System.Int32>(); 
            DATE_CREATED = reader["DATE_CREATED"].Convert<System.DateTime>(); 
            DATE_MODIFIED = reader["DATE_MODIFIED"].Convert<System.DateTime>();            
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
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String COLUMN_NAME; 
        public System.Guid COLUMN_GUID; 
        public System.Int32 COLUMN_PROPID; 
        public System.Int32 ORDINAL_POSITION; 
        public System.Boolean COLUMN_HASDEFAULT; 
        public System.String COLUMN_DEFAULT; 
        public System.Int32 COLUMN_FLAGS; 
        public System.String IS_NULLABLE; 
        public System.String DATA_TYPE; 
        public System.Guid TYPE_GUID; 
        public System.Int32 CHARACTER_MAXIMUM_LENGTH; 
        public System.Int32 CHARACTER_OCTET_LENGTH; 
        public System.Int16 NUMERIC_PRECISION; 
        public System.Int16 NUMERIC_SCALE; 
        public System.Int32 DATETIME_PRECISION; 
        public System.String CHARACTER_SET_CATALOG; 
        public System.String CHARACTER_SET_SCHEMA; 
        public System.String CHARACTER_SET_NAME; 
        public System.String COLLATION_CATALOG; 
        public System.String COLLATION_SCHEMA; 
        public System.String COLLATION_NAME; 
        public System.String DOMAIN_CATALOG; 
        public System.String DOMAIN_SCHEMA; 
        public System.String DOMAIN_NAME; 
        public System.String DESCRIPTION; 
        public System.Int64 AUTOINC_MIN; 
        public System.Int64 AUTOINC_MAX; 
        public System.Int64 AUTOINC_NEXT; 
        public System.Int64 AUTOINC_SEED; 
        public System.Int64 AUTOINC_INCREMENT;

        static public ColumnClass[] Initialize(Context context) 
        {
            return context
                .Execute<ColumnClass>("SELECT * FROM INFORMATION_SCHEMA.Columns")
                .ToArray();
        }

        public void Initialize(DbDataReader reader) 
        { 
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<System.String>(); 
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<System.String>(); 
            TABLE_NAME = reader["TABLE_NAME"].Convert<System.String>(); 
            COLUMN_NAME = reader["COLUMN_NAME"].Convert<System.String>(); 
            COLUMN_GUID = reader["COLUMN_GUID"].Convert<System.Guid>(); 
            COLUMN_PROPID = reader["COLUMN_PROPID"].Convert<System.Int32>(); 
            ORDINAL_POSITION = reader["ORDINAL_POSITION"].Convert<System.Int32>(); 
            COLUMN_HASDEFAULT = reader["COLUMN_HASDEFAULT"].Convert<System.Boolean>(); 
            COLUMN_DEFAULT = reader["COLUMN_DEFAULT"].Convert<System.String>(); 
            COLUMN_FLAGS = reader["COLUMN_FLAGS"].Convert<System.Int32>(); 
            IS_NULLABLE = reader["IS_NULLABLE"].Convert<System.String>(); 
            DATA_TYPE = reader["DATA_TYPE"].Convert<System.String>(); 
            TYPE_GUID = reader["TYPE_GUID"].Convert<System.Guid>(); 
            CHARACTER_MAXIMUM_LENGTH = reader["CHARACTER_MAXIMUM_LENGTH"].Convert<System.Int32>(); 
            CHARACTER_OCTET_LENGTH = reader["CHARACTER_OCTET_LENGTH"].Convert<System.Int32>(); 
            NUMERIC_PRECISION = reader["NUMERIC_PRECISION"].Convert<System.Int16>(); 
            NUMERIC_SCALE = reader["NUMERIC_SCALE"].Convert<System.Int16>(); 
            DATETIME_PRECISION = reader["DATETIME_PRECISION"].Convert<System.Int32>(); 
            CHARACTER_SET_CATALOG = reader["CHARACTER_SET_CATALOG"].Convert<System.String>(); 
            CHARACTER_SET_SCHEMA = reader["CHARACTER_SET_SCHEMA"].Convert<System.String>(); 
            CHARACTER_SET_NAME = reader["CHARACTER_SET_NAME"].Convert<System.String>(); 
            COLLATION_CATALOG = reader["COLLATION_CATALOG"].Convert<System.String>(); 
            COLLATION_SCHEMA = reader["COLLATION_SCHEMA"].Convert<System.String>(); 
            COLLATION_NAME = reader["COLLATION_NAME"].Convert<System.String>(); 
            DOMAIN_CATALOG = reader["DOMAIN_CATALOG"].Convert<System.String>(); 
            DOMAIN_SCHEMA = reader["DOMAIN_SCHEMA"].Convert<System.String>(); 
            DOMAIN_NAME = reader["DOMAIN_NAME"].Convert<System.String>(); 
            DESCRIPTION = reader["DESCRIPTION"].Convert<System.String>(); 
            AUTOINC_MIN = reader["AUTOINC_MIN"].Convert<System.Int64>(); 
            AUTOINC_MAX = reader["AUTOINC_MAX"].Convert<System.Int64>(); 
            AUTOINC_NEXT = reader["AUTOINC_NEXT"].Convert<System.Int64>(); 
            AUTOINC_SEED = reader["AUTOINC_SEED"].Convert<System.Int64>(); 
            AUTOINC_INCREMENT = reader["AUTOINC_INCREMENT"].Convert<System.Int64>();            
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
        public System.String CONSTRAINT_CATALOG; 
        public System.String CONSTRAINT_SCHEMA; 
        public System.String CONSTRAINT_NAME; 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String COLUMN_NAME; 
        public System.Guid COLUMN_GUID; 
        public System.Int32 COLUMN_PROPID; 
        public System.Int32 ORDINAL_POSITION;

        static public OneOfKeyColumnUsageClass[] Initialize(Context context) 
        {
            return context
                .Execute<OneOfKeyColumnUsageClass>("SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE")
                .ToArray();
        }

        public void Initialize(DbDataReader reader) 
        { 
            CONSTRAINT_CATALOG = reader["CONSTRAINT_CATALOG"].Convert<System.String>(); 
            CONSTRAINT_SCHEMA = reader["CONSTRAINT_SCHEMA"].Convert<System.String>(); 
            CONSTRAINT_NAME = reader["CONSTRAINT_NAME"].Convert<System.String>(); 
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<System.String>(); 
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<System.String>(); 
            TABLE_NAME = reader["TABLE_NAME"].Convert<System.String>(); 
            COLUMN_NAME = reader["COLUMN_NAME"].Convert<System.String>(); 
            COLUMN_GUID = reader["COLUMN_GUID"].Convert<System.Guid>(); 
            COLUMN_PROPID = reader["COLUMN_PROPID"].Convert<System.Int32>(); 
            ORDINAL_POSITION = reader["ORDINAL_POSITION"].Convert<System.Int32>();            
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
        public System.String CONSTRAINT_CATALOG; 
        public System.String CONSTRAINT_SCHEMA; 
        public System.String CONSTRAINT_TABLE_NAME; 
        public System.String CONSTRAINT_NAME; 
        public System.String UNIQUE_CONSTRAINT_CATALOG; 
        public System.String UNIQUE_CONSTRAINT_SCHEMA; 
        public System.String UNIQUE_CONSTRAINT_TABLE_NAME; 
        public System.String UNIQUE_CONSTRAINT_NAME; 
        public System.String MATCH_OPTION; 
        public System.String UPDATE_RULE; 
        public System.String DELETE_RULE; 
        public System.String DESCRIPTION;

        static public ReferentialConstraintClass[] Initialize(Context context) 
        {
            return context
                .Execute<ReferentialConstraintClass>("SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS")
                .ToArray();
        }

        public void Initialize(DbDataReader reader) 
        { 
            CONSTRAINT_CATALOG = reader["CONSTRAINT_CATALOG"].Convert<System.String>(); 
            CONSTRAINT_SCHEMA = reader["CONSTRAINT_SCHEMA"].Convert<System.String>(); 
            CONSTRAINT_TABLE_NAME = reader["CONSTRAINT_TABLE_NAME"].Convert<System.String>(); 
            CONSTRAINT_NAME = reader["CONSTRAINT_NAME"].Convert<System.String>(); 
            UNIQUE_CONSTRAINT_CATALOG = reader["UNIQUE_CONSTRAINT_CATALOG"].Convert<System.String>(); 
            UNIQUE_CONSTRAINT_SCHEMA = reader["UNIQUE_CONSTRAINT_SCHEMA"].Convert<System.String>(); 
            UNIQUE_CONSTRAINT_TABLE_NAME = reader["UNIQUE_CONSTRAINT_TABLE_NAME"].Convert<System.String>(); 
            UNIQUE_CONSTRAINT_NAME = reader["UNIQUE_CONSTRAINT_NAME"].Convert<System.String>(); 
            MATCH_OPTION = reader["MATCH_OPTION"].Convert<System.String>(); 
            UPDATE_RULE = reader["UPDATE_RULE"].Convert<System.String>(); 
            DELETE_RULE = reader["DELETE_RULE"].Convert<System.String>(); 
            DESCRIPTION = reader["DESCRIPTION"].Convert<System.String>();            
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
        public System.String CONSTRAINT_CATALOG; 
        public System.String CONSTRAINT_SCHEMA; 
        public System.String CONSTRAINT_NAME; 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String CONSTRAINT_TYPE; 
        public System.Boolean IS_DEFERRABLE; 
        public System.Boolean INITIALLY_DEFERRED; 
        public System.String DESCRIPTION;

        static public TableConstraintClass[] Initialize(Context context) 
        {
            return context
                .Execute<TableConstraintClass>("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS")
                .ToArray();
        }

        public void Initialize(DbDataReader reader) 
        { 
            CONSTRAINT_CATALOG = reader["CONSTRAINT_CATALOG"].Convert<System.String>(); 
            CONSTRAINT_SCHEMA = reader["CONSTRAINT_SCHEMA"].Convert<System.String>(); 
            CONSTRAINT_NAME = reader["CONSTRAINT_NAME"].Convert<System.String>(); 
            TABLE_CATALOG = reader["TABLE_CATALOG"].Convert<System.String>(); 
            TABLE_SCHEMA = reader["TABLE_SCHEMA"].Convert<System.String>(); 
            TABLE_NAME = reader["TABLE_NAME"].Convert<System.String>(); 
            CONSTRAINT_TYPE = reader["CONSTRAINT_TYPE"].Convert<System.String>(); 
            IS_DEFERRABLE = reader["IS_DEFERRABLE"].Convert<System.Boolean>(); 
            INITIALLY_DEFERRED = reader["INITIALLY_DEFERRED"].Convert<System.Boolean>(); 
            DESCRIPTION = reader["DESCRIPTION"].Convert<System.String>();            
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
