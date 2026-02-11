using hw.Parser;
using Tester.Tests.CompilerTool;

// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool;

[UnitTest]
public sealed class BracketBalance
{
    [UnitTest]
    public static void Simple()
    {
        var expr = "a b c";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(syntax => syntax.BracketBalance());
        (depth == 0).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }

    [UnitTest]
    public static void Simple1()
    {
        var expr = "(a b c)";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(syntax => syntax.BracketBalance());
        (depth == 0).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }

    [UnitTest]
    public static void Mixed()
    {
        var expr = "(a b {}c)";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(syntax => syntax.BracketBalance());
        (depth == 0).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }

    [UnitTest]
    public static void SimpleUnmatched()
    {
        var expr = "{";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(syntax => syntax.BracketBalance());
        (depth == -1).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }
    [UnitTest]
    public static void MixedUnmatched1()
    {
        var expr = "a s{a b (c} d";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(item => item.BracketBalance());
        (depth == -1).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }

    [UnitTest]
    public static void MixedUnmatched2()
    {
        var expr = "a s{a b )c}";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(syntax => syntax.BracketBalance());
        (depth == 0).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }

    [UnitTest]
    public static void Unmatched1()
    {
        var expr = "a sa b (c";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(syntax => syntax.BracketBalance());
        (depth == -1).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }

    [UnitTest]
    public static void Unmatched2()
    {
        var expr = "a sa b )c";
        var result = ParserUtil.Parse(expr);
        var items = result.Tokens;
        var depth = items.Sum(syntax => syntax.BracketBalance());
        (depth == 0).Assert
        (() =>
            items.Select(item => item.Characters.Id + ":" + item.BracketBalance())
                .Stringify(" "));
    }
}