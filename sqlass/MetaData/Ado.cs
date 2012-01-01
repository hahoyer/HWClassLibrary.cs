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
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.MetaData.Ado
{
    public sealed class MetaDataCollectionClass
    {
        public String CollectionName;
        public Int32 NumberOfRestrictions;
        public Int32 NumberOfIdentifierParts;

        public static MetaDataCollectionClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("MetaDataCollections")
                .Select()
                .Select
                (
                    row => new MetaDataCollectionClass
                           {
                               CollectionName = row["CollectionName"].ToString(),
                               NumberOfRestrictions = row["NumberOfRestrictions"].ToInt32(),
                               NumberOfIdentifierParts = row["NumberOfIdentifierParts"].ToInt32(),
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
        public String CompositeIdentifierSeparatorPattern;
        public String DataSourceProductName;
        public String DataSourceProductVersion;
        public String DataSourceProductVersionNormalized;
        public Int32 GroupByBehavior;
        public String IdentifierPattern;
        public Int32 IdentifierCase;
        public Boolean OrderByColumnsInSelect;
        public String ParameterMarkerFormat;
        public String ParameterMarkerPattern;
        public Int32 ParameterNameMaxLength;
        public String ParameterNamePattern;
        public String QuotedIdentifierPattern;
        public Int32 QuotedIdentifierCase;
        public String StatementSeparatorPattern;
        public String StringLiteralPattern;
        public Int32 SupportedJoinOperators;

        public static OneOfDataSourceInformationClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("DataSourceInformation")
                .Select()
                .Select
                (
                    row => new OneOfDataSourceInformationClass
                           {
                               CompositeIdentifierSeparatorPattern = row["CompositeIdentifierSeparatorPattern"].ToString(),
                               DataSourceProductName = row["DataSourceProductName"].ToString(),
                               DataSourceProductVersion = row["DataSourceProductVersion"].ToString(),
                               DataSourceProductVersionNormalized = row["DataSourceProductVersionNormalized"].ToString(),
                               GroupByBehavior = row["GroupByBehavior"].ToInt32(),
                               IdentifierPattern = row["IdentifierPattern"].ToString(),
                               IdentifierCase = row["IdentifierCase"].ToInt32(),
                               OrderByColumnsInSelect = row["OrderByColumnsInSelect"].ToBoolean(),
                               ParameterMarkerFormat = row["ParameterMarkerFormat"].ToString(),
                               ParameterMarkerPattern = row["ParameterMarkerPattern"].ToString(),
                               ParameterNameMaxLength = row["ParameterNameMaxLength"].ToInt32(),
                               ParameterNamePattern = row["ParameterNamePattern"].ToString(),
                               QuotedIdentifierPattern = row["QuotedIdentifierPattern"].ToString(),
                               QuotedIdentifierCase = row["QuotedIdentifierCase"].ToInt32(),
                               StatementSeparatorPattern = row["StatementSeparatorPattern"].ToString(),
                               StringLiteralPattern = row["StringLiteralPattern"].ToString(),
                               SupportedJoinOperators = row["SupportedJoinOperators"].ToInt32(),
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
        public String TypeName;
        public Int32 ProviderDbType;
        public Int64 ColumnSize;
        public String CreateFormat;
        public String CreateParameters;
        public String DataType;
        public Boolean IsAutoIncrementable;
        public Boolean IsBestMatch;
        public Boolean IsCaseSensitive;
        public Boolean IsFixedLength;
        public Boolean IsFixedPrecisionScale;
        public Boolean IsLong;
        public Boolean IsNullable;
        public Boolean IsSearchable;
        public Boolean IsSearchableWithLike;
        public Boolean IsLiteralSupported;
        public String LiteralPrefix;
        public String LiteralSuffix;
        public Boolean IsUnsigned;
        public Int16 MaximumScale;
        public Int16 MinimumScale;
        public Boolean IsConcurrencyType;

        public static DataTypeClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("DataTypes")
                .Select()
                .Select
                (
                    row => new DataTypeClass
                           {
                               TypeName = row["TypeName"].ToString(),
                               ProviderDbType = row["ProviderDbType"].ToInt32(),
                               ColumnSize = row["ColumnSize"].ToInt64(),
                               CreateFormat = row["CreateFormat"].ToString(),
                               CreateParameters = row["CreateParameters"].ToString(),
                               DataType = row["DataType"].ToString(),
                               IsAutoIncrementable = row["IsAutoIncrementable"].ToBoolean(),
                               IsBestMatch = row["IsBestMatch"].ToBoolean(),
                               IsCaseSensitive = row["IsCaseSensitive"].ToBoolean(),
                               IsFixedLength = row["IsFixedLength"].ToBoolean(),
                               IsFixedPrecisionScale = row["IsFixedPrecisionScale"].ToBoolean(),
                               IsLong = row["IsLong"].ToBoolean(),
                               IsNullable = row["IsNullable"].ToBoolean(),
                               IsSearchable = row["IsSearchable"].ToBoolean(),
                               IsSearchableWithLike = row["IsSearchableWithLike"].ToBoolean(),
                               IsLiteralSupported = row["IsLiteralSupported"].ToBoolean(),
                               LiteralPrefix = row["LiteralPrefix"].ToString(),
                               LiteralSuffix = row["LiteralSuffix"].ToString(),
                               IsUnsigned = row["IsUnsigned"].ToBoolean(),
                               MaximumScale = row["MaximumScale"].ToInt16(),
                               MinimumScale = row["MinimumScale"].ToInt16(),
                               IsConcurrencyType = row["IsConcurrencyType"].ToBoolean(),
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
        public String ReservedWord;
        public String MaximumVersion;
        public String MinimumVersion;

        public static ReservedWordClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("ReservedWords")
                .Select()
                .Select
                (
                    row => new ReservedWordClass
                           {
                               ReservedWord = row["ReservedWord"].ToString(),
                               MaximumVersion = row["MaximumVersion"].ToString(),
                               MinimumVersion = row["MinimumVersion"].ToString(),
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
        public String CATALOG_NAME;
        public String DESCRIPTION;
        public Int64 ID;

        public static CatalogClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("Catalogs")
                .Select()
                .Select
                (
                    row => new CatalogClass
                           {
                               CATALOG_NAME = row["CATALOG_NAME"].ToString(),
                               DESCRIPTION = row["DESCRIPTION"].ToString(),
                               ID = row["ID"].ToInt64(),
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
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String COLUMN_NAME;
        public Guid COLUMN_GUID;
        public Int64 COLUMN_PROPID;
        public Int32 ORDINAL_POSITION;
        public Boolean COLUMN_HASDEFAULT;
        public String COLUMN_DEFAULT;
        public Int64 COLUMN_FLAGS;
        public Boolean IS_NULLABLE;
        public String DATA_TYPE;
        public Guid TYPE_GUID;
        public Int32 CHARACTER_MAXIMUM_LENGTH;
        public Int32 CHARACTER_OCTET_LENGTH;
        public Int32 NUMERIC_PRECISION;
        public Int32 NUMERIC_SCALE;
        public Int64 DATETIME_PRECISION;
        public String CHARACTER_SET_CATALOG;
        public String CHARACTER_SET_SCHEMA;
        public String CHARACTER_SET_NAME;
        public String COLLATION_CATALOG;
        public String COLLATION_SCHEMA;
        public String COLLATION_NAME;
        public String DOMAIN_CATALOG;
        public String DOMAIN_NAME;
        public String DESCRIPTION;
        public Boolean PRIMARY_KEY;
        public String EDM_TYPE;
        public Boolean AUTOINCREMENT;
        public Boolean UNIQUE;

        public static ColumnClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("Columns")
                .Select()
                .Select
                (
                    row => new ColumnClass
                           {
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               COLUMN_NAME = row["COLUMN_NAME"].ToString(),
                               COLUMN_GUID = row["COLUMN_GUID"].ToGuid(),
                               COLUMN_PROPID = row["COLUMN_PROPID"].ToInt64(),
                               ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32(),
                               COLUMN_HASDEFAULT = row["COLUMN_HASDEFAULT"].ToBoolean(),
                               COLUMN_DEFAULT = row["COLUMN_DEFAULT"].ToString(),
                               COLUMN_FLAGS = row["COLUMN_FLAGS"].ToInt64(),
                               IS_NULLABLE = row["IS_NULLABLE"].ToBoolean(),
                               DATA_TYPE = row["DATA_TYPE"].ToString(),
                               TYPE_GUID = row["TYPE_GUID"].ToGuid(),
                               CHARACTER_MAXIMUM_LENGTH = row["CHARACTER_MAXIMUM_LENGTH"].ToInt32(),
                               CHARACTER_OCTET_LENGTH = row["CHARACTER_OCTET_LENGTH"].ToInt32(),
                               NUMERIC_PRECISION = row["NUMERIC_PRECISION"].ToInt32(),
                               NUMERIC_SCALE = row["NUMERIC_SCALE"].ToInt32(),
                               DATETIME_PRECISION = row["DATETIME_PRECISION"].ToInt64(),
                               CHARACTER_SET_CATALOG = row["CHARACTER_SET_CATALOG"].ToString(),
                               CHARACTER_SET_SCHEMA = row["CHARACTER_SET_SCHEMA"].ToString(),
                               CHARACTER_SET_NAME = row["CHARACTER_SET_NAME"].ToString(),
                               COLLATION_CATALOG = row["COLLATION_CATALOG"].ToString(),
                               COLLATION_SCHEMA = row["COLLATION_SCHEMA"].ToString(),
                               COLLATION_NAME = row["COLLATION_NAME"].ToString(),
                               DOMAIN_CATALOG = row["DOMAIN_CATALOG"].ToString(),
                               DOMAIN_NAME = row["DOMAIN_NAME"].ToString(),
                               DESCRIPTION = row["DESCRIPTION"].ToString(),
                               PRIMARY_KEY = row["PRIMARY_KEY"].ToBoolean(),
                               EDM_TYPE = row["EDM_TYPE"].ToString(),
                               AUTOINCREMENT = row["AUTOINCREMENT"].ToBoolean(),
                               UNIQUE = row["UNIQUE"].ToBoolean(),
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
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String INDEX_CATALOG;
        public String INDEX_SCHEMA;
        public String INDEX_NAME;
        public Boolean PRIMARY_KEY;
        public Boolean UNIQUE;
        public Boolean CLUSTERED;
        public Int32 TYPE;
        public Int32 FILL_FACTOR;
        public Int32 INITIAL_SIZE;
        public Int32 NULLS;
        public Boolean SORT_BOOKMARKS;
        public Boolean AUTO_UPDATE;
        public Int32 NULL_COLLATION;
        public Int32 ORDINAL_POSITION;
        public String COLUMN_NAME;
        public Guid COLUMN_GUID;
        public Int64 COLUMN_PROPID;
        public Int16 COLLATION;
        public Decimal CARDINALITY;
        public Int32 PAGES;
        public String FILTER_CONDITION;
        public Boolean INTEGRATED;
        public String INDEX_DEFINITION;

        public static IndexClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("Indexes")
                .Select()
                .Select
                (
                    row => new IndexClass
                           {
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               INDEX_CATALOG = row["INDEX_CATALOG"].ToString(),
                               INDEX_SCHEMA = row["INDEX_SCHEMA"].ToString(),
                               INDEX_NAME = row["INDEX_NAME"].ToString(),
                               PRIMARY_KEY = row["PRIMARY_KEY"].ToBoolean(),
                               UNIQUE = row["UNIQUE"].ToBoolean(),
                               CLUSTERED = row["CLUSTERED"].ToBoolean(),
                               TYPE = row["TYPE"].ToInt32(),
                               FILL_FACTOR = row["FILL_FACTOR"].ToInt32(),
                               INITIAL_SIZE = row["INITIAL_SIZE"].ToInt32(),
                               NULLS = row["NULLS"].ToInt32(),
                               SORT_BOOKMARKS = row["SORT_BOOKMARKS"].ToBoolean(),
                               AUTO_UPDATE = row["AUTO_UPDATE"].ToBoolean(),
                               NULL_COLLATION = row["NULL_COLLATION"].ToInt32(),
                               ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32(),
                               COLUMN_NAME = row["COLUMN_NAME"].ToString(),
                               COLUMN_GUID = row["COLUMN_GUID"].ToGuid(),
                               COLUMN_PROPID = row["COLUMN_PROPID"].ToInt64(),
                               COLLATION = row["COLLATION"].ToInt16(),
                               CARDINALITY = row["CARDINALITY"].ToDecimal(),
                               PAGES = row["PAGES"].ToInt32(),
                               FILTER_CONDITION = row["FILTER_CONDITION"].ToString(),
                               INTEGRATED = row["INTEGRATED"].ToBoolean(),
                               INDEX_DEFINITION = row["INDEX_DEFINITION"].ToString(),
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
        public String CONSTRAINT_CATALOG;
        public String CONSTRAINT_SCHEMA;
        public String CONSTRAINT_NAME;
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String COLUMN_NAME;
        public Int32 ORDINAL_POSITION;
        public String INDEX_NAME;
        public String COLLATION_NAME;
        public String SORT_MODE;
        public Int32 CONFLICT_OPTION;

        public static IndexColumnClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("IndexColumns")
                .Select()
                .Select
                (
                    row => new IndexColumnClass
                           {
                               CONSTRAINT_CATALOG = row["CONSTRAINT_CATALOG"].ToString(),
                               CONSTRAINT_SCHEMA = row["CONSTRAINT_SCHEMA"].ToString(),
                               CONSTRAINT_NAME = row["CONSTRAINT_NAME"].ToString(),
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               COLUMN_NAME = row["COLUMN_NAME"].ToString(),
                               ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32(),
                               INDEX_NAME = row["INDEX_NAME"].ToString(),
                               COLLATION_NAME = row["COLLATION_NAME"].ToString(),
                               SORT_MODE = row["SORT_MODE"].ToString(),
                               CONFLICT_OPTION = row["CONFLICT_OPTION"].ToInt32(),
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
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String TABLE_TYPE;
        public Int64 TABLE_ID;
        public Int32 TABLE_ROOTPAGE;
        public String TABLE_DEFINITION;

        public static TableClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("Tables")
                .Select()
                .Select
                (
                    row => new TableClass
                           {
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               TABLE_TYPE = row["TABLE_TYPE"].ToString(),
                               TABLE_ID = row["TABLE_ID"].ToInt64(),
                               TABLE_ROOTPAGE = row["TABLE_ROOTPAGE"].ToInt32(),
                               TABLE_DEFINITION = row["TABLE_DEFINITION"].ToString(),
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
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String VIEW_DEFINITION;
        public Boolean CHECK_OPTION;
        public Boolean IS_UPDATABLE;
        public String DESCRIPTION;
        public DateTime DATE_CREATED;
        public DateTime DATE_MODIFIED;

        public static ViewClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("Views")
                .Select()
                .Select
                (
                    row => new ViewClass
                           {
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               VIEW_DEFINITION = row["VIEW_DEFINITION"].ToString(),
                               CHECK_OPTION = row["CHECK_OPTION"].ToBoolean(),
                               IS_UPDATABLE = row["IS_UPDATABLE"].ToBoolean(),
                               DESCRIPTION = row["DESCRIPTION"].ToString(),
                               DATE_CREATED = row["DATE_CREATED"].ToDateTime(),
                               DATE_MODIFIED = row["DATE_MODIFIED"].ToDateTime(),
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
        public String VIEW_CATALOG;
        public String VIEW_SCHEMA;
        public String VIEW_NAME;
        public String VIEW_COLUMN_NAME;
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String COLUMN_NAME;
        public Int32 ORDINAL_POSITION;
        public Boolean COLUMN_HASDEFAULT;
        public String COLUMN_DEFAULT;
        public Int64 COLUMN_FLAGS;
        public Boolean IS_NULLABLE;
        public String DATA_TYPE;
        public Int32 CHARACTER_MAXIMUM_LENGTH;
        public Int32 NUMERIC_PRECISION;
        public Int32 NUMERIC_SCALE;
        public Int64 DATETIME_PRECISION;
        public String CHARACTER_SET_CATALOG;
        public String CHARACTER_SET_SCHEMA;
        public String CHARACTER_SET_NAME;
        public String COLLATION_CATALOG;
        public String COLLATION_SCHEMA;
        public String COLLATION_NAME;
        public Boolean PRIMARY_KEY;
        public String EDM_TYPE;
        public Boolean AUTOINCREMENT;
        public Boolean UNIQUE;

        public static ViewColumnClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("ViewColumns")
                .Select()
                .Select
                (
                    row => new ViewColumnClass
                           {
                               VIEW_CATALOG = row["VIEW_CATALOG"].ToString(),
                               VIEW_SCHEMA = row["VIEW_SCHEMA"].ToString(),
                               VIEW_NAME = row["VIEW_NAME"].ToString(),
                               VIEW_COLUMN_NAME = row["VIEW_COLUMN_NAME"].ToString(),
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               COLUMN_NAME = row["COLUMN_NAME"].ToString(),
                               ORDINAL_POSITION = row["ORDINAL_POSITION"].ToInt32(),
                               COLUMN_HASDEFAULT = row["COLUMN_HASDEFAULT"].ToBoolean(),
                               COLUMN_DEFAULT = row["COLUMN_DEFAULT"].ToString(),
                               COLUMN_FLAGS = row["COLUMN_FLAGS"].ToInt64(),
                               IS_NULLABLE = row["IS_NULLABLE"].ToBoolean(),
                               DATA_TYPE = row["DATA_TYPE"].ToString(),
                               CHARACTER_MAXIMUM_LENGTH = row["CHARACTER_MAXIMUM_LENGTH"].ToInt32(),
                               NUMERIC_PRECISION = row["NUMERIC_PRECISION"].ToInt32(),
                               NUMERIC_SCALE = row["NUMERIC_SCALE"].ToInt32(),
                               DATETIME_PRECISION = row["DATETIME_PRECISION"].ToInt64(),
                               CHARACTER_SET_CATALOG = row["CHARACTER_SET_CATALOG"].ToString(),
                               CHARACTER_SET_SCHEMA = row["CHARACTER_SET_SCHEMA"].ToString(),
                               CHARACTER_SET_NAME = row["CHARACTER_SET_NAME"].ToString(),
                               COLLATION_CATALOG = row["COLLATION_CATALOG"].ToString(),
                               COLLATION_SCHEMA = row["COLLATION_SCHEMA"].ToString(),
                               COLLATION_NAME = row["COLLATION_NAME"].ToString(),
                               PRIMARY_KEY = row["PRIMARY_KEY"].ToBoolean(),
                               EDM_TYPE = row["EDM_TYPE"].ToString(),
                               AUTOINCREMENT = row["AUTOINCREMENT"].ToBoolean(),
                               UNIQUE = row["UNIQUE"].ToBoolean(),
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
        public String CONSTRAINT_CATALOG;
        public String CONSTRAINT_SCHEMA;
        public String CONSTRAINT_NAME;
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String CONSTRAINT_TYPE;
        public Boolean IS_DEFERRABLE;
        public Boolean INITIALLY_DEFERRED;
        public String FKEY_FROM_COLUMN;
        public Int32 FKEY_FROM_ORDINAL_POSITION;
        public String FKEY_TO_CATALOG;
        public String FKEY_TO_SCHEMA;
        public String FKEY_TO_TABLE;
        public String FKEY_TO_COLUMN;

        public static ForeignKeyClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("ForeignKeys")
                .Select()
                .Select
                (
                    row => new ForeignKeyClass
                           {
                               CONSTRAINT_CATALOG = row["CONSTRAINT_CATALOG"].ToString(),
                               CONSTRAINT_SCHEMA = row["CONSTRAINT_SCHEMA"].ToString(),
                               CONSTRAINT_NAME = row["CONSTRAINT_NAME"].ToString(),
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               CONSTRAINT_TYPE = row["CONSTRAINT_TYPE"].ToString(),
                               IS_DEFERRABLE = row["IS_DEFERRABLE"].ToBoolean(),
                               INITIALLY_DEFERRED = row["INITIALLY_DEFERRED"].ToBoolean(),
                               FKEY_FROM_COLUMN = row["FKEY_FROM_COLUMN"].ToString(),
                               FKEY_FROM_ORDINAL_POSITION = row["FKEY_FROM_ORDINAL_POSITION"].ToInt32(),
                               FKEY_TO_CATALOG = row["FKEY_TO_CATALOG"].ToString(),
                               FKEY_TO_SCHEMA = row["FKEY_TO_SCHEMA"].ToString(),
                               FKEY_TO_TABLE = row["FKEY_TO_TABLE"].ToString(),
                               FKEY_TO_COLUMN = row["FKEY_TO_COLUMN"].ToString(),
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
        public String TABLE_CATALOG;
        public String TABLE_SCHEMA;
        public String TABLE_NAME;
        public String TRIGGER_NAME;
        public String TRIGGER_DEFINITION;

        public static TriggerClass[] Initialize(Context context)
        {
            return context
                .Connection
                .GetSchema("Triggers")
                .Select()
                .Select
                (
                    row => new TriggerClass
                           {
                               TABLE_CATALOG = row["TABLE_CATALOG"].ToString(),
                               TABLE_SCHEMA = row["TABLE_SCHEMA"].ToString(),
                               TABLE_NAME = row["TABLE_NAME"].ToString(),
                               TRIGGER_NAME = row["TRIGGER_NAME"].ToString(),
                               TRIGGER_DEFINITION = row["TRIGGER_DEFINITION"].ToString(),
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