using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.T4
{
    interface IDynamicHost
    {
        /// <summary>
        ///     An abstracted call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost ResolveParameterValue
        /// </summary>
        string ResolveParameterValue(string id, string name, string otherName);

        /// <summary>
        ///     An abstracted call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost ResolvePath
        /// </summary>
        string ResolvePath(string path);

        /// <summary>
        ///     An abstracted call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost TemplateFile
        /// </summary>
        string TemplateFile { get; }

        /// <summary>
        ///     Returns the Host instance cast as an IServiceProvider
        /// </summary>
        IServiceProvider AsIServiceProvider();
    }
}