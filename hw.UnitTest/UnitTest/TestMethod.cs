// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;

namespace HWClassLibrary.UnitTest
{
    sealed class TestMethod : Dumpable
    {
        readonly MethodInfo _methodInfo;
        public bool IsSuspended;
        public TestMethod(MethodInfo methodInfo) { _methodInfo = methodInfo; }

        public string ConfigurationString { get { return Name + ","; } }

        public string Name { get { return _methodInfo.Name; } }

        void ShowException(Exception e)
        {
            Tracer.Assert(_methodInfo.DeclaringType != null);
            Tracer.Line("*********************Exception: " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            Tracer.Line(e.GetType().FullName);
            Tracer.Line(e.Message);
            Tracer.Line("*********************End Exception: " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            throw new TestFailedException();
        }

        public void Run()
        {
            Tracer.Assert(_methodInfo.DeclaringType != null);
            Tracer.Line("Start " + _methodInfo.ReturnType.Name + " " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            Tracer.IndentStart();
            try
            {
                if(!IsSuspended)
                {
                    var test = Activator.CreateInstance(_methodInfo.ReflectedType);
                    var isBreakDisabled = Tracer.IsBreakDisabled;
                    Tracer.IsBreakDisabled = !TestRunner.IsModeErrorFocus;
                    try
                    {
                        _methodInfo.Invoke(test, new object[0]);
                    }
                    catch(Exception e)
                    {
                        ShowException(e);
                    }
                    Tracer.IsBreakDisabled = isBreakDisabled;
                }
            }
            finally
            {
                Tracer.IndentEnd();
                Tracer.Assert(_methodInfo.DeclaringType != null);
                Tracer.Line("End " + _methodInfo.ReturnType.Name + " " + _methodInfo.DeclaringType.FullName + "." + _methodInfo.Name);
            }
        }
    }
}