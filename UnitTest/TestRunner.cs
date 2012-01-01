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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.Relation;

namespace HWClassLibrary.UnitTest
{
    public sealed class TestRunner : Dumpable
    {
        readonly TestType[] _testTypes;
        public static bool IsModeErrorFocus;
        readonly File _configFile = File.m("Test.HWconfig");
        string _status = "Start";
        int _complete;
        string _currentMethodName = "";

        TestRunner(IEnumerable<TestType> testTypes)
        {
            _testTypes = testTypes.ToArray();
            Tracer.Assert(_testTypes.IsCircuidFree(Dependants), () => Tracer.Dump(_testTypes.Circuids(Dependants).ToArray()));
            if(IsModeErrorFocus)
                LoadConfiguration();
        }

        internal static void RunTests(Assembly rootAssembly) { new TestRunner(GetUnitTestTypes(rootAssembly)).Run(); }

        TestType[] Dependants(TestType type)
        {
            if(IsModeErrorFocus)
                return new TestType[0];
            return type
                .Dependants
                .Select(attribute => attribute.AsTestType(_testTypes))
                .ToArray();
        }

        void Run()
        {
            _status = "run";
            while(RunLevel())
                continue;
            _status = "ran";
            SaveConfiguration();
        }

        bool RunLevel()
        {
            var openTests = _testTypes.Where(x => x.IsStartable).ToArray();
            if(openTests.Length == 0)
                return false;

            foreach(var openTest in openTests)
            {
                var dependants = Dependants(openTest);
                if(dependants.All(test => test.IsStarted))
                {
                    openTest.IsStarted = true;
                    if(dependants.All(test => test.IsSuccessfull))
                    {
                        if(!IsModeErrorFocus)
                        {
                            _currentMethodName = openTest.Type.FullName;
                            SaveConfiguration();
                            _currentMethodName = "";
                        }
                        openTest.Run();
                        _complete++;
                    }
                }
            }
            return true;
        }

        string ConfigurationString
        {
            get
            {
                return HeaderText + "\n" +
                       _testTypes
                           .OrderBy(t => t.ConfigurationModePriority)
                           .Aggregate("", (current, testType) => current + testType.ConfigurationString);
            }
            set
            {
                if(value == null)
                    return;
                var pairs = value.Split('\n')
                    .Where((line, i) => i > 0 && line != "")
                    .Join(_testTypes, line => line.Split(' ')[1], type => type.Type.FullName, (line, type) => new {line, type});
                foreach(var pair in pairs)
                    pair.type.ConfigurationString = pair.line;
            }
        }

        string HeaderText { get { return DateTime.Now.Format() + " " + _status + " " + _complete + " of " + _testTypes.Length + " " + _currentMethodName; } }

        void SaveConfiguration()
        {
            _configFile.String = ConfigurationString;
            ConfigFileMessage("Configuration saved");
        }

        void ConfigFileMessage(string flagText) { Tracer.Line(Tracer.FilePosn(_configFile.FullName, 1, 1, flagText)); }


        void LoadConfiguration()
        {
            ConfigurationString = _configFile.String;
            ConfigFileMessage("Configuration loaded");
        }

        static IEnumerable<TestType> GetUnitTestTypes(Assembly rootAssembly)
        {
            return rootAssembly
                .GetReferencedTypes()
                .Where(type => !(type.IsAbstract || type.GetAttribute<TestFixtureAttribute>(true) == null))
                .Select(methodInfo => new TestType(methodInfo));
        }
    }

    sealed class TestFailedException : Exception
    {}
}