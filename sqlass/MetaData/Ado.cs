
using System.Linq;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData.Ado
{
    public sealed class MetaDataCollectionClass 
    { 
        public System.String CollectionName; 
        public System.Int32 NumberOfRestrictions; 
        public System.Int32 NumberOfIdentifierParts;

        static public MetaDataCollectionClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("MetaDataCollections")
                .Select()
                .Select
                (
                    row=>new MetaDataCollectionClass 
                    { 
                        CollectionName = row["CollectionName"].ToString() , 
                        NumberOfRestrictions = row["NumberOfRestrictions"].ToInt32() , 
                        NumberOfIdentifierParts = row["NumberOfIdentifierParts"].ToInt32() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            CollectionName 
             + " " + NumberOfRestrictions 
             + " " + NumberOfIdentifierParts;            
        }
    }

    public sealed class OneOfDataSourceInformationClass 
    { 
        public System.String CompositeIdentifierSeparatorPattern; 
        public System.String DataSourceProductName; 
        public System.String DataSourceProductVersion; 
        public System.String DataSourceProductVersionNormalized; 
        public System.Int32 GroupByBehavior; 
        public System.String IdentifierPattern; 
        public System.Int32 IdentifierCase; 
        public System.Boolean OrderByColumnsInSelect; 
        public System.String ParameterMarkerFormat; 
        public System.String ParameterMarkerPattern; 
        public System.Int32 ParameterNameMaxLength; 
        public System.String ParameterNamePattern; 
        public System.String QuotedIdentifierPattern; 
        public System.Int32 QuotedIdentifierCase; 
        public System.String StatementSeparatorPattern; 
        public System.String StringLiteralPattern; 
        public System.Int32 SupportedJoinOperators;

        static public OneOfDataSourceInformationClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("DataSourceInformation")
                .Select()
                .Select
                (
                    row=>new OneOfDataSourceInformationClass 
                    { 
                        CompositeIdentifierSeparatorPattern = row["CompositeIdentifierSeparatorPattern"].ToString() , 
                        DataSourceProductName = row["DataSourceProductName"].ToString() , 
                        DataSourceProductVersion = row["DataSourceProductVersion"].ToString() , 
                        DataSourceProductVersionNormalized = row["DataSourceProductVersionNormalized"].ToString() , 
                        GroupByBehavior = row["GroupByBehavior"].ToInt32() , 
                        IdentifierPattern = row["IdentifierPattern"].ToString() , 
                        IdentifierCase = row["IdentifierCase"].ToInt32() , 
                        OrderByColumnsInSelect = row["OrderByColumnsInSelect"].ToBoolean() , 
                        ParameterMarkerFormat = row["ParameterMarkerFormat"].ToString() , 
                        ParameterMarkerPattern = row["ParameterMarkerPattern"].ToString() , 
                        ParameterNameMaxLength = row["ParameterNameMaxLength"].ToInt32() , 
                        ParameterNamePattern = row["ParameterNamePattern"].ToString() , 
                        QuotedIdentifierPattern = row["QuotedIdentifierPattern"].ToString() , 
                        QuotedIdentifierCase = row["QuotedIdentifierCase"].ToInt32() , 
                        StatementSeparatorPattern = row["StatementSeparatorPattern"].ToString() , 
                        StringLiteralPattern = row["StringLiteralPattern"].ToString() , 
                        SupportedJoinOperators = row["SupportedJoinOperators"].ToInt32() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            CompositeIdentifierSeparatorPattern 
             + " " + DataSourceProductName 
             + " " + DataSourceProductVersion 
             + " " + DataSourceProductVersionNormalized 
             + " " + GroupByBehavior 
             + " " + IdentifierPattern 
             + " " + IdentifierCase 
             + " " + OrderByColumnsInSelect 
             + " " + ParameterMarkerFormat 
             + " " + ParameterMarkerPattern 
             + " " + ParameterNameMaxLength 
             + " " + ParameterNamePattern 
             + " " + QuotedIdentifierPattern 
             + " " + QuotedIdentifierCase 
             + " " + StatementSeparatorPattern 
             + " " + StringLiteralPattern 
             + " " + SupportedJoinOperators;            
        }
    }

    public sealed class DataTypeClass 
    { 
        public System.String TypeName; 
        public System.Int32 ProviderDbType; 
        public System.Int64 ColumnSize; 
        public System.String CreateFormat; 
        public System.String CreateParameters; 
        public System.String DataType; 
        public System.Boolean IsAutoIncrementable; 
        public System.Boolean IsBestMatch; 
        public System.Boolean IsCaseSensitive; 
        public System.Boolean IsFixedLength; 
        public System.Boolean IsFixedPrecisionScale; 
        public System.Boolean IsLong; 
        public System.Boolean IsNullable; 
        public System.Boolean IsSearchable; 
        public System.Boolean IsSearchableWithLike; 
        public System.Boolean IsLiteralSupported; 
        public System.String LiteralPrefix; 
        public System.String LiteralSuffix; 
        public System.Boolean IsUnsigned; 
        public System.Int16 MaximumScale; 
        public System.Int16 MinimumScale; 
        public System.Boolean IsConcurrencyType;

        static public DataTypeClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("DataTypes")
                .Select()
                .Select
                (
                    row=>new DataTypeClass 
                    { 
                        TypeName = row["TypeName"].ToString() , 
                        ProviderDbType = row["ProviderDbType"].ToInt32() , 
                        ColumnSize = row["ColumnSize"].ToInt64() , 
                        CreateFormat = row["CreateFormat"].ToString() , 
                        CreateParameters = row["CreateParameters"].ToString() , 
                        DataType = row["DataType"].ToString() , 
                        IsAutoIncrementable = row["IsAutoIncrementable"].ToBoolean() , 
                        IsBestMatch = row["IsBestMatch"].ToBoolean() , 
                        IsCaseSensitive = row["IsCaseSensitive"].ToBoolean() , 
                        IsFixedLength = row["IsFixedLength"].ToBoolean() , 
                        IsFixedPrecisionScale = row["IsFixedPrecisionScale"].ToBoolean() , 
                        IsLong = row["IsLong"].ToBoolean() , 
                        IsNullable = row["IsNullable"].ToBoolean() , 
                        IsSearchable = row["IsSearchable"].ToBoolean() , 
                        IsSearchableWithLike = row["IsSearchableWithLike"].ToBoolean() , 
                        IsLiteralSupported = row["IsLiteralSupported"].ToBoolean() , 
                        LiteralPrefix = row["LiteralPrefix"].ToString() , 
                        LiteralSuffix = row["LiteralSuffix"].ToString() , 
                        IsUnsigned = row["IsUnsigned"].ToBoolean() , 
                        MaximumScale = row["MaximumScale"].ToInt16() , 
                        MinimumScale = row["MinimumScale"].ToInt16() , 
                        IsConcurrencyType = row["IsConcurrencyType"].ToBoolean() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TypeName 
             + " " + ProviderDbType 
             + " " + ColumnSize 
             + " " + CreateFormat 
             + " " + CreateParameters 
             + " " + DataType 
             + " " + IsAutoIncrementable 
             + " " + IsBestMatch 
             + " " + IsCaseSensitive 
             + " " + IsFixedLength 
             + " " + IsFixedPrecisionScale 
             + " " + IsLong 
             + " " + IsNullable 
             + " " + IsSearchable 
             + " " + IsSearchableWithLike 
             + " " + IsLiteralSupported 
             + " " + LiteralPrefix 
             + " " + LiteralSuffix 
             + " " + IsUnsigned 
             + " " + MaximumScale 
             + " " + MinimumScale 
             + " " + IsConcurrencyType;            
        }
    }

    public sealed class ReservedWordClass 
    { 
        public System.String ReservedWord; 
        public System.String MaximumVersion; 
        public System.String MinimumVersion;

        static public ReservedWordClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("ReservedWords")
                .Select()
                .Select
                (
                    row=>new ReservedWordClass 
                    { 
                        ReservedWord = row["ReservedWord"].ToString() , 
                        MaximumVersion = row["MaximumVersion"].ToString() , 
                        MinimumVersion = row["MinimumVersion"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            ReservedWord 
             + " " + MaximumVersion 
             + " " + MinimumVersion;            
        }
    }

    public sealed class CatalogClass 
    { 
        public System.String CATALOG_NAME; 
        public System.String DESCRIPTION; 
        public System.Int64 ID;

        static public CatalogClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("Catalogs")
                .Select()
                .Select
                (
                    row=>new CatalogClass 
                    { 
                        CATALOG_NAME = row["CATALOG_NAME"].ToString() , 
                        DESCRIPTION = row["DESCRIPTION"].ToString() , 
                        ID = row["ID"].ToInt64() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            CATALOG_NAME 
             + " " + DESCRIPTION 
             + " " + ID;            
        }
    }

    public sealed class ColumnClass 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String COLUMN_NAME; 
        public System.Guid COLUMN_GUID; 
        public System.Int64 COLUMN_PROPID; 
        public System.Int32 ORDINAL_POSITION; 
        public System.Boolean COLUMN_HASDEFAULT; 
        public System.String COLUMN_DEFAULT; 
        public System.Int64 COLUMN_FLAGS; 
        public System.Boolean IS_NULLABLE; 
        public System.String DATA_TYPE; 
        public System.Guid TYPE_GUID; 
        public System.Int32 CHARACTER_MAXIMUM_LENGTH; 
        public System.Int32 CHARACTER_OCTET_LENGTH; 
        public System.Int32 NUMERIC_PRECISION; 
        public System.Int32 NUMERIC_SCALE; 
        public System.Int64 DATETIME_PRECISION; 
        public System.String CHARACTER_SET_CATALOG; 
        public System.String CHARACTER_SET_SCHEMA; 
        public System.String CHARACTER_SET_NAME; 
        public System.String COLLATION_CATALOG; 
        public System.String COLLATION_SCHEMA; 
        public System.String COLLATION_NAME; 
        public System.String DOMAIN_CATALOG; 
        public System.String DOMAIN_NAME; 
        public System.String DESCRIPTION; 
        public System.Boolean PRIMARY_KEY; 
        public System.String EDM_TYPE; 
        public System.Boolean AUTOINCREMENT; 
        public System.Boolean UNIQUE;

        static public ColumnClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("Columns")
                .Select()
                .Select
                (
                    row=>new ColumnClass 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        COLUMN_NAME = row["COLUMN_NAME"].ToString() , 
                        COLUMN_GUID = row["COLUMN_GUID"].ToGuid() , 
                        COLUMN_PROPID = row["COLUMN_PROPID"].ToInt64() , 
                        ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32() , 
                        COLUMN_HASDEFAULT = row["COLUMN_HASDEFAULT"].ToBoolean() , 
                        COLUMN_DEFAULT = row["COLUMN_DEFAULT"].ToString() , 
                        COLUMN_FLAGS = row["COLUMN_FLAGS"].ToInt64() , 
                        IS_NULLABLE = row["IS_NULLABLE"].ToBoolean() , 
                        DATA_TYPE = row["DATA_TYPE"].ToString() , 
                        TYPE_GUID = row["TYPE_GUID"].ToGuid() , 
                        CHARACTER_MAXIMUM_LENGTH = row["CHARACTER_MAXIMUM_LENGTH"].ToInt32() , 
                        CHARACTER_OCTET_LENGTH = row["CHARACTER_OCTET_LENGTH"].ToInt32() , 
                        NUMERIC_PRECISION = row["NUMERIC_PRECISION"].ToInt32() , 
                        NUMERIC_SCALE = row["NUMERIC_SCALE"].ToInt32() , 
                        DATETIME_PRECISION = row["DATETIME_PRECISION"].ToInt64() , 
                        CHARACTER_SET_CATALOG = row["CHARACTER_SET_CATALOG"].ToString() , 
                        CHARACTER_SET_SCHEMA = row["CHARACTER_SET_SCHEMA"].ToString() , 
                        CHARACTER_SET_NAME = row["CHARACTER_SET_NAME"].ToString() , 
                        COLLATION_CATALOG = row["COLLATION_CATALOG"].ToString() , 
                        COLLATION_SCHEMA = row["COLLATION_SCHEMA"].ToString() , 
                        COLLATION_NAME = row["COLLATION_NAME"].ToString() , 
                        DOMAIN_CATALOG = row["DOMAIN_CATALOG"].ToString() , 
                        DOMAIN_NAME = row["DOMAIN_NAME"].ToString() , 
                        DESCRIPTION = row["DESCRIPTION"].ToString() , 
                        PRIMARY_KEY = row["PRIMARY_KEY"].ToBoolean() , 
                        EDM_TYPE = row["EDM_TYPE"].ToString() , 
                        AUTOINCREMENT = row["AUTOINCREMENT"].ToBoolean() , 
                        UNIQUE = row["UNIQUE"].ToBoolean() ,            
                    }
                )
                .ToArray();
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
             + " " + DOMAIN_NAME 
             + " " + DESCRIPTION 
             + " " + PRIMARY_KEY 
             + " " + EDM_TYPE 
             + " " + AUTOINCREMENT 
             + " " + UNIQUE;            
        }
    }

    public sealed class IndexClass 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String INDEX_CATALOG; 
        public System.String INDEX_SCHEMA; 
        public System.String INDEX_NAME; 
        public System.Boolean PRIMARY_KEY; 
        public System.Boolean UNIQUE; 
        public System.Boolean CLUSTERED; 
        public System.Int32 TYPE; 
        public System.Int32 FILL_FACTOR; 
        public System.Int32 INITIAL_SIZE; 
        public System.Int32 NULLS; 
        public System.Boolean SORT_BOOKMARKS; 
        public System.Boolean AUTO_UPDATE; 
        public System.Int32 NULL_COLLATION; 
        public System.Int32 ORDINAL_POSITION; 
        public System.String COLUMN_NAME; 
        public System.Guid COLUMN_GUID; 
        public System.Int64 COLUMN_PROPID; 
        public System.Int16 COLLATION; 
        public System.Decimal CARDINALITY; 
        public System.Int32 PAGES; 
        public System.String FILTER_CONDITION; 
        public System.Boolean INTEGRATED; 
        public System.String INDEX_DEFINITION;

        static public IndexClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("Indexes")
                .Select()
                .Select
                (
                    row=>new IndexClass 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        INDEX_CATALOG = row["INDEX_CATALOG"].ToString() , 
                        INDEX_SCHEMA = row["INDEX_SCHEMA"].ToString() , 
                        INDEX_NAME = row["INDEX_NAME"].ToString() , 
                        PRIMARY_KEY = row["PRIMARY_KEY"].ToBoolean() , 
                        UNIQUE = row["UNIQUE"].ToBoolean() , 
                        CLUSTERED = row["CLUSTERED"].ToBoolean() , 
                        TYPE = row["TYPE"].ToInt32() , 
                        FILL_FACTOR = row["FILL_FACTOR"].ToInt32() , 
                        INITIAL_SIZE = row["INITIAL_SIZE"].ToInt32() , 
                        NULLS = row["NULLS"].ToInt32() , 
                        SORT_BOOKMARKS = row["SORT_BOOKMARKS"].ToBoolean() , 
                        AUTO_UPDATE = row["AUTO_UPDATE"].ToBoolean() , 
                        NULL_COLLATION = row["NULL_COLLATION"].ToInt32() , 
                        ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32() , 
                        COLUMN_NAME = row["COLUMN_NAME"].ToString() , 
                        COLUMN_GUID = row["COLUMN_GUID"].ToGuid() , 
                        COLUMN_PROPID = row["COLUMN_PROPID"].ToInt64() , 
                        COLLATION = row["COLLATION"].ToInt16() , 
                        CARDINALITY = row["CARDINALITY"].ToDecimal() , 
                        PAGES = row["PAGES"].ToInt32() , 
                        FILTER_CONDITION = row["FILTER_CONDITION"].ToString() , 
                        INTEGRATED = row["INTEGRATED"].ToBoolean() , 
                        INDEX_DEFINITION = row["INDEX_DEFINITION"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG 
             + " " + TABLE_SCHEMA 
             + " " + TABLE_NAME 
             + " " + INDEX_CATALOG 
             + " " + INDEX_SCHEMA 
             + " " + INDEX_NAME 
             + " " + PRIMARY_KEY 
             + " " + UNIQUE 
             + " " + CLUSTERED 
             + " " + TYPE 
             + " " + FILL_FACTOR 
             + " " + INITIAL_SIZE 
             + " " + NULLS 
             + " " + SORT_BOOKMARKS 
             + " " + AUTO_UPDATE 
             + " " + NULL_COLLATION 
             + " " + ORDINAL_POSITION 
             + " " + COLUMN_NAME 
             + " " + COLUMN_GUID 
             + " " + COLUMN_PROPID 
             + " " + COLLATION 
             + " " + CARDINALITY 
             + " " + PAGES 
             + " " + FILTER_CONDITION 
             + " " + INTEGRATED 
             + " " + INDEX_DEFINITION;            
        }
    }

    public sealed class IndexColumnClass 
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
        public System.String COLLATION_NAME; 
        public System.String SORT_MODE; 
        public System.Int32 CONFLICT_OPTION;

        static public IndexColumnClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("IndexColumns")
                .Select()
                .Select
                (
                    row=>new IndexColumnClass 
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
                        COLLATION_NAME = row["COLLATION_NAME"].ToString() , 
                        SORT_MODE = row["SORT_MODE"].ToString() , 
                        CONFLICT_OPTION = row["CONFLICT_OPTION"].ToInt32() ,            
                    }
                )
                .ToArray();
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
             + " " + ORDINAL_POSITION 
             + " " + INDEX_NAME 
             + " " + COLLATION_NAME 
             + " " + SORT_MODE 
             + " " + CONFLICT_OPTION;            
        }
    }

    public sealed class TableClass 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String TABLE_TYPE; 
        public System.Int64 TABLE_ID; 
        public System.Int32 TABLE_ROOTPAGE; 
        public System.String TABLE_DEFINITION;

        static public TableClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("Tables")
                .Select()
                .Select
                (
                    row=>new TableClass 
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
            TABLE_CATALOG 
             + " " + TABLE_SCHEMA 
             + " " + TABLE_NAME 
             + " " + TABLE_TYPE 
             + " " + TABLE_ID 
             + " " + TABLE_ROOTPAGE 
             + " " + TABLE_DEFINITION;            
        }
    }

    public sealed class ViewClass 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String VIEW_DEFINITION; 
        public System.Boolean CHECK_OPTION; 
        public System.Boolean IS_UPDATABLE; 
        public System.String DESCRIPTION; 
        public System.DateTime DATE_CREATED; 
        public System.DateTime DATE_MODIFIED;

        static public ViewClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("Views")
                .Select()
                .Select
                (
                    row=>new ViewClass 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        VIEW_DEFINITION = row["VIEW_DEFINITION"].ToString() , 
                        CHECK_OPTION = row["CHECK_OPTION"].ToBoolean() , 
                        IS_UPDATABLE = row["IS_UPDATABLE"].ToBoolean() , 
                        DESCRIPTION = row["DESCRIPTION"].ToString() , 
                        DATE_CREATED = row["DATE_CREATED"].ToDateTime() , 
                        DATE_MODIFIED = row["DATE_MODIFIED"].ToDateTime() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG 
             + " " + TABLE_SCHEMA 
             + " " + TABLE_NAME 
             + " " + VIEW_DEFINITION 
             + " " + CHECK_OPTION 
             + " " + IS_UPDATABLE 
             + " " + DESCRIPTION 
             + " " + DATE_CREATED 
             + " " + DATE_MODIFIED;            
        }
    }

    public sealed class ViewColumnClass 
    { 
        public System.String VIEW_CATALOG; 
        public System.String VIEW_SCHEMA; 
        public System.String VIEW_NAME; 
        public System.String VIEW_COLUMN_NAME; 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String COLUMN_NAME; 
        public System.Int32 ORDINAL_POSITION; 
        public System.Boolean COLUMN_HASDEFAULT; 
        public System.String COLUMN_DEFAULT; 
        public System.Int64 COLUMN_FLAGS; 
        public System.Boolean IS_NULLABLE; 
        public System.String DATA_TYPE; 
        public System.Int32 CHARACTER_MAXIMUM_LENGTH; 
        public System.Int32 NUMERIC_PRECISION; 
        public System.Int32 NUMERIC_SCALE; 
        public System.Int64 DATETIME_PRECISION; 
        public System.String CHARACTER_SET_CATALOG; 
        public System.String CHARACTER_SET_SCHEMA; 
        public System.String CHARACTER_SET_NAME; 
        public System.String COLLATION_CATALOG; 
        public System.String COLLATION_SCHEMA; 
        public System.String COLLATION_NAME; 
        public System.Boolean PRIMARY_KEY; 
        public System.String EDM_TYPE; 
        public System.Boolean AUTOINCREMENT; 
        public System.Boolean UNIQUE;

        static public ViewColumnClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("ViewColumns")
                .Select()
                .Select
                (
                    row=>new ViewColumnClass 
                    { 
                        VIEW_CATALOG = row["VIEW_CATALOG"].ToString() , 
                        VIEW_SCHEMA = row["VIEW_SCHEMA"].ToString() , 
                        VIEW_NAME = row["VIEW_NAME"].ToString() , 
                        VIEW_COLUMN_NAME = row["VIEW_COLUMN_NAME"].ToString() , 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        COLUMN_NAME = row["COLUMN_NAME"].ToString() , 
                        ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32() , 
                        COLUMN_HASDEFAULT = row["COLUMN_HASDEFAULT"].ToBoolean() , 
                        COLUMN_DEFAULT = row["COLUMN_DEFAULT"].ToString() , 
                        COLUMN_FLAGS = row["COLUMN_FLAGS"].ToInt64() , 
                        IS_NULLABLE = row["IS_NULLABLE"].ToBoolean() , 
                        DATA_TYPE = row["DATA_TYPE"].ToString() , 
                        CHARACTER_MAXIMUM_LENGTH = row["CHARACTER_MAXIMUM_LENGTH"].ToInt32() , 
                        NUMERIC_PRECISION = row["NUMERIC_PRECISION"].ToInt32() , 
                        NUMERIC_SCALE = row["NUMERIC_SCALE"].ToInt32() , 
                        DATETIME_PRECISION = row["DATETIME_PRECISION"].ToInt64() , 
                        CHARACTER_SET_CATALOG = row["CHARACTER_SET_CATALOG"].ToString() , 
                        CHARACTER_SET_SCHEMA = row["CHARACTER_SET_SCHEMA"].ToString() , 
                        CHARACTER_SET_NAME = row["CHARACTER_SET_NAME"].ToString() , 
                        COLLATION_CATALOG = row["COLLATION_CATALOG"].ToString() , 
                        COLLATION_SCHEMA = row["COLLATION_SCHEMA"].ToString() , 
                        COLLATION_NAME = row["COLLATION_NAME"].ToString() , 
                        PRIMARY_KEY = row["PRIMARY_KEY"].ToBoolean() , 
                        EDM_TYPE = row["EDM_TYPE"].ToString() , 
                        AUTOINCREMENT = row["AUTOINCREMENT"].ToBoolean() , 
                        UNIQUE = row["UNIQUE"].ToBoolean() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            VIEW_CATALOG 
             + " " + VIEW_SCHEMA 
             + " " + VIEW_NAME 
             + " " + VIEW_COLUMN_NAME 
             + " " + TABLE_CATALOG 
             + " " + TABLE_SCHEMA 
             + " " + TABLE_NAME 
             + " " + COLUMN_NAME 
             + " " + ORDINAL_POSITION 
             + " " + COLUMN_HASDEFAULT 
             + " " + COLUMN_DEFAULT 
             + " " + COLUMN_FLAGS 
             + " " + IS_NULLABLE 
             + " " + DATA_TYPE 
             + " " + CHARACTER_MAXIMUM_LENGTH 
             + " " + NUMERIC_PRECISION 
             + " " + NUMERIC_SCALE 
             + " " + DATETIME_PRECISION 
             + " " + CHARACTER_SET_CATALOG 
             + " " + CHARACTER_SET_SCHEMA 
             + " " + CHARACTER_SET_NAME 
             + " " + COLLATION_CATALOG 
             + " " + COLLATION_SCHEMA 
             + " " + COLLATION_NAME 
             + " " + PRIMARY_KEY 
             + " " + EDM_TYPE 
             + " " + AUTOINCREMENT 
             + " " + UNIQUE;            
        }
    }

    public sealed class ForeignKeyClass 
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
        public System.String FKEY_FROM_COLUMN; 
        public System.Int32 FKEY_FROM_ORDINAL_POSITION; 
        public System.String FKEY_TO_CATALOG; 
        public System.String FKEY_TO_SCHEMA; 
        public System.String FKEY_TO_TABLE; 
        public System.String FKEY_TO_COLUMN;

        static public ForeignKeyClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("ForeignKeys")
                .Select()
                .Select
                (
                    row=>new ForeignKeyClass 
                    { 
                        CONSTRAINT_CATALOG = row["CONSTRAINT_CATALOG"].ToString() , 
                        CONSTRAINT_SCHEMA = row["CONSTRAINT_SCHEMA"].ToString() , 
                        CONSTRAINT_NAME = row["CONSTRAINT_NAME"].ToString() , 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        CONSTRAINT_TYPE = row["CONSTRAINT_TYPE"].ToString() , 
                        IS_DEFERRABLE = row["IS_DEFERRABLE"].ToBoolean() , 
                        INITIALLY_DEFERRED = row["INITIALLY_DEFERRED"].ToBoolean() , 
                        FKEY_FROM_COLUMN = row["FKEY_FROM_COLUMN"].ToString() , 
                        FKEY_FROM_ORDINAL_POSITION = row["FKEY_FROM_ORDINAL_POSITION"].ToInt32() , 
                        FKEY_TO_CATALOG = row["FKEY_TO_CATALOG"].ToString() , 
                        FKEY_TO_SCHEMA = row["FKEY_TO_SCHEMA"].ToString() , 
                        FKEY_TO_TABLE = row["FKEY_TO_TABLE"].ToString() , 
                        FKEY_TO_COLUMN = row["FKEY_TO_COLUMN"].ToString() ,            
                    }
                )
                .ToArray();
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
             + " " + FKEY_FROM_COLUMN 
             + " " + FKEY_FROM_ORDINAL_POSITION 
             + " " + FKEY_TO_CATALOG 
             + " " + FKEY_TO_SCHEMA 
             + " " + FKEY_TO_TABLE 
             + " " + FKEY_TO_COLUMN;            
        }
    }

    public sealed class TriggerClass 
    { 
        public System.String TABLE_CATALOG; 
        public System.String TABLE_SCHEMA; 
        public System.String TABLE_NAME; 
        public System.String TRIGGER_NAME; 
        public System.String TRIGGER_DEFINITION;

        static public TriggerClass[] Initialize(Context context) 
        {
            return context
                .Connection
                .GetSchema("Triggers")
                .Select()
                .Select
                (
                    row=>new TriggerClass 
                    { 
                        TABLE_CATALOG = row["TABLE_CATALOG"].ToString() , 
                        TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString() , 
                        TABLE_NAME = row["TABLE_NAME"].ToString() , 
                        TRIGGER_NAME = row["TRIGGER_NAME"].ToString() , 
                        TRIGGER_DEFINITION = row["TRIGGER_DEFINITION"].ToString() ,            
                    }
                )
                .ToArray();
        }

        public override string ToString()
        {
            return 
            TABLE_CATALOG 
             + " " + TABLE_SCHEMA 
             + " " + TABLE_NAME 
             + " " + TRIGGER_NAME 
             + " " + TRIGGER_DEFINITION;            
        }
    }

}
