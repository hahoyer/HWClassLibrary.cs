using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace hw.UnitTest
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [MeansImplicitUse]
    public sealed class UnitTestAttribute : Attribute
    {
        public string DefaultMethod;
        public readonly SourceFilePosition Where;

        public UnitTestAttribute
            ([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Where = new SourceFilePosition
            {
                FileName = fileName,
                LineNumber = lineNumber
            };
        }
    }
}