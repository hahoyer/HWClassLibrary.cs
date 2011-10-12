using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.T4
{
    /// <summary>
    ///     Reponsible for implementing the IDynamicHost when the
    ///     Host property is not available on the TextTemplating type. The Host
    ///     property only exists when the hostspecific attribute of the template
    ///     directive is set to true.
    /// </summary>
    public class NullHost : IDynamicHost
    {
        /// <summary>
        ///     An abstraction of the call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost ResolveParameterValue
        ///     that simply retuns null.
        /// </summary>
        public string ResolveParameterValue(string id, string name, string otherName) { return null; }

        /// <summary>
        ///     An abstraction of the call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost ResolvePath
        ///     that simply retuns the path passed in.
        /// </summary>
        public string ResolvePath(string path) { return path; }

        /// <summary>
        ///     An abstraction of the call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost TemplateFile
        ///     that returns null.
        /// </summary>
        public string TemplateFile { get { return null; } }

        /// <summary>
        ///     Returns null.
        /// </summary>
        public IServiceProvider AsIServiceProvider() { return null; }
    }
}