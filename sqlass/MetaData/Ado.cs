using System.Linq;
using System.Data.Common;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData
{
 
    // MetaDataCollections 0 0  
    // DataSourceInformation 0 0  
    // DataTypes 0 0  
    // ReservedWords 0 0  
    // Catalogs 1 1  
    public sealed class Catalog 
    { 
        public System.String CATALOG_NAME; 
        public System.String DESCRIPTION;

        static public Catalog[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("Catalogs")
                .Select()
                .Select
                (
                    row=>new Catalog 
                    { 
                        CATALOG_NAME = row["CATALOG_NAME"].ToString() , 
                        DESCRIPTION = row["DESCRIPTION"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            CATALOG_NAME.ToString() 
             + " " + DESCRIPTION.ToString();            
        }
    }
 
    // Columns 4 4  
    public sealed class Column 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String COLUMN_NAME; 
        public System.Guid COLUMN_GUID; 
        public System.Int64 COLUMN_PROPID; 
        public System.Int32 ORDINAL_POSITION; 
        public System.Boolean COLUMN_HASDEFAULT;

        static public Column[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("Columns")
                .Select()
                .Select
                (
                    row=>new Column 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        COLUMN_NAME = row["COLUMN_NAME"].ToString() , 
                        COLUMN_GUID = row["COLUMN_GUID"].ToGuid() , 
                        COLUMN_PROPID = row["COLUMN_PROPID"].ToInt64() , 
                        ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32() , 
                        COLUMN_HASDEFAULT = row["COLUMN_HASDEFAULT"].ToBoolean() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + COLUMN_NAME.ToString() 
             + " " + COLUMN_GUID.ToString() 
             + " " + COLUMN_PROPID.ToString() 
             + " " + ORDINAL_POSITION.ToString() 
             + " " + COLUMN_HASDEFAULT.ToString();            
        }
    }
 
    // Indexes 4 3  
    public sealed class Index 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String INDEX_CATALOG; 
        public System.String INDEX_SCHEMA; 
        public System.String INDEX_NAME; 
        public System.Boolean PRIMARY_KEY;

        static public Index[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("Indexes")
                .Select()
                .Select
                (
                    row=>new Index 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        INDEX_CATALOG = row["INDEX_CATALOG"].ToString() , 
                        INDEX_SCHEMA = row["INDEX_SCHEMA"].ToString() , 
                        INDEX_NAME = row["INDEX_NAME"].ToString() , 
                        PRIMARY_KEY = row["PRIMARY_KEY"].ToBoolean() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + INDEX_CATALOG.ToString() 
             + " " + INDEX_SCHEMA.ToString() 
             + " " + INDEX_NAME.ToString() 
             + " " + PRIMARY_KEY.ToString();            
        }
    }
 
    // IndexColumns 5 4  
    public sealed class IndexColumn 
    { 
        public System.String CONSTRAINT_CATALOG; 
        public System.String CONSTRAINT_SCHEMA; 
        public System.String CONSTRAINT_NAME; 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String COLUMN_NAME; 
        public System.Int32 ORDINAL_POSITION; 
        public System.String INDEX_NAME;

        static public IndexColumn[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("IndexColumns")
                .Select()
                .Select
                (
                    row=>new IndexColumn 
                    { 
                        CONSTRAINT_CATALOG = row["CONSTRAINT_CATALOG"].ToString() , 
                        CONSTRAINT_SCHEMA = row["CONSTRAINT_SCHEMA"].ToString() , 
                        CONSTRAINT_NAME = row["CONSTRAINT_NAME"].ToString() , 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        COLUMN_NAME = row["COLUMN_NAME"].ToString() , 
                        ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32() , 
                        INDEX_NAME = row["INDEX_NAME"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            CONSTRAINT_CATALOG.ToString() 
             + " " + CONSTRAINT_SCHEMA.ToString() 
             + " " + CONSTRAINT_NAME.ToString() 
             + " " + TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + COLUMN_NAME.ToString() 
             + " " + ORDINAL_POSITION.ToString() 
             + " " + INDEX_NAME.ToString();            
        }
    }
 
    // Tables 4 3  
    public sealed class Table 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String TABLE_TYPE; 
        public System.Int64 TABLE_ID; 
        public System.Int32 TABLE_ROOTPAGE; 
        public System.String TABLE_DEFINITION;

        static public Table[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("Tables")
                .Select()
                .Select
                (
                    row=>new Table 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        TABLE_TYPE = row["TABLE_TYPE"].ToString() , 
                        TABLE_ID = row["TABLE_ID"].ToInt64() , 
                        TABLE_ROOTPAGE = row["TABLE_ROOTPAGE"].ToInt32() , 
                        TABLE_DEFINITION = row["TABLE_DEFINITION"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + TABLE_TYPE.ToString() 
             + " " + TABLE_ID.ToString() 
             + " " + TABLE_ROOTPAGE.ToString() 
             + " " + TABLE_DEFINITION.ToString();            
        }
    }
 
    // Views 3 3  
    public sealed class View 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String VIEW_DEFINITION; 
        public System.Boolean CHECK_OPTION; 
        public System.Boolean IS_UPDATABLE;

        static public View[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("Views")
                .Select()
                .Select
                (
                    row=>new View 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        VIEW_DEFINITION = row["VIEW_DEFINITION"].ToString() , 
                        CHECK_OPTION = row["CHECK_OPTION"].ToBoolean() , 
                        IS_UPDATABLE = row["IS_UPDATABLE"].ToBoolean() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + VIEW_DEFINITION.ToString() 
             + " " + CHECK_OPTION.ToString() 
             + " " + IS_UPDATABLE.ToString();            
        }
    }
 
    // ViewColumns 4 4  
    public sealed class ViewColumn 
    { 
        public System.String VIEW_CATALOG; 
        public System.String VIEW_SCHEMA; 
        public System.String VIEW_NAME; 
        public System.String VIEW_COLUMN_NAME; 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String COLUMN_NAME;

        static public ViewColumn[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("ViewColumns")
                .Select()
                .Select
                (
                    row=>new ViewColumn 
                    { 
                        VIEW_CATALOG = row["VIEW_CATALOG"].ToString() , 
                        VIEW_SCHEMA = row["VIEW_SCHEMA"].ToString() , 
                        VIEW_NAME = row["VIEW_NAME"].ToString() , 
                        VIEW_COLUMN_NAME = row["VIEW_COLUMN_NAME"].ToString() , 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        COLUMN_NAME = row["COLUMN_NAME"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            VIEW_CATALOG.ToString() 
             + " " + VIEW_SCHEMA.ToString() 
             + " " + VIEW_NAME.ToString() 
             + " " + VIEW_COLUMN_NAME.ToString() 
             + " " + TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + COLUMN_NAME.ToString();            
        }
    }
 
    // ForeignKeys 4 3  
    public sealed class ForeignKey 
    { 
        public System.String CONSTRAINT_CATALOG; 
        public System.String CONSTRAINT_SCHEMA; 
        public System.String CONSTRAINT_NAME; 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String CONSTRAINT_TYPE;

        static public ForeignKey[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("ForeignKeys")
                .Select()
                .Select
                (
                    row=>new ForeignKey 
                    { 
                        CONSTRAINT_CATALOG = row["CONSTRAINT_CATALOG"].ToString() , 
                        CONSTRAINT_SCHEMA = row["CONSTRAINT_SCHEMA"].ToString() , 
                        CONSTRAINT_NAME = row["CONSTRAINT_NAME"].ToString() , 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        CONSTRAINT_TYPE = row["CONSTRAINT_TYPE"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            CONSTRAINT_CATALOG.ToString() 
             + " " + CONSTRAINT_SCHEMA.ToString() 
             + " " + CONSTRAINT_NAME.ToString() 
             + " " + TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + CONSTRAINT_TYPE.ToString();            
        }
    }
 
    // Triggers 4   
    public sealed class Trigger 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String TRIGGER_NAME;

        static public Trigger[] Initialize(DbConnection connection) 
        {
            return connection
                .GetSchema("Triggers")
                .Select()
                .Select
                (
                    row=>new Trigger 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        TRIGGER_NAME = row["TRIGGER_NAME"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG.ToString() 
             + " " + TABLE_SCHEMA.ToString() 
             + " " + TABLE_NAME.ToString() 
             + " " + TRIGGER_NAME.ToString();            
        }
    }

    public sealed class MetaData
    {
        readonly DbConnection _connection;
        public MetaData(DbConnection connection) { _connection = connection; } 
        public Catalog[] Catalogs {get{return Catalog.Initialize(_connection);}} 
        public Column[] Columns {get{return Column.Initialize(_connection);}} 
        public Index[] Indexes {get{return Index.Initialize(_connection);}} 
        public IndexColumn[] IndexColumns {get{return IndexColumn.Initialize(_connection);}} 
        public Table[] Tables {get{return Table.Initialize(_connection);}} 
        public View[] Views {get{return View.Initialize(_connection);}} 
        public ViewColumn[] ViewColumns {get{return ViewColumn.Initialize(_connection);}} 
        public ForeignKey[] ForeignKeys {get{return ForeignKey.Initialize(_connection);}} 
        public Trigger[] Triggers {get{return Trigger.Initialize(_connection);}}
    }
}