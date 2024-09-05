using hw.UnitTest;

// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool;

[UnitTest]
[TestFixture]
public sealed class UnmatchedBrackets : DependenceProvider
{
    [UnitTest]
    [Test]
    public static void MissingRight()
        => ParserUtil.ParseAndCheck
        (
            "{a b( } c",
            "((((<null> a <null>) b (<null> ?(? <null>)) () <null>) c <null>)"
        );

    [UnitTest]
    [Test]
    public static void MissingLeft()
        => ParserUtil.ParseAndCheck
        (
            "{a b) } c",
            "(((((<null> a <null>) b <null>) ?)? <null>) () <null>) c <null>)"
        );
}