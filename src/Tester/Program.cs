using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hw.Tests.ReplaceVariables;
using hw.UnitTest;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Example.TestMethod();
            //TestDump.M1(); 
            TestRunner.RunTests(Assembly.GetExecutingAssembly());
        }
    }
}