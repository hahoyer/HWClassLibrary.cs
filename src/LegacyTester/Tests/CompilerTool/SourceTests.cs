using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;

// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool;

[TestFixture]
[UnitTest]
public sealed class SourceTests
{
    const string Text = @"asdf
1234356
qwertz






";

    [Test]
    [UnitTest]
    public void FromLineAndColumn()
    {
        var source = new Source(Text);

        var s = source.FromLineAndColumn(0, 0);
        (s.LineIndex == 0).Assert();
        (s.ColumnIndex == 0).Assert();

        s = source.FromLineAndColumn(1, 2);
        (s.LineIndex == 1).Assert();
        (s.ColumnIndex == 2).Assert();

        s = source.FromLineAndColumn(2, 12);
        (s.LineIndex == 2).Assert();
        (s.ColumnIndex == 6).Assert();

        s = source.FromLineAndColumn(100, 12);
        (s.LineIndex == 9).Assert();
        (s.ColumnIndex == 0).Assert();
    }
}