//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

namespace HWClassLibrary.EF.Utility
{
// Copyright (c) Microsoft Corporation.  All rights reserved.


    /// <summary>
    ///     Responsible for encapsulating the constants defined in Metadata
    /// </summary>
    public static class MetadataConstants
    {
        public const string EDMX_NAMESPACE_V1 = "http://schemas.microsoft.com/ado/2007/06/edmx";
        public const string EDMX_NAMESPACE_V2 = "http://schemas.microsoft.com/ado/2008/10/edmx";


        public const string CSDL_EXTENSION = ".csdl";
        public const string CSDL_NAMESPACE_V1 = "http://schemas.microsoft.com/ado/2006/04/edm";
        public const string CSDL_NAMESPACE_V2 = "http://schemas.microsoft.com/ado/2008/09/edm";
        public const string CSDL_EDMX_SECTION_NAME = "ConceptualModels";
        public const string CSDL_ROOT_ELEMENT_NAME = "Schema";
        public const string EDM_ANNOTATION_09_02 = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";

        public const string SSDL_EXTENSION = ".ssdl";
        public const string SSDL_NAMESPACE_V1 = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl";
        public const string SSDL_NAMESPACE_V2 = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl";
        public const string SSDL_EDMX_SECTION_NAME = "StorageModels";
        public const string SSDL_ROOT_ELEMENT_NAME = "Schema";

        public const string MSL_EXTENSION = ".msl";
        public const string MSL_NAMESPACE_V1 = "urn:schemas-microsoft-com:windows:storage:mapping:CS";
        public const string MSL_NAMESPACE_V2 = "http://schemas.microsoft.com/ado/2008/09/mapping/cs";
        public const string MSL_EDMX_SECTION_NAME = "Mappings";
        public const string MSL_ROOT_ELEMENT_NAME = "Mapping";
    }
}