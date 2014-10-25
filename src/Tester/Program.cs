using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Tests.CompilerTool;
using hw.Tests.ReplaceVariables;
using hw.UnitTest;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.NormalParser();
            Parser.NestedParser();
            Parser.NestedParserMultipleEntries();
            //TestDump.M1(); 
            TestRunner.RunTests(Assembly.GetExecutingAssembly());
        }
    }
}