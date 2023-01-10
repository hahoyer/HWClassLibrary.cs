using System;
using hw.UnitTest;

namespace TestUnitTest;

// ReSharper disable once ClassCanBeSealed.Global
public class NotTaggedClass { }

[UnitTest]
public abstract class AbstractClass { }

[UnitTest]
public class BareClass { }

[UnitTest]
public sealed class SealedClass { }

[UnitTest]
public static class StaticClass { }

[UnitTest]
public class InterfaceTest : ITestFixture
{
    void ITestFixture.Run() => throw new NotImplementedException();
}