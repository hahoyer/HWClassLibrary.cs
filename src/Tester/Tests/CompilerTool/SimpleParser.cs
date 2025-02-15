﻿using hw.DebugFormatter;
using hw.UnitTest;

// ReSharper disable UnusedVariable
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool;

[UnitTest]
public sealed class SimpleParser : DependenceProvider
{
    [UnitTest]
    public static void NormalParser()
        => ParserUtil.ParseAndCheck
        (
            "a b c",
            "(((<null> a <null>) b <null>) c <null>)"
        );

    [UnitTest]
    public static void NotWrongExpression() => ParserUtil.ParseAndCheck
    (
        "(a;b);c",
        "((((<null> a <null>) ; (<null> b <null>)) () <null>) ; (<null> c <null>))"
    );

    [UnitTest]
    public static void InvalidComment() => ParserUtil.ParseAndCheck
    (
        "#( \na",
        "(<null> ?EOFInComment <null>)"
    );

    [UnitTest]
    public static void PreceededInvalidComment()
    {
        var result = ParserUtil.Parse("    #( \na");
        (result.Token.PrecededWith > 0).Assert();
    }

    [UnitTest]
    public static void Expression()
    {
        var expr = "(a*b+c*d)*(e*f+g*h)";
        var subResult = ParserUtil.Parse(expr).Dump();

        var result = ParserUtil.Parse(expr + " + " + expr).Dump();
        var xresult = result
                .Replace(subResult, "<expr>")
                .Replace("(<null> a <null>)", "<a>")
                .Replace("(<null> b <null>)", "<b>")
                .Replace("(<null> c <null>)", "<c>")
                .Replace("(<null> d <null>)", "<d>")
                .Replace("(<null> e <null>)", "<e>")
                .Replace("(<null> f <null>)", "<f>")
                .Replace("(<null> g <null>)", "<g>")
                .Replace("(<null> h <null>)", "<h>")
            ;

        ParserUtil.ParseAndCheck(expr + " + " + expr, "(" + subResult + " + " + subResult + ")");
    }
}