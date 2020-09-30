using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

namespace hw.UnitTest
{
    public sealed class TestRunner : Dumpable
    {
        public static bool IsModeErrorFocus;

        public static readonly List<IFramework> RegisteredFrameworks = new List<IFramework>();
        int Complete;

        // ReSharper disable once StringLiteralTypo
        readonly SmbFile ConfigFile = "Test.HWconfig".ToSmbFile();
        string CurrentMethodName = "";
        string Status = "Start";

        readonly Func<Type, bool>[] TestLevels;
        readonly TestType[] TestTypes;

        TestRunner(IEnumerable<TestType> testTypes)
        {
            TestLevels = new Func<Type, bool>[] {IsNormalPriority, IsLowPriority};
            TestTypes = testTypes.ToArray();
            Tracer.Assert
            (
                TestTypes.IsCircuitFree(DependentTypes),
                () => Tracer.Dump(TestTypes.Circuits(DependentTypes).ToArray()));
            if(IsModeErrorFocus)
                LoadConfiguration();
        }

        bool AllIsFine => TestTypes.All(t => !t.IsStarted || t.IsSuccessful);

        string ConfigurationString
        {
            get => HeaderText
                   + "\n"
                   + TestTypes.OrderBy(testType => testType.ConfigurationModePriority)
                       .Aggregate("", (current, testType) => current + testType.ConfigurationString);
            set
            {
                if(value == null)
                    return;
                var pairs = value.Split('\n')
                    .Where((line, i) => i > 0 && line != "")
                    .Join
                    (
                        TestTypes,
                        line => line.Split(' ')[1],
                        type => type.Type.FullName,
                        (line, type) => new
                        {
                            line, type
                        });
                foreach(var pair in pairs)
                    pair.type.ConfigurationString = pair.line;
            }
        }

        string HeaderText => DateTime.Now.Format() + " " + Status + " " + Complete + " of "
                             + TestTypes.Length + " " + CurrentMethodName;

        public static bool RunTests(Assembly rootAssembly)
        {
            var testRunner = new TestRunner(GetUnitTestTypes(rootAssembly));
            testRunner.Run();
            return testRunner.AllIsFine;
        }

        static bool IsNormalPriority(Type type) => type.GetAttribute<LowPriority>(true) == null;

        static bool IsLowPriority(Type type) => true;

        TestType[] DependentTypes(TestType type)
        {
            if(IsModeErrorFocus)
                return new TestType[0];
            return
                type.DependenceProviders.SelectMany
                    (attribute => attribute.AsTestType(TestTypes).NullableToArray()).ToArray();
        }

        void Run()
        {
            Status = "run";
            for(var index = 0; index < TestLevels.Length && AllIsFine; index++)
            {
                var level = TestLevels[index];
                while(RunLevel(level)) { }
            }

            Status = "ran";
            SaveConfiguration();
        }

        bool RunLevel(Func<Type, bool> isLevel)
        {
            var openTests = TestTypes.Where(target => target.CanBeStarted(isLevel)).ToArray();
            if(openTests.Length == 0)
                return false;

            var hasAnyTestRan = false;
            foreach(var openTest in openTests)
            {
                var dependentTypes = DependentTypes(openTest);
                if(dependentTypes.All(test => test.IsStarted))
                {
                    openTest.IsStarted = true;
                    if(dependentTypes.All(test => test.IsSuccessful))
                    {
                        if(!IsModeErrorFocus)
                        {
                            CurrentMethodName = openTest.Type.FullName;
                            SaveConfiguration();
                            CurrentMethodName = "";
                        }

                        openTest.Run();
                        Complete++;
                        hasAnyTestRan = true;
                    }
                }
            }

            return hasAnyTestRan;
        }

        void SaveConfiguration()
        {
            ConfigFile.String = ConfigurationString;
            ConfigFileMessage("Configuration saved");
        }

        void ConfigFileMessage(string flagText)
            => Tracer.Line(Tracer.FilePosition(ConfigFile.FullName, 1, 1, FilePositionTag.Test) + flagText);


        void LoadConfiguration()
        {
            ConfigurationString = ConfigFile.String;
            ConfigFileMessage("Configuration loaded");
        }

        static IEnumerable<TestType> GetUnitTestTypes(Assembly rootAssembly) => rootAssembly
            .GetReferencedTypes()
            .Where(IsUnitTestType)
            .Select(type => new TestType(type));

        static bool IsUnitTestType(Type type)
        {
            if(!type.IsSealed)
                return false;
            if(type.GetAttribute<UnitTestAttribute>(true) != null)
                return true;
            return RegisteredFrameworks.Any(any => any.IsUnitTestType(type));
        }
    }

    public interface IFramework
    {
        bool IsUnitTestType(Type type);
        bool IsUnitTestMethod(MethodInfo methodInfo);
    }

    [PublicAPI]
    public class AttributedFramework<TType, TMethod> : IFramework
        where TType : Attribute
        where TMethod : Attribute
    {
        bool IFramework.IsUnitTestMethod(MethodInfo methodInfo) => methodInfo.GetAttributes<TMethod>(true).Any();
        bool IFramework.IsUnitTestType(Type type) => type.GetAttributes<TType>(true).Any();
    }

    sealed class TestFailedException : Exception { }
}