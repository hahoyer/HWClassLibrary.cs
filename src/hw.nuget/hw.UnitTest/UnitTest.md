# Overview

Provides some support for unit-testing.
It is comapatible to nunit and MSTest although is provides some more features.
### Supports:
- almost any public non-abstract class can be a test class
- supports dependencies between tests
- supports tagging tests as low-priority
- supports pending-tests file as editable code file

### Restrictions: 
- No gui or automatic IDE-integration available
- Startup and teardown concepts are not supported. Use constructor and dispose if required.

# Getting started

- Mark classes and their methods with the attribute `UnitTest` to define them as unit test.
- classes must be:
  - public
  - static or sealed
  - have no template parameters (although their base classes may have them)
  - classes can be toplevel or nested
- methods must be
  - public
  - non-abstract
  - static or not
  - without parameters and return values
- include at least `Assembly.GetExecutingAssembly().RunTests()` in Main-function of your test-executable

With default configuration when executed it will create and update a file that contains the prograss of the testrun. 
The file is named `Test.HW.config` and is located at working directory of test run. 
It is also used as input for subsequent runs by not re-running successful tests.
To completely rerun all tests just delete it.

# Features 
## Test Selection
The testrunner is responsible for selecting the test to execute. 
It is done at different stages. 
1. For starting it accepts a list of assemblies.
2. For each assembly it searches all public classes that are marked as test.
3. For each such class it searches for public methods that are marked as test.
4. For any such method it checks if all run-conditions are met (see below)

**Run-conditions** are:
- Test is not marked as sussessful by `Test.HW.config`
- All dependencies are marked as successful
- Test has correct priority

#Test execution

**Important**: Each test (on method-level) is executed in its own environment. Even for excuting methods of the same class a new class instances is created for each method. 

There are the following variants for executing a test:
### Default
If the class is not static then an instance of it is created (by use of the default constructor).
This instance (or null for static classes) is used for invoking the the method.
### By Action
Only for debug purposes this test infrastructure can be used by providing a `System.Action` to test runner.

Example:
    
    [UnitTest]
    public sealed class ExampleTest
    {
        [UnitTest]
        public void ExampleMethod()
        {
            "BlaBlaBla".AssertValid()
        }
    }
     
    void Main()
    {
        ...

        TestRunner.RunTest(new ExampleTest().ExampleMethod);
    }

### By Interface ITestFixture
Your test class can implement the interface `ITestFixture`. 
There you have to provide a method Run which serves a the method to execute.
It gets a freshly created instance of the class and is executed with it. As in the default case.
Although the class still has to be provided with the UnitTest attribute, this can be omitted from the method in this case.
The default variant and the ITestFixture variant can be used in the same class.

## Custom Test Selection
By default the methods to execute are selected by attribute `UnitTest`.
This can be extended by registering test frameworks.
You should provide an class derived from IFramework and register an instance to test runner.

Here is an example (AttributedFramework is implementing IFramework):

    sealed class NunitFramework : AttributedFramework<TestFixtureAttribute,TestAttribute>
    {
        public static readonly NunitFramework Instance = new();
    }


    void RegisterIt()
    {
        TestRunner.RegisteredFrameworks.Add(NunitFramework.Instance);
    }

The registration should take place before running any test.

The above example registers all tests from the ever-famous NUnit test suite.
To implement even more advanced test selection you can implement the `IFramework`-interface. 
Look at the source code if you need to achieve this. It should be easy.

## Dependencies
You may derive your test class from class `DependenceProvider`. 
`DependenceProvider` is an abstract class derived from `System.Attribute`.
Thus you can use your test class as attribute. When you use this attributes on other test classes you definded a dependency.
`TestRunner.RunTests` will use this information as run-condition (see above).
It has no effect on `TestRunner.RunTest`.

## Priorities
A class can be flagged with the attribute `LowPriority`. In this case the tests contained in this class are only executed by `TestRunner.RunTests` when all other tests are successful.
This will also prevent dependant test from beeing executed.
It has no effect on `TestRunner.RunTest`.

## Configuration

The testrunner can be configured. This can be done according to the following example:
        
    TestRunner.Configuration.IsBreakEnabled = Debugger.IsAttached;


The following options are available:

### IsBreakEnabled
This boolean is controlling the function `Tracer.TraceBreak` of hw.Helper.Core.
### SaveResults
This boolean controlls if `Test.HW.config` is written each time a method is going to be executed by `TestRunner.RunTests`.
It has no effect on `TestRunner.RunTest`.
### SkipSuccessfulMethods
This boolean controlls if `Test.HW.config` is used to skip tests that have already been successful in a previous run.
Affects only `TestRunner.RunTests`.
### TestsFileName
This string may contain a path to a C# file. 
`TestRunner.RunTests` will write this file between each excution of a test-method.
This file will be a valid C\#-file. 
It will contain a line for each method that has not been executed successfully. 

Thus it will shrink (hopefully) during test run whenever a test method executed successfully.

The file is looking like this example: 
    
    namespace hw.UnitTest;
    public static class PendingTests
    {
        public static void Run()
        {
        
        // notrun 
        TestRunner.RunTest(new MyNameSpace.ExampleTestClass().TestMethod);
        ...
        }
    }


It is recommended to include this file into your C\#-project.
Furthermore it is recommended to call the method `PendingTests.Run` during your debug session.

This has the following very nice effects:
- You can easily watch the progress when you run the test outside the debugger
- You can easily switch to debug mode after a (partly) unsuccessful test run.
- In debug mode you are free to remove lines from this file that belong to tests fixed. 
- In debug mode you are free to add lines to this file for newly created tests.

### Recommended Configuration

In your executable you should call a function that is looking like the following example:

    static void RunAllTests()
    {
        var configuration = TestRunner.Configuration;

        if(Debugger.IsAttached)
        {
            configuration.IsBreakEnabled = true;
            configuration.SkipSuccessfulMethods = true;
            configuration.SaveResults = false;
            hw.UnitTest.PendingTests.Run();
        }
        else
        {
            configuration.IsBreakEnabled = false;
            configuration.SkipSuccessfulMethods = false;
            configuration.SaveResults = true;
            configuration.TestsFileName = SmbFile.SourcePath().PathCombine("PendingTests.cs");
            Assembly.GetExecutingAssembly().RunTests();
        }
    }


# Issues and Co.
- Report to project website on github (https://github.com/hahoyer/HWClassLibrary.cs)
- issues and feedback: https://github.com/hahoyer/HWClassLibrary.cs/issues
- [![Support me on ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/G2G4BH6WX)

