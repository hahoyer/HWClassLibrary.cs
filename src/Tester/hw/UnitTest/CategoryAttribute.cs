using System;
using System.Runtime.CompilerServices;
using hw.Helper;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.UnitTest
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [MeansImplicitUse]
    [PublicAPI]
    public sealed class UnitTestAttribute : LocationProviderAttribute
    {
        public string DefaultMethod;

        public UnitTestAttribute([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
            => Where = new SourceFilePosition {FileName = fileName, LineNumber = lineNumber};
    }
}