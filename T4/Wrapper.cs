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

using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using EnvDTE;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using JetBrains.Annotations;

namespace HWClassLibrary.T4
{
    sealed class Wrapper
    {
        readonly object _instance;
        IDynamicHost _dynamicHost;

        readonly MethodInfo _write;
        readonly MethodInfo _writeLine;
        readonly PropertyInfo _generationEnvironment;
        readonly PropertyInfo _errors;
        readonly PropertyInfo _host;
        readonly SimpleCache<FileManager> _fileManager;
        readonly SimpleCache<DTE> _dte;

        internal static Wrapper Create([NotNull] object instance)
        {
            return 
                instance as Wrapper 
                ?? new Wrapper(instance);
        }

        Wrapper(object instance)
        {
            _instance = instance;
            var type = _instance.GetType();
            _write = type.GetMethod("Write", new[] {typeof(string)});
            _writeLine = type.GetMethod("WriteLine", new[] {typeof(string)});
            _generationEnvironment = type.GetProperty("GenerationEnvironment", BindingFlags.Instance | BindingFlags.NonPublic);
            _host = type.GetProperty("Host");
            _errors = type.GetProperty("Errors");
            _fileManager = new SimpleCache<FileManager>(() => new FileManager(this));
            _dte = new SimpleCache<DTE>(ObtainDTE);
        }

        internal FileManager FileManager { get { return _fileManager.Value; } }
        internal DTE DTE { get { return _dte.Value; } }
        internal StringBuilder GenerationEnvironment { get { return (StringBuilder) _generationEnvironment.GetValue(_instance, null); } }
        internal CompilerErrorCollection Errors { get { return (CompilerErrorCollection) _errors.GetValue(_instance, null); } }
        internal void Write(string text) { _write.Invoke(_instance, new object[] {text}); }
        internal void WriteLine(string text) { _writeLine.Invoke(_instance, new object[] {text}); }

        public IDynamicHost Host
        {
            get
            {
                if(_dynamicHost == null)
                    if(_host == null)
                        _dynamicHost = new NullHost();
                    else
                        _dynamicHost = new DynamicHost(_host.GetValue(_instance, null));
                return _dynamicHost;
            }
        }

        DTE ObtainDTE()
        {
            var provider = Host.AsIServiceProvider();
            if(provider == null)
                return null;
            return (DTE) provider.GetService(typeof(DTE));
        }

        internal ProjectItem TemplateProjectItem()
        {
            if(DTE == null)
                return null;
            return DTE.Solution.FindProjectItem(Host.TemplateFile);
        }

        internal void CheckoutFileIfRequired(string fileName)
        {
            if (DTE == null
                || DTE.SourceControl == null
                || !DTE.SourceControl.IsItemUnderSCC(fileName)
                || DTE.SourceControl.IsItemCheckedOut(fileName))
                return;

            Action<string> checkOutAction = name => DTE.SourceControl.CheckOutItem(name);
            checkOutAction.EndInvoke(checkOutAction.BeginInvoke(fileName, null, null));
        }
    }
}
