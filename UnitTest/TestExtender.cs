using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HWClassLibrary.Helper;

namespace HWClassLibrary.UnitTest
{
    public static class TestExtender
    {
        public static void RunTests(this Assembly rootAssembly) { TestRunner.RunTests(rootAssembly); }
    }
}