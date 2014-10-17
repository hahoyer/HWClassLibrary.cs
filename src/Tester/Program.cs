using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using hw.Debug;
using hw.Helper;
using hw.Tests.ReplaceVariables;
using hw.UnitTest;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Log("ww");
            var a = typeof(Test).GetAttribute<TestAttribute>(true);
            Example.TestMethod();
            //TestDump.M1(); 
            TestRunner.RunTests(Assembly.GetExecutingAssembly());


        }

        static void Log
            (
            string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
            )
        {
            Tracer.Line("{0}_{1}({2}): {3}".ReplaceArgs(Path.GetFileName(file), member, line, text));
        }


    }

    sealed class TestAttribute : Attribute
    {
        readonly string _file;
        readonly string _member;
        readonly int _line;
        public TestAttribute
            (
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            _file = file;
            _member = member;
            _line = line;
        }
    }

    [TestAttribute]
    class Test{}
}