using hw.Scanner;

// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool;

[UnitTest]
public sealed class SourceTests
{
    const string Text =
        """
        asdf
        1234356
        qwertz







        """;

    sealed class NoProvider : ISourceProvider
    {
        ITextProvider? ISourceProvider.Data => null;
        bool ISourceProvider.IsPersistent => false;
        int ISourceProvider.Length => 0;
        string? ISourceProvider.Identifier => null;
    }

    [UnitTest]
    public void IsValid()
    {
        var source = new Source(Text);
        (source.IsValid).Assert();
        (!new Source(new NoProvider()).IsValid).Assert();
    }

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

    [UnitTest]
    public void IsEndFromIndex()
    {
        var source = new Source(Text);
        (!source.IsEndPosition(Index.FromStart(2))).Assert();
        (source.IsEndPosition(Index.FromStart(source.Length))).Assert();

        (!source.IsEndPosition(Index.FromEnd(2))).Assert();
        (source.IsEndPosition(Index.FromEnd(0))).Assert();
        (source.IsEndPosition(Index.End)).Assert();
    }
}