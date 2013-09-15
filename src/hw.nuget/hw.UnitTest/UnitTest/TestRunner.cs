#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Debug;
using hw.Helper;

namespace hw.UnitTest
{
    public sealed class TestRunner : Dumpable
    {
        readonly TestType[] _testTypes;
        public static bool IsModeErrorFocus;
        readonly File _configFile = "Test.HWconfig".FileHandle();
        string _status = "Start";
        int _complete;
        string _currentMethodName = "";

        readonly Func<Type, bool>[] _testLevels;

        static bool IsNormalPriority(Type type)
        {
            return type.GetAttribute<LowPriority>(true) == null;
            ;
        }
        static bool IsLowPriority(Type type) { return true; }

        TestRunner(IEnumerable<TestType> testTypes)
        {
            _testLevels = new Func<Type, bool>[] {IsNormalPriority, IsLowPriority};
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
            return type.Dependants.Select(attribute => attribute.AsTestType(_testTypes)).ToArray();
        }

        void Run()
        {
            _status = "run";
            for(var index = 0; index < _testLevels.Length && AllIsFine; index++)
            {
                var level = _testLevels[index];
                while(RunLevel(level))
                    continue;
            }

            _status = "ran";
            SaveConfiguration();
        }

        bool AllIsFine { get { return _testTypes.All(t => !t.IsStarted || t.IsSuccessfull); } }

        bool RunLevel(Func<Type, bool> isLevel)
        {
            var openTests = _testTypes.Where(x => x.IsStartable(isLevel)).ToArray();
            if(openTests.Length == 0)
                return false;

            var hasAnyTestRan = false;
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
                        hasAnyTestRan = true;
                    }
                }
            }
            return hasAnyTestRan;
        }

        string ConfigurationString
        {
            get { return HeaderText + "\n" + _testTypes.OrderBy(t => t.ConfigurationModePriority).Aggregate("", (current, testType) => current + testType.ConfigurationString); }
            set
            {
                if(value == null)
                    return;
                var pairs = value.Split('\n').Where((line, i) => i > 0 && line != "").Join(_testTypes, line => line.Split(' ')[1], type => type.Type.FullName, (line, type) => new {line, type});
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

        void ConfigFileMessage(string flagText) { Tracer.Line(Tracer.FilePosn(_configFile.FullName, 1, 1, FilePositionTag.Test) + flagText); }


        void LoadConfiguration()
        {
            ConfigurationString = _configFile.String;
            ConfigFileMessage("Configuration loaded");
        }

        static IEnumerable<TestType> GetUnitTestTypes(Assembly rootAssembly) { return rootAssembly.GetReferencedTypes().Where(type => !(type.IsAbstract || type.GetAttribute<TestFixtureAttribute>(true) == null)).Select(methodInfo => new TestType(methodInfo)); }
    }

    sealed class TestFailedException : Exception
    {}
}