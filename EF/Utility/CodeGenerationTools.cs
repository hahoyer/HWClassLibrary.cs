using System.CodeDom;
using System.CodeDom.Compiler;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.IO;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.CSharp;

namespace HWClassLibrary.EF.Utility
{
    /// <summary>
    ///     Responsible for helping to create source code that is
    ///     correctly formated and functional
    /// </summary>
    public class CodeGenerationTools
    {
        readonly DynamicTextTransformation _textTransformation;
        readonly CSharpCodeProvider _code;
        readonly MetadataTools _ef;

        /// <summary>
        ///     Initializes a new CodeGenerationTools object with the TextTransformation (T4 generated class)
        ///     that is currently running
        /// </summary>
        public CodeGenerationTools(object textTransformation)
        {
            if(textTransformation == null)
                throw new ArgumentNullException("textTransformation");

            _textTransformation = DynamicTextTransformation.Create(textTransformation);
            _code = new CSharpCodeProvider();
            _ef = new MetadataTools(_textTransformation);
            FullyQualifySystemTypes = false;
            CamelCaseFields = true;
        }

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

        /// <summary>
        ///     Returns the NamespaceName suggested by VS if running inside VS.  Otherwise, returns
        ///     null.
        /// </summary>
        public string VsNamespaceSuggestion()
        {
            var suggestion = _textTransformation.Host.ResolveParameterValue("directiveId", "namespaceDirectiveProcessor", "namespaceHint");
            if(String.IsNullOrEmpty(suggestion))
                return null;

            return suggestion;
        }

        /// <summary>
        ///     Returns a string that is safe for use as an identifier in C#.
        ///     Keywords are escaped.
        /// </summary>
        public string Escape(string name)
        {
            if(name == null)
                return null;

            return _code.CreateEscapedIdentifier(name);
        }

        /// <summary>
        ///     Returns the name of the TypeUsage's EdmType that is safe for
        ///     use as an identifier.
        /// </summary>
        public string Escape(TypeUsage typeUsage)
        {
            if(typeUsage == null)
                return null;

            if(typeUsage.EdmType is ComplexType ||
               typeUsage.EdmType is EntityType)
                return Escape(typeUsage.EdmType.Name);
            else if(typeUsage.EdmType is PrimitiveType)
            {
                var clrType = _ef.ClrType(typeUsage);
                var typeName = Escape(clrType);
                if(clrType.IsValueType && _ef.IsNullable(typeUsage))
                    return String.Format(CultureInfo.InvariantCulture, "Nullable<{0}>", typeName);

                return typeName;
            }
            else if(typeUsage.EdmType is CollectionType)
                return String.Format(CultureInfo.InvariantCulture, "ICollection<{0}>", Escape(((CollectionType) typeUsage.EdmType).TypeUsage));


            throw new ArgumentException("typeUsage");
        }

        /// <summary>
        ///     Returns the name of the EdmMember that is safe for
        ///     use as an identifier.
        /// </summary>
        public string Escape(EdmMember member)
        {
            if(member == null)
                return null;

            return Escape(member.Name);
        }

        /// <summary>
        ///     Returns the name of the EdmType that is safe for
        ///     use as an identifier.
        /// </summary>
        public string Escape(EdmType type)
        {
            if(type == null)
                return null;

            return Escape(type.Name);
        }

        /// <summary>
        ///     Returns the name of the EdmFunction that is safe for
        ///     use as an identifier.
        /// </summary>
        public string Escape(EdmFunction function)
        {
            if(function == null)
                return null;

            return Escape(function.Name);
        }

        /// <summary>
        ///     Returns the name of the EntityContainer that is safe for
        ///     use as an identifier.
        /// </summary>
        public string Escape(EntityContainer container)
        {
            if(container == null)
                return null;

            return Escape(container.Name);
        }

        /// <summary>
        ///     Returns the name of the EntitySet that is safe for
        ///     use as an identifier.
        /// </summary>
        public string Escape(EntitySet set)
        {
            if(set == null)
                return null;

            return Escape(set.Name);
        }

        /// <summary>
        ///     Returns the name of the StructuralType that is safe for
        ///     use as an identifier.
        /// </summary>
        public string Escape(StructuralType type)
        {
            if(type == null)
                return null;

            return Escape(type.Name);
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

        ///<summary>
        ///    Returns the name of the EdmMember formatted for
        ///    use as a field identifier.
        ///
        ///    This method changes behavior based on the CamelCaseFields
        ///    setting.
        ///</summary>
        public string FieldName(EdmMember member)
        {
            if(member == null)
                return null;

            return FieldName(member.Name);
        }

        ///<summary>
        ///    Returns the name of the EntitySet formatted for
        ///    use as a field identifier.
        ///
        ///    This method changes behavior based on the CamelCaseFields
        ///    setting.
        ///</summary>
        public string FieldName(EntitySet set)
        {
            if(set == null)
                return null;

            return FieldName(set.Name);
        }

        string FieldName(string name)
        {
            if(CamelCaseFields)
                return "_" + CamelCase(name);
            else
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
                typeName = _code.GetTypeOutput(new CodeTypeReference(clrType));
            return typeName;
        }


        /// <summary>
        ///     Returns the abstract option if the entity is Abstract, otherwise returns String.Empty
        /// </summary>
        public string AbstractOption(EntityType entity)
        {
            if(entity.Abstract)
                return "abstract";
            return String.Empty;
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
                return string.Empty;

            var type = value.GetType();
            if(type.IsEnum)
                return type.FullName + "." + value;
            if(type == typeof(Guid))
                return string.Format(CultureInfo.InvariantCulture, "new Guid(\"{0}\")",
                                     ((Guid) value).ToString("D", CultureInfo.InvariantCulture));
            else if(type == typeof(DateTime))
                return string.Format(CultureInfo.InvariantCulture, "new DateTime({0}, DateTimeKind.Unspecified)",
                                     ((DateTime) value).Ticks);
            else if(type == typeof(byte[]))
            {
                var arrayInit = string.Join(", ", ((byte[]) value).Select(b => b.ToString(CultureInfo.InvariantCulture)).ToArray());
                return string.Format(CultureInfo.InvariantCulture, "new Byte[] {{{0}}}", arrayInit);
            }
            else if(type == typeof(DateTimeOffset))
            {
                var dto = (DateTimeOffset) value;
                return string.Format(CultureInfo.InvariantCulture, "new DateTimeOffset({0}, new TimeSpan({1}))",
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