

// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool;

[UnitTest]
public sealed class UnmatchedBrackets : DependenceProvider
{
    [UnitTest]
    public static void MissingRight()
        => ParserUtil.ParseAndCheck
        (
            "{a b( } c",
            "((((<null> a <null>) b (<null> ?(? <null>)) () <null>) c <null>)"
        );

    [UnitTest]
    public static void MissingLeft()
        => ParserUtil.ParseAndCheck
        (
            "{a b) } c",
            "(((((<null> a <null>) b <null>) ?)? <null>) () <null>) c <null>)"
        );
}