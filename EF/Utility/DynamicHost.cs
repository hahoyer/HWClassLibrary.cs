using System.Reflection;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.EF.Utility
{
    /// <summary>
    ///     Reponsible for implementing the IDynamicHost as a dynamic
    ///     shape wrapper over the Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost interface
    ///     rather than type dependent wrapper.  We don't use the
    ///     interface type so that the code can be run in preprocessed mode
    ///     on a .net framework only installed machine.
    /// </summary>
    public class DynamicHost : IDynamicHost
    {
        readonly object _instance;
        readonly MethodInfo _resolveParameterValue;
        readonly MethodInfo _resolvePath;
        readonly PropertyInfo _templateFile;

        /// <summary>
        ///     Creates an instance of the DynamicHost class around the passed in
        ///     Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost shapped instance passed in.
        /// </summary>
        public DynamicHost(object instance)
        {
            _instance = instance;
            var type = _instance.GetType();
            _resolveParameterValue = type.GetMethod("ResolveParameterValue", new[] {typeof(string), typeof(string), typeof(string)});
            _resolvePath = type.GetMethod("ResolvePath", new[] {typeof(string)});
            _templateFile = type.GetProperty("TemplateFile");
        }

        /// <summary>
        ///     A call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost ResolveParameterValue
        /// </summary>
        public string ResolveParameterValue(string id, string name, string otherName) { return (string) _resolveParameterValue.Invoke(_instance, new object[] {id, name, otherName}); }

        /// <summary>
        ///     A call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost ResolvePath
        /// </summary>
        public string ResolvePath(string path) { return (string) _resolvePath.Invoke(_instance, new object[] {path}); }

        /// <summary>
        ///     A call to Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost TemplateFile
        /// </summary>
        public string TemplateFile { get { return (string) _templateFile.GetValue(_instance, null); } }

        /// <summary>
        ///     Returns the Host instance cast as an IServiceProvider
        /// </summary>
        public IServiceProvider AsIServiceProvider() { return _instance as IServiceProvider; }
    }
}