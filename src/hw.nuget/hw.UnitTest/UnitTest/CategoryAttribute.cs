using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace hw.UnitTest
{
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public sealed class TestAttribute : Attribute
    {
        public readonly SourceFilePosition Where;

        public TestAttribute([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Where = new SourceFilePosition
            {
                FileName = fileName,
                LineNumber = lineNumber
            };
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public sealed class TestFixtureAttribute : Attribute
    {
        public string DefaultMethod;
        public readonly SourceFilePosition Where;

        public TestFixtureAttribute([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Where = new SourceFilePosition
            {
                FileName = fileName,
                LineNumber = lineNumber
            };
        }
    }
}