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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.sqlass.T4
{
    using System;


#line 1 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
    [GeneratedCode("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class SQLTable : SQLTableBase
    {
        public virtual string TransformText()
        {
            Write("using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing HWCla" +
                  "ssLibrary.Debug;\r\nusing HWClassLibrary.sqlass;\r\nusing HWClassLibrary.sqlass.Meta" +
                  "Data;\r\n\r\nnamespace ");

#line 9 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(_context.NameSpace));

#line default
#line hidden
            Write(".Tables");

#line 9 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(NameSpaceSuffix));

#line default
#line hidden
            Write("\r\n{\r\n    public partial class ");

#line 11 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(ClassName));

#line default
#line hidden
            Write(" \r\n        : ISQLSupportProvider");

#line 12 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

            if(HasSQLKey)
            {
#line default
#line hidden
                Write("        \r\n        , ");

#line 16 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(KeyProvider));

#line default
#line hidden

#line 16 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            }

#line default
#line hidden
            Write("    \r\n    {");

#line 19 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

            foreach(var column in Columns)
            {
#line default
#line hidden
                Write(" \r\n        public ");

#line 22 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.FieldTypeName));

#line default
#line hidden
                Write(" ");

#line 22 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.Name));

#line default
#line hidden
                Write(";");

#line 22 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            }

#line default
#line hidden
            Write("\r\n\r\n        ISQLSupport ISQLSupportProvider.SQLSupport{get{return new SQLSupport." +
                  "");

#line 26 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(ClassName));

#line default
#line hidden
            Write("(this);}} ");

#line 26 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

            if(HasSQLKey)
            {
#line default
#line hidden
                Write("\r\n        ");

#line 30 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(KeyType));

#line default
#line hidden
                Write(" ");

#line 30 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(KeyProvider));

#line default
#line hidden
                Write(".SQLKey { get { return ");

#line 30 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(KeyValue));

#line default
#line hidden
                Write("; } }");

#line 30 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            }

#line default
#line hidden
            Write("    \r\n    }\r\n}\r\n\r\nnamespace ");

#line 36 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(_context.NameSpace));

#line default
#line hidden
            Write(".SQLSupport");

#line 36 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(NameSpaceSuffix));

#line default
#line hidden
            Write("\r\n{\r\n    public partial class ");

#line 38 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(ClassName));

#line default
#line hidden
            Write(": ISQLSupport\r\n    {\r\n        readonly Tables.");

#line 40 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(ClassName));

#line default
#line hidden
            Write(" _target;\r\n        public ");

#line 41 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(ClassName));

#line default
#line hidden
            Write("(Tables.");

#line 41 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(ClassName));

#line default
#line hidden
            Write(" target){_target = target;}\r\n\r\n        string ISQLSupport.Insert\r\n        {\r\n    " +
                  "        get\r\n            {\r\n                var result = \"insert into ");

#line 47 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(SQLTableName));

#line default
#line hidden
            Write(" values(\";");

#line 47 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

            foreach(var column in Columns)
            {
#line default
#line hidden
                Write(" \r\n                result += _target.");

#line 50 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.Name));

#line default
#line hidden
                Write(".SQLFormat();");

#line 50 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

                if(!IsLast(column))
#line default
#line hidden
                    Write("\r\n                result += \", \";");
            
#line 54 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                else
#line default
#line hidden
                    Write("\r\n                result += \")\";");
            
#line 59 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            }

#line default
#line hidden
            Write("   \r\n\r\n                return result;\r\n            }\r\n        }\r\n\r\n        public" +
                  " static readonly Table MetaDataSupport = new Table\r\n        (\r\n            Table" +
                  "Name.Find(");

#line 69 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(Table.TableName.Catalog.Quote()));

#line default
#line hidden
            Write(", ");

#line 69 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(Table.TableName.Schema.Quote()));

#line default
#line hidden
            Write(", ");

#line 69 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(Table.TableName.Name.Quote()));

#line default
#line hidden
            Write("),\r\n            () => new []\r\n            {");

#line 71 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

            foreach(var column in Columns)
            {
#line default
#line hidden
                Write(" \r\n\r\n                new Column\r\n                {\r\n                    Name = ");

#line 77 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.Name.Quote()));

#line default
#line hidden
                Write(",\r\n                    Type = typeof(");

#line 78 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.Type.PrettyName()));

#line default
#line hidden
                Write("),\r\n                    IsKey = ");

#line 79 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.IsKey ? "true" : "false"));

#line default
#line hidden
                Write(",\r\n                    IsNullable = ");

#line 80 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.IsNullable ? "true" : "false"));

#line default
#line hidden
                Write(",");

#line 80 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

                if(column.ReferencedTable != null)
                {
#line default
#line hidden
                    Write("                    \r\n                    ReferencedTable = ");

#line 84 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                    Write(ToStringHelper.ToStringWithCulture(column.ReferencedTable.Name));

#line default
#line hidden
                    Write(".MetaDataSupport");

#line 84 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                }

#line default
#line hidden
                Write("\r\n                },");

#line 87 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            }

#line default
#line hidden
            Write("   \r\n            \r\n            },\r\n            (record, context) => new Tables.");

#line 91 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            Write(ToStringHelper.ToStringWithCulture(ClassName));

#line default
#line hidden
            Write("\r\n            {");

#line 92 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

            foreach(var column in Columns)
            {
#line default
#line hidden
                Write(" \r\n                ");

#line 95 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.Name));

#line default
#line hidden
                Write(" = ");

#line 95 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

                if(column.ReferencedTable != null)
                {
#line default
#line hidden
                    Write("((Context)context).Container.");

#line 99 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                    Write(ToStringHelper.ToStringWithCulture(column.ReferencedTable.Name));

#line default
#line hidden
                    Write(".Find");

#line 99 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                }


#line default
#line hidden
                Write("((");

#line 101 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.Type.PrettyName()));

#line default
#line hidden
                Write(") record[");

#line 101 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
                Write(ToStringHelper.ToStringWithCulture(column.Name.Quote()));

#line default
#line hidden
                Write("]),");

#line 101 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"
            }

#line default
#line hidden
            Write("   \r\n            }\r\n        );\r\n    }\r\n}\r\n");

#line 107 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLTable.tt"

// ReSharper disable FieldCanBeMadeReadOnly.Local


#line default
#line hidden
            Write("\r\n");
            return GenerationEnvironment.ToString();
        }
    }

#line default
#line hidden

    #region Base class

    /// <summary>
    ///     Base class for this transformation
    /// </summary>
    [GeneratedCode("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public class SQLTableBase
    {
        #region Fields

        StringBuilder generationEnvironmentField;
        CompilerErrorCollection errorsField;
        List<int> indentLengthsField;
        string currentIndentField = "";
        bool endsWithNewline;

        #endregion

        #region Properties

        /// <summary>
        ///     The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected StringBuilder GenerationEnvironment
        {
            get
            {
                if((generationEnvironmentField == null))
                    generationEnvironmentField = new StringBuilder();
                return generationEnvironmentField;
            }
            set { generationEnvironmentField = value; }
        }
        /// <summary>
        ///     The error collection for the generation process
        /// </summary>
        public CompilerErrorCollection Errors
        {
            get
            {
                if((errorsField == null))
                    errorsField = new CompilerErrorCollection();
                return errorsField;
            }
        }
        /// <summary>
        ///     A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        List<int> indentLengths
        {
            get
            {
                if((indentLengthsField == null))
                    indentLengthsField = new List<int>();
                return indentLengthsField;
            }
        }
        /// <summary>
        ///     Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent { get { return currentIndentField; } }
        /// <summary>
        ///     Current transformation session
        /// </summary>
        public virtual IDictionary<string, object> Session { get; set; }

        #endregion

        #region Transform-time helpers

        /// <summary>
        ///     Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if(string.IsNullOrEmpty(textToAppend))
                return;
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if(((GenerationEnvironment.Length == 0)
                || endsWithNewline))
            {
                GenerationEnvironment.Append(currentIndentField);
                endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if(textToAppend.EndsWith(Environment.NewLine, StringComparison.CurrentCulture))
                endsWithNewline = true;
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if((currentIndentField.Length == 0))
            {
                GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(Environment.NewLine, (Environment.NewLine + currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if(endsWithNewline)
                GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - currentIndentField.Length));
            else
                GenerationEnvironment.Append(textToAppend);
        }
        /// <summary>
        ///     Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            Write(textToAppend);
            GenerationEnvironment.AppendLine();
            endsWithNewline = true;
        }
        /// <summary>
        ///     Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args) { Write(string.Format(CultureInfo.CurrentCulture, format, args)); }
        /// <summary>
        ///     Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args) { WriteLine(string.Format(CultureInfo.CurrentCulture, format, args)); }
        /// <summary>
        ///     Raise an error
        /// </summary>
        public void Error(string message)
        {
            var error = new CompilerError();
            error.ErrorText = message;
            Errors.Add(error);
        }
        /// <summary>
        ///     Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            var error = new CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            Errors.Add(error);
        }
        /// <summary>
        ///     Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if((indent == null))
                throw new ArgumentNullException("indent");
            currentIndentField = (currentIndentField + indent);
            indentLengths.Add(indent.Length);
        }
        /// <summary>
        ///     Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            var returnValue = "";
            if((indentLengths.Count > 0))
            {
                var indentLength = indentLengths[(indentLengths.Count - 1)];
                indentLengths.RemoveAt((indentLengths.Count - 1));
                if((indentLength > 0))
                {
                    returnValue = currentIndentField.Substring((currentIndentField.Length - indentLength));
                    currentIndentField = currentIndentField.Remove((currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        ///     Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            indentLengths.Clear();
            currentIndentField = "";
        }

        #endregion

        #region ToString Helpers

        /// <summary>
        ///     Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            IFormatProvider formatProviderField = CultureInfo.InvariantCulture;
            /// <summary>
            ///     Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public IFormatProvider FormatProvider
            {
                get { return formatProviderField; }
                set
                {
                    if((value != null))
                        formatProviderField = value;
                }
            }
            /// <summary>
            ///     This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if((objectToConvert == null))
                    throw new ArgumentNullException("objectToConvert");
                var t = objectToConvert.GetType();
                var method = t.GetMethod("ToString", new[]
                                                     {
                                                         typeof(IFormatProvider)
                                                     });
                if((method == null))
                    return objectToConvert.ToString();
                else
                    return ((string) (method.Invoke(objectToConvert, new object[]
                                                                     {
                                                                         formatProviderField
                                                                     })));
            }
        }

        ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper { get { return toStringHelperField; } }

        #endregion
    }

    #endregion
}