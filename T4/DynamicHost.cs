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

using System.Reflection;
using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HWClassLibrary.T4
{
    sealed class DynamicHost : IDynamicHost
    {
        readonly object _instance;
        readonly MethodInfo _resolveParameterValue;
        readonly MethodInfo _resolvePath;
        readonly PropertyInfo _templateFile;

        internal DynamicHost(object instance)
        {
            _instance = instance;
            var type = _instance.GetType();
            _resolveParameterValue = type.GetMethod("ResolveParameterValue", new[] {typeof(string), typeof(string), typeof(string)});
            _resolvePath = type.GetMethod("ResolvePath", new[] {typeof(string)});
            _templateFile = type.GetProperty("TemplateFile");
        }

        public string ResolveParameterValue(string id, string name, string otherName) { return (string) _resolveParameterValue.Invoke(_instance, new object[] {id, name, otherName}); }
        public string ResolvePath(string path) { return (string) _resolvePath.Invoke(_instance, new object[] {path}); }
        public string TemplateFile { get { return (string) _templateFile.GetValue(_instance, null); } }
        public IServiceProvider AsIServiceProvider() { return _instance as IServiceProvider; }
    }
}