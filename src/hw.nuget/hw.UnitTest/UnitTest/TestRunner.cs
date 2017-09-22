using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;

namespace hw.UnitTest
{
    public sealed class TestRunner : Dumpable
    {
        readonly TestType[] _testTypes;
        public static bool IsModeErrorFocus;
        readonly SmbFile _configFile = "Test.HWconfig".ToSmbFile();
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
            Tracer.Assert
                (
                    _testTypes.IsCircuidFree(Dependants),
                    () => Tracer.Dump(_testTypes.Circuids(Dependants).ToArray()));
            if(IsModeErrorFocus)
                LoadConfiguration();
        }

        public static bool RunTests(Assembly rootAssembly)
        {
            var testRunner = new TestRunner(GetUnitTestTypes(rootAssembly));
            testRunner.Run();
            return testRunner.AllIsFine;
        }

        TestType[] Dependants(TestType type)
        {
            if(IsModeErrorFocus)
                return new TestType[0];
            return
                type.Dependants.SelectMany
                    (attribute => attribute.AsTestType(_testTypes).NullableToArray()).ToArray();
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
            get
            {
                return HeaderText + "\n"
                    + _testTypes.OrderBy(t => t.ConfigurationModePriority)
                        .Aggregate
                        ("", (current, testType) => current + testType.ConfigurationString);
            }
            set
            {
                if(value == null)
                    return;
                var pairs = value.Split('\n')
                    .Where((line, i) => i > 0 && line != "")
                    .Join
                    (
                        _testTypes,
                        line => line.Split(' ')[1],
                        type => type.Type.FullName,
                        (line, type) => new
                        {
                            line,
                            type
                        });
                foreach(var pair in pairs)
                    pair.type.ConfigurationString = pair.line;
            }
        }

        string HeaderText
        {
            get
            {
                return DateTime.Now.Format() + " " + _status + " " + _complete + " of "
                    + _testTypes.Length + " " + _currentMethodName;
            }
        }

        void SaveConfiguration()
        {
            _configFile.String = ConfigurationString;
            ConfigFileMessage("Configuration saved");
        }

        void ConfigFileMessage(string flagText)
        {
            Tracer.Line(Tracer.FilePosn(_configFile.FullName, 1, 1, FilePositionTag.Test) + flagText);
        }


        void LoadConfiguration()
        {
            ConfigurationString = _configFile.String;
            ConfigFileMessage("Configuration loaded");
        }

        static IEnumerable<TestType> GetUnitTestTypes(Assembly rootAssembly)
        {
            return rootAssembly
                .GetReferencedTypes()
                .Where
                (type => IsUnitTestType(type))
                .Select(type => new TestType(type));
        }

        static bool IsUnitTestType(Type type)
        {
            if(!type.IsSealed)
                return false;
            if(type.GetAttribute<UnitTestAttribute>(true) != null)
                return true;
            return RegisteredFrameworks.Any(any => any.IsUnitTestType(type));
        }

        public static readonly List<IFramework> RegisteredFrameworks = new List<IFramework>();
    }

    public interface IFramework
    {
        bool IsUnitTestType(Type type);
        bool IsUnitTestMethod(MethodInfo methodInfo);
    }

    public class AttributedFramework<TType, TMethod> : IFramework
        where TType : Attribute
        where TMethod : Attribute
    {
        bool IFramework.IsUnitTestType(Type type) { return type.GetAttributes<TType>(true).Any(); }
        bool IFramework.IsUnitTestMethod(MethodInfo methodInfo)
        {
            return methodInfo.GetAttributes<TMethod>(true).Any();
        }
    }

    sealed class TestFailedException : Exception {}
}