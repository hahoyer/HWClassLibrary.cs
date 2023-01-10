using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.UnitTest;

[PublicAPI]
public sealed class TestRunner : Dumpable
{
    [PublicAPI]
    public class ConfigurationContainer
    {
        public bool IsBreakEnabled;
        public bool SaveResults;
        public bool SkipSuccessfulMethods;
        public string TestsFileName;
    }


    public static readonly ConfigurationContainer Configuration = new();

    public static readonly List<IFramework> RegisteredFrameworks = new();

    // ReSharper disable once StringLiteralTypo
    readonly SmbFile ConfigFile = "Test.HW.config".ToSmbFile();
    readonly SmbFile PendingTestsFile = Configuration.TestsFileName?.ToSmbFile();

    readonly Func<Type, bool>[] TestLevels;
    readonly TestType[] TestTypes;
    int Complete;
    string CurrentMethodName = "";
    string Status = "Start";

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

    string PendingTestsString
        => $@"//{HeaderText}

// ReSharper disable once CheckNamespace
namespace hw.UnitTest;
public static class PendingTests
{{
    public static void Run()
    {{
    {GeneratedTestCalls}
}}}}
";

    string GeneratedTestCalls
        => TestTypes
            .SelectMany(type => type.PendingTestsMethods.Select(method => (type, method)))
            .Where(item => !item.type.IsSuccessful)
            .OrderBy(item => item.type.GetPriority(item.method))
            .GroupBy(item => item.type.GetMode(item.method))
            .Select(GeneratedTestCallsForMode)
            .Stringify("\n");

    string HeaderText => $"{DateTime.Now.Format()} {Status} {Complete} of {TestTypes.Length} {CurrentMethodName}";

    public static bool IsModeErrorFocus
    {
        set
        {
            Configuration.IsBreakEnabled = value;
            Configuration.SaveResults = !value;
            Configuration.SkipSuccessfulMethods = value;
        }
    }

    TestRunner(IEnumerable<TestType> testTypes)
    {
        TestLevels = new[] { IsNormalPriority, IsLowPriority };
        TestTypes = testTypes.ToArray();
        TestTypes.IsCircuitFree(DependentTypes).Assert
            (() => Tracer.Dump(TestTypes.Circuits(DependentTypes).ToArray()));
        if(Configuration.SkipSuccessfulMethods)
            LoadConfiguration();
    }

    string GeneratedTestCallsForMode(IGrouping<string, (TestType type, TestMethod method)> group)
        => $"\n// {group.Key} \n\n"
            + group
                .Select(testType => $"{testType.method.RunString};")
                .Stringify("\n");

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
        if(Configuration.SkipSuccessfulMethods)
            return new TestType[0];
        return
            type.DependenceProviders.SelectMany
                (attribute => attribute.AsTestType(TestTypes).NullableToArray()).ToArray();
    }

    void Run()
    {
        "".Log();
        "".Log();
        "======================================".Log();
        "Test run started.".Log();
        "======================================".Log();
        "".Log();
        PendingTestsFile?.FilePosition(null, FilePositionTag.Test).Log();
        Status = "run";
        for(var index = 0; index < TestLevels.Length && AllIsFine; index++)
        {
            var level = TestLevels[index];
            while(RunLevel(level)) { }
        }

        Status = "ran";
        SaveConfiguration();
        "".Log();
        "======================================".Log();
        "Test run completed.".Log();
        "======================================".Log();
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
                    Run(openTest);
                    Complete++;
                    hasAnyTestRan = true;
                }
            }
        }

        return hasAnyTestRan;
    }

    void Run(TestType type)
    {
        foreach(var method in type.UnitTestMethods.Where(unitTestMethod => !unitTestMethod.IsSuspended))
            try
            {
                method.IsActive = true;
                CurrentMethodName = method.LongName;
                SaveConfiguration();
                CurrentMethodName = "";

                method.Run();
                method.IsSuccessful = true;
            }
            catch(TestFailedException)
            {
                type.FailedMethods.Add(method);
                method.IsSuccessful = false;
            }
            finally
            {
                method.IsActive = false;
            }

        type.IsComplete = true;
    }

    void SaveConfiguration()
    {
        try
        {
            if(Configuration.SaveResults)
            {
                ConfigFile.String = ConfigurationString;
                ConfigFileMessage("Configuration saved");
            }

            if(PendingTestsFile != null)
                PendingTestsFile.String = PendingTestsString;
        }
        catch(Exception)
        {
            // ignored
        }
    }

    void ConfigFileMessage(string flagText)
        => (Tracer.FilePosition(ConfigFile.FullName, null, FilePositionTag.Test) + flagText).Log();


    void LoadConfiguration()
    {
        ConfigurationString = ConfigFile.String;
        ConfigFileMessage("Configuration loaded");
    }

    internal static IEnumerable<TestType> GetUnitTestTypes(Assembly rootAssembly) => rootAssembly
        .GetReferencedTypes()
        .Where(IsUnitTestType)
        .Select(type => new TestType(type));

    internal static bool IsUnitTestType(Type type)
    {
        if(type.IsAbstract && !type.IsSealed)
            return false;
        if(type.GetAttribute<UnitTestAttribute>(true) != null)
            return true;
        return RegisteredFrameworks.Any(any => any.IsUnitTestType(type));
    }

    public static void RunTest(Action action) => new TestMethod(action).Run();
    public static void RunTest(MethodInfo method) => new TestMethod(method).Run();
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