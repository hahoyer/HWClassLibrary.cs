using System;
using System.Runtime.CompilerServices;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.UnitTest;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
[MeansImplicitUse]
[PublicAPI]
public sealed class UnitTestAttribute : LocationProviderAttribute
{
    public string DefaultMethod;

    public UnitTestAttribute([CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        => Where = new() { FileName = fileName, LineNumber = lineNumber };
}

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
[PublicAPI]
public sealed class TestFixture : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
[PublicAPI]
public sealed class TestAttribute : Attribute { }