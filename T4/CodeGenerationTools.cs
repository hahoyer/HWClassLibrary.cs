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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Microsoft.CSharp;

namespace HWClassLibrary.T4
{
    /// <summary>
    ///     Responsible for helping to create source code that is
    ///     correctly formated and functional
    /// </summary>
    public sealed class CodeGenerationTools
    {
        readonly DynamicTextTransformation _textTransformation;
        readonly SimpleCache<CSharpCodeProvider> _codeProviderCache = new SimpleCache<CSharpCodeProvider>(()=> new CSharpCodeProvider());

        /// <summary>
        ///     Initializes a new CodeGenerationTools object with the TextTransformation (T4 generated class)
        ///     that is currently running
        /// </summary>
        internal CodeGenerationTools([NotNull] object textTransformation)
        {
            _textTransformation = DynamicTextTransformation.Create(textTransformation);
            FullyQualifySystemTypes = false;
            CamelCaseFields = true;
        }

        [UsedImplicitly]
        public string[] ProcessFiles() { return FileManager.Process(true); }

        /// <summary>
        ///     Marks the end of the last file if there was one, and starts a new
        ///     and marks this point in generation as a new file.
        /// </summary>
        [UsedImplicitly]
        public void StartNewFile(string name) { FileManager.StartNewFile(name); }

        ///<summary>
        ///    When true, all types that are not being generated
        ///    are fully qualified to keep them from conflicting with
        ///    types that are being generated. Useful when you have
        ///    something like a type being generated named System.
        ///
        ///    Default is false.
        ///</summary>
        public bool FullyQualifySystemTypes { get; set; }

        ///<summary>
        ///    When true, the field names are Camel Cased,
        ///    otherwise they will preserve the case they
        ///    start with.
        ///
        ///    Default is true.
        ///</summary>
        public bool CamelCaseFields { get; set; }
        
        TemplateFileManager FileManager { get { return _textTransformation.FileManager; } }

        /// <summary>
        ///     Returns the NamespaceName suggested by VS if running inside VS.  Otherwise, returns
        ///     null.
        /// </summary>
        [UsedImplicitly]
        public string NameSpace()
        {
            var result = _textTransformation.Host.ResolveParameterValue("directiveId", "namespaceDirectiveProcessor", "namespaceHint");
            if(String.IsNullOrEmpty(result))
                return null;

            return result;
        }

        /// <summary>
        ///     Returns a string that is safe for use as an identifier in C#.
        ///     Keywords are escaped.
        /// </summary>
        public string Escape(string name)
        {
            if(name == null)
                return null;

            return _codeProviderCache.Value.CreateEscapedIdentifier(name);
        }


        /// <summary>
        ///     Returns the NamespaceName with each segment safe to
        ///     use as an identifier.
        /// </summary>
        public string EscapeNamespace(string namespaceName)
        {
            if(String.IsNullOrEmpty(namespaceName))
                return namespaceName;

            var parts = namespaceName.Split('.');
            namespaceName = String.Empty;
            foreach(var part in parts)
            {
                if(namespaceName != String.Empty)
                    namespaceName += ".";

                namespaceName += Escape(part);
            }

            return namespaceName;
        }

        string FieldName(string name)
        {
            if(CamelCaseFields)
                return "_" + CamelCase(name);
            return "_" + name;
        }

        ///<summary>
        ///    Returns the name of the Type object formatted for
        ///    use in source code.
        ///
        ///    This method changes behavior based on the FullyQualifySystemTypes
        ///    setting.
        ///</summary>
        public string Escape(Type clrType)
        {
            if(clrType == null)
                return null;

            string typeName;
            if(FullyQualifySystemTypes)
                typeName = "global::" + clrType.FullName;
            else
                typeName = _codeProviderCache.Value.GetTypeOutput(new CodeTypeReference(clrType));
            return typeName;
        }


        /// <summary>
        ///     Returns the passed in identifier with the first letter changed to lowercase
        /// </summary>
        public string CamelCase(string identifier)
        {
            if(String.IsNullOrEmpty(identifier))
                return identifier;

            if(identifier.Length == 1)
                return identifier[0].ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

            return identifier[0].ToString(CultureInfo.InvariantCulture).ToLowerInvariant() + identifier.Substring(1);
        }

        /// <summary>
        ///     If the value parameter is null or empty an empty string is returned,
        ///     otherwise it retuns value with a single space concatenated on the end.
        /// </summary>
        public string SpaceAfter(string value) { return StringAfter(value, " "); }

        /// <summary>
        ///     If the value parameter is null or empty an empty string is returned,
        ///     otherwise it retuns value with a single space concatenated on the end.
        /// </summary>
        public string SpaceBefore(string value) { return StringBefore(" ", value); }

        /// <summary>
        ///     If the value parameter is null or empty an empty string is returned,
        ///     otherwise it retuns value with append concatenated on the end.
        /// </summary>
        public string StringAfter(string value, string append)
        {
            if(String.IsNullOrEmpty(value))
                return String.Empty;

            return value + append;
        }

        /// <summary>
        ///     If the value parameter is null or empty an empty string is returned,
        ///     otherwise it retuns value with prepend concatenated on the front.
        /// </summary>
        public string StringBefore(string prepend, string value)
        {
            if(String.IsNullOrEmpty(value))
                return String.Empty;

            return prepend + value;
        }

        /// <summary>
        ///     Retuns as full of a name as possible, if a namespace is provided
        ///     the namespace and name are combined with a period, otherwise just
        ///     the name is returned.
        /// </summary>
        public string CreateFullName(string namespaceName, string name)
        {
            if(String.IsNullOrEmpty(namespaceName))
                return name;

            return namespaceName + "." + name;
        }

        public string CreateLiteral(object value)
        {
            if(value == null)
                return String.Empty;

            var type = value.GetType();
            if(type.IsEnum)
                return type.FullName + "." + value;
            if(type == typeof(Guid))
                return String.Format(CultureInfo.InvariantCulture, "new Guid(\"{0}\")",
                                     ((Guid) value).ToString("D", CultureInfo.InvariantCulture));
            if(type == typeof(DateTime))
                return String.Format(CultureInfo.InvariantCulture, "new DateTime({0}, DateTimeKind.Unspecified)",
                                     ((DateTime) value).Ticks);
            if(type == typeof(byte[]))
            {
                var arrayInit = String.Join(", ", ((byte[]) value).Select(b => b.ToString(CultureInfo.InvariantCulture)).ToArray());
                return String.Format(CultureInfo.InvariantCulture, "new Byte[] {{{0}}}", arrayInit);
            }
            if(type == typeof(DateTimeOffset))
            {
                var dto = (DateTimeOffset) value;
                return String.Format(CultureInfo.InvariantCulture, "new DateTimeOffset({0}, new TimeSpan({1}))",
                                     dto.Ticks, dto.Offset.Ticks);
            }

            var expression = new CodePrimitiveExpression(value);
            var writer = new StringWriter();
            var code = new CSharpCodeProvider();
            code.GenerateCodeFromExpression(expression, writer, new CodeGeneratorOptions());
            return writer.ToString();
        }
    }
}