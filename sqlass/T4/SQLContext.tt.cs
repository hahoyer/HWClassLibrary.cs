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

namespace HWClassLibrary.sqlass.T4
{
    using System;


#line 1 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
    [GeneratedCode("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class SQLContext : SQLContextBase
    {
        public virtual string TransformText()
        {
            Write("using System;\r\nusing HWClassLibrary.Helper;\r\nusing HWClassLibrary.sqlass;\r\nusing " +
                  "");

#line 5 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
            Write(ToStringHelper.ToStringWithCulture(_context.NameSpace));

#line default
#line hidden
            Write(".Tables;\r\n\r\nnamespace ");

#line 7 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
            Write(ToStringHelper.ToStringWithCulture(_context.NameSpace));

#line default
#line hidden
            Write("\r\n{\r\n    sealed public class Container\r\n    {");

#line 10 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"

            foreach(var table in _tables)
            {
#line default
#line hidden
                Write(" \r\n        public readonly ");

#line 13 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
                Write(ToStringHelper.ToStringWithCulture(table.TableTypeName));

#line default
#line hidden
                Write(" ");

#line 13 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
                Write(ToStringHelper.ToStringWithCulture(table.ClassName));

#line default
#line hidden
                Write(";");

#line 13 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
            }

#line default
#line hidden
            Write("\r\n        public Container(Context context)\r\n        {");

#line 17 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"

            foreach(var table in _tables)
            {
#line default
#line hidden
                Write(" \r\n            ");

#line 20 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
                Write(ToStringHelper.ToStringWithCulture(table.ClassName));

#line default
#line hidden
                Write(" = new ");

#line 20 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
                Write(ToStringHelper.ToStringWithCulture(table.TableTypeName));

#line default
#line hidden
                Write("(context, SQLSupport.");

#line 20 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
                Write(ToStringHelper.ToStringWithCulture(table.ClassName));

#line default
#line hidden
                Write(".MetaDataSupport);");

#line 20 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"
            }

#line default
#line hidden
            Write(@"
        }
    }

    sealed public partial class Context: HWClassLibrary.sqlass.Context
    {
        public readonly Container Container;
        public Context(){Container = new Container(this);}
        public void UpdateDatabase() { UpdateDatabase(Container); }
    }
}
");

#line 33 "C:\disks\anne.data\data\develop\HWsqlass\src\HWClassLibrary\sqlass\T4\SQLContext.tt"

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
    public class SQLContextBase
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