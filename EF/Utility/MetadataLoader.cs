using System.CodeDom.Compiler;
using System.Data.Entity.Design;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.EF.Utility
{
    /// <summary>
    ///     Responsible for loading an EdmItemCollection from a .edmx file or .csdl files
    /// </summary>
    public class MetadataLoader
    {
        readonly DynamicTextTransformation _textTransformation;

        /// <summary>
        ///     Initializes an MetadataLoader Instance  with the
        ///     TextTransformation (T4 generated class) that is currently running
        /// </summary>
        public MetadataLoader(object textTransformation)
        {
            if(textTransformation == null)
                throw new ArgumentNullException("textTransformation");

            _textTransformation = DynamicTextTransformation.Create(textTransformation);
        }

        /// <summary>
        ///     Load the metadata for Edm, Store, and Mapping collections and register them
        ///     with a new MetadataWorkspace, returns false if any of the parts can't be
        ///     created, some of the ItemCollections may be registered and usable even if false is
        ///     returned
        /// </summary>
        public bool TryLoadAllMetadata(string inputFile, out MetadataWorkspace metadataWorkspace)
        {
            metadataWorkspace = new MetadataWorkspace();

            var edmItemCollection = CreateEdmItemCollection(inputFile);
            metadataWorkspace.RegisterItemCollection(edmItemCollection);


            StoreItemCollection storeItemCollection = null;
            if(TryCreateStoreItemCollection(inputFile, out storeItemCollection))
            {
                StorageMappingItemCollection storageMappingItemCollection = null;
                if(TryCreateStorageMappingItemCollection(inputFile, edmItemCollection, storeItemCollection, out storageMappingItemCollection))
                {
                    metadataWorkspace.RegisterItemCollection(storeItemCollection);
                    metadataWorkspace.RegisterItemCollection(storageMappingItemCollection);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Create an EdmItemCollection loaded with the metadata provided
        /// </summary>
        public EdmItemCollection CreateEdmItemCollection(string sourcePath, params string[] referenceSchemas)
        {
            EdmItemCollection edmItemCollection;
            if(TryCreateEdmItemCollection(sourcePath, referenceSchemas, out edmItemCollection))
                return edmItemCollection;

            return new EdmItemCollection();
        }

        /// <summary>
        ///     Attempts to create a EdmItemCollection from the specified metadata file
        /// </summary>
        public bool TryCreateEdmItemCollection(string sourcePath, out EdmItemCollection edmItemCollection) { return TryCreateEdmItemCollection(sourcePath, null, out edmItemCollection); }

        /// <summary>
        ///     Attempts to create a EdmItemCollection from the specified metadata file
        /// </summary>
        public bool TryCreateEdmItemCollection(string sourcePath, string[] referenceSchemas, out EdmItemCollection edmItemCollection)
        {
            edmItemCollection = null;

            if(String.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("sourcePath");

            if(referenceSchemas == null)
                referenceSchemas = new string[0];

            ItemCollection itemCollection = null;
            sourcePath = _textTransformation.Host.ResolvePath(sourcePath);
            var collectionBuilder = new EdmItemCollectionBuilder(_textTransformation, referenceSchemas.Select(s => _textTransformation.Host.ResolvePath(s)).Where(s => s != sourcePath));
            if(collectionBuilder.TryCreateItemCollection(sourcePath, out itemCollection))
                edmItemCollection = (EdmItemCollection) itemCollection;

            return edmItemCollection != null;
        }

        /// <summary>
        ///     Attempts to create a StoreItemCollection from the specified metadata file
        /// </summary>
        public bool TryCreateStoreItemCollection(string sourcePath, out StoreItemCollection storeItemCollection)
        {
            storeItemCollection = null;

            if(String.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("sourcePath");

            ItemCollection itemCollection = null;
            var collectionBuilder = new StoreItemCollectionBuilder(_textTransformation);
            if(collectionBuilder.TryCreateItemCollection(_textTransformation.Host.ResolvePath(sourcePath), out itemCollection))
                storeItemCollection = (StoreItemCollection) itemCollection;
            return storeItemCollection != null;
        }

        /// <summary>
        ///     Attempts to create a StorageMappingItemCollection from the specified metadata file, EdmItemCollection, and StoreItemCollection
        /// </summary>
        public bool TryCreateStorageMappingItemCollection(string sourcePath, EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection, out StorageMappingItemCollection storageMappingItemCollection)
        {
            storageMappingItemCollection = null;

            if(String.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("sourcePath");

            if(edmItemCollection == null)
                throw new ArgumentNullException("edmItemCollection");

            if(storeItemCollection == null)
                throw new ArgumentNullException("storeItemCollection");

            ItemCollection itemCollection = null;
            var collectionBuilder = new StorageMappingItemCollectionBuilder(_textTransformation, edmItemCollection, storeItemCollection);
            if(collectionBuilder.TryCreateItemCollection(_textTransformation.Host.ResolvePath(sourcePath), out itemCollection))
                storageMappingItemCollection = (StorageMappingItemCollection) itemCollection;
            return storageMappingItemCollection != null;
        }

        /// <summary>
        ///     Gets the Model Namespace from the provided schema file.
        /// </summary>
        public string GetModelNamespace(string sourcePath)
        {
            if(String.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("sourcePath");

            if(sourcePath == "$edmxInputFile$")
            {
                _textTransformation.Errors.Add(new CompilerError(_textTransformation.Host.TemplateFile ?? "Currently Running Template", 0, 0, "",
                                                                 "Please overwrite the replacement token '$edmxInputFile$' with the actual name of the .edmx file you would like to generate from."));
                return String.Empty;
            }

            var builder = new EdmItemCollectionBuilder(_textTransformation);
            XElement model;
            if(builder.TryLoadRootElement(_textTransformation.Host.ResolvePath(sourcePath), out model))
            {
                var attribute = model.Attribute("Namespace");
                if(attribute != null)
                    return attribute.Value;
            }

            return String.Empty;
        }

        /// <summary>
        ///     base class for ItemCollectionBuilder classes that
        ///     load the specific types of metadata
        /// </summary>
        abstract class ItemCollectionBuilder
        {
            readonly DynamicTextTransformation _textTransformation;
            readonly string _fileExtension;
            readonly string _namespaceV1;
            readonly string _namespaceV2;
            readonly string _edmxSectionName;
            readonly string _rootElementName;

            /// <summary>
            ///     FileExtension for individual (non-edmx) metadata file for this
            ///     specific ItemCollection type
            /// </summary>
            public string FileExtension { get { return _fileExtension; } }

            /// <summary>
            ///     EF Version 1 XmlNamespace name
            /// </summary>
            public string NamespaceV1 { get { return _namespaceV1; } }

            /// <summary>
            ///     EF Version 2 XmlNamespace name
            /// </summary>
            public string NamespaceV2 { get { return _namespaceV2; } }

            /// <summary>
            ///     The name of the XmlElement in the .edmx <Runtime> element
            ///                                                 to find this ItemCollection's metadata
            /// </summary>
            public string EdmxSectionName { get { return _edmxSectionName; } }

            /// <summary>
            ///     The name of the root element of this ItemCollection's metadata
            /// </summary>
            public string RootElementName { get { return _rootElementName; } }

            /// <summary>
            ///     Method to build the appropriate ItemCollection
            /// </summary>
            protected abstract ItemCollection CreateItemCollection(IEnumerable<XmlReader> readers, out IList<EdmSchemaError> errors);

            /// <summary>
            ///     Ctor to setup the ItemCollectionBuilder members
            /// </summary>
            protected ItemCollectionBuilder(DynamicTextTransformation textTransformation, string fileExtension, string namespaceV1, string namespaceV2, string edmxSectionName, string rootElementName)
            {
                _textTransformation = textTransformation;
                _fileExtension = fileExtension;
                _namespaceV1 = namespaceV1;
                _namespaceV2 = namespaceV2;
                _edmxSectionName = edmxSectionName;
                _rootElementName = rootElementName;
            }

            /// <summary>
            ///     Try to create an ItemCollection loaded with the metadata provided
            /// </summary>
            public bool TryCreateItemCollection(string sourcePath, out ItemCollection itemCollection)
            {
                itemCollection = null;

                if(String.IsNullOrEmpty(sourcePath))
                    throw new ArgumentException("sourcePath");

                if(sourcePath == "$edmxInputFile$")
                {
                    _textTransformation.Errors.Add(new CompilerError(_textTransformation.Host.TemplateFile ?? "Currently Running Template", 0, 0, "",
                                                                     "Please overwrite the replacement token '$edmxInputFile$' with the actual name of the .edmx file you would like to generate from."));
                    return false;
                }

                XElement schemaElement = null;
                if(TryLoadRootElement(sourcePath, out schemaElement))
                {
                    var readers = new List<XmlReader>();
                    try
                    {
                        readers.Add(schemaElement.CreateReader());
                        IList<EdmSchemaError> errors = null;

                        var tempItemCollection = CreateItemCollection(readers, out errors);
                        if(ProcessErrors(errors, sourcePath))
                            return false;

                        itemCollection = tempItemCollection;
                        return true;
                    }
                    finally
                    {
                        foreach(var reader in readers)
                            ((IDisposable) reader).Dispose();
                    }
                }

                return false;
            }

            /// <summary>
            ///     Tries to load the root element from the metadata file provided
            /// </summary>
            public bool TryLoadRootElement(string sourcePath, out XElement schemaElement)
            {
                schemaElement = null;
                var extension = Path.GetExtension(sourcePath);
                if(extension.Equals(".edmx", StringComparison.InvariantCultureIgnoreCase))
                    return TryLoadRootElementFromEdmx(sourcePath, out schemaElement);
                else if(extension.Equals(FileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    // load from single metadata file (.csdl, .ssdl, or .msl)
                    schemaElement = XElement.Load(sourcePath, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Trys to load the root element from the edmxDocument provided
            /// </summary>
            static bool TryLoadRootElementFromEdmx(XElement edmxDocument, string edmxNamespace, string sectionNamespace, string sectionName, string rootElementName, out XElement rootElement)
            {
                rootElement = null;

                XNamespace edmxNs = edmxNamespace;
                XNamespace sectionNs = sectionNamespace;

                var runtime = edmxDocument.Element(edmxNs + "Runtime");
                if(runtime == null)
                    return false;

                var section = runtime.Element(edmxNs + sectionName);
                if(section == null)
                    return false;

                rootElement = section.Element(sectionNs + rootElementName);
                return rootElement != null;
            }

            /// <summary>
            ///     Trys to load the root element from the .edmx metadata file provided
            /// </summary>
            bool TryLoadRootElementFromEdmx(string edmxPath, out XElement rootElement)
            {
                rootElement = null;

                var element = XElement.Load(edmxPath, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);

                return TryLoadRootElementFromEdmx(element, MetadataConstants.EDMX_NAMESPACE_V2, NamespaceV2, EdmxSectionName, RootElementName, out rootElement)
                       || TryLoadRootElementFromEdmx(element, MetadataConstants.EDMX_NAMESPACE_V1, NamespaceV1, EdmxSectionName, RootElementName, out rootElement);
            }

            /// <summary>
            ///     Takes an Enumerable of EdmSchemaErrors, and adds them
            ///     to the errors collection of the template class
            /// </summary>
            bool ProcessErrors(IEnumerable<EdmSchemaError> errors, string sourceFilePath)
            {
                var foundErrors = false;
                foreach(var error in errors)
                {
                    var newError = new CompilerError(error.SchemaLocation, error.Line, error.Column,
                                                     error.ErrorCode.ToString(CultureInfo.InvariantCulture),
                                                     error.Message);
                    newError.IsWarning = error.Severity == EdmSchemaErrorSeverity.Warning;
                    foundErrors |= error.Severity == EdmSchemaErrorSeverity.Error;
                    if(error.SchemaLocation == null)
                        newError.FileName = sourceFilePath;
                    _textTransformation.Errors.Add(newError);
                }

                return foundErrors;
            }
        }

        /// <summary>
        ///     Builder class for creating a StorageMappingItemCollection
        /// </summary>
        class StorageMappingItemCollectionBuilder : ItemCollectionBuilder
        {
            readonly EdmItemCollection _edmItemCollection;
            readonly StoreItemCollection _storeItemCollection;

            public StorageMappingItemCollectionBuilder(DynamicTextTransformation textTransformation, EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection)
                : base(textTransformation, MetadataConstants.MSL_EXTENSION, MetadataConstants.MSL_NAMESPACE_V1, MetadataConstants.MSL_NAMESPACE_V2, MetadataConstants.MSL_EDMX_SECTION_NAME, MetadataConstants.MSL_ROOT_ELEMENT_NAME)
            {
                _edmItemCollection = edmItemCollection;
                _storeItemCollection = storeItemCollection;
            }

            protected override ItemCollection CreateItemCollection(IEnumerable<XmlReader> readers, out IList<EdmSchemaError> errors) { return MetadataItemCollectionFactory.CreateStorageMappingItemCollection(_edmItemCollection, _storeItemCollection, readers, out errors); }
        }

        /// <summary>
        ///     Builder class for creating a StoreItemCollection
        /// </summary>
        class StoreItemCollectionBuilder : ItemCollectionBuilder
        {
            public StoreItemCollectionBuilder(DynamicTextTransformation textTransformation)
                : base(textTransformation, MetadataConstants.SSDL_EXTENSION, MetadataConstants.SSDL_NAMESPACE_V1, MetadataConstants.SSDL_NAMESPACE_V2, MetadataConstants.SSDL_EDMX_SECTION_NAME, MetadataConstants.SSDL_ROOT_ELEMENT_NAME) { }

            protected override ItemCollection CreateItemCollection(IEnumerable<XmlReader> readers, out IList<EdmSchemaError> errors) { return MetadataItemCollectionFactory.CreateStoreItemCollection(readers, out errors); }
        }

        /// <summary>
        ///     Builder class for creating a EdmItemCollection
        /// </summary>
        class EdmItemCollectionBuilder : ItemCollectionBuilder
        {
            readonly List<string> _referenceSchemas = new List<string>();

            public EdmItemCollectionBuilder(DynamicTextTransformation textTransformation)
                : base(textTransformation, MetadataConstants.CSDL_EXTENSION, MetadataConstants.CSDL_NAMESPACE_V1, MetadataConstants.CSDL_NAMESPACE_V2, MetadataConstants.CSDL_EDMX_SECTION_NAME, MetadataConstants.CSDL_ROOT_ELEMENT_NAME) { }

            public EdmItemCollectionBuilder(DynamicTextTransformation textTransformation, IEnumerable<string> referenceSchemas)
                : this(textTransformation) { _referenceSchemas.AddRange(referenceSchemas); }

            protected override ItemCollection CreateItemCollection(IEnumerable<XmlReader> readers, out IList<EdmSchemaError> errors)
            {
                var ownedReaders = new List<XmlReader>();
                var allReaders = new List<XmlReader>();
                try
                {
                    allReaders.AddRange(readers);
                    foreach(var path in _referenceSchemas.Distinct())
                    {
                        XElement reference;
                        if(TryLoadRootElement(path, out reference))
                        {
                            var reader = reference.CreateReader();
                            allReaders.Add(reader);
                            ownedReaders.Add(reader);
                        }
                    }

                    return MetadataItemCollectionFactory.CreateEdmItemCollection(allReaders, out errors);
                }
                finally
                {
                    foreach(var reader in ownedReaders)
                        ((IDisposable) reader).Dispose();
                }
            }
        }
    }
}