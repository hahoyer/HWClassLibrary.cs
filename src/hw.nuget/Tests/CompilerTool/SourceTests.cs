using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;
// ReSharper disable CheckNamespace

namespace hw.Tests.CompilerTool
{
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
            Tracer.Assert(s.LineIndex == 0);
            Tracer.Assert(s.ColumnIndex == 0);

            s = source.FromLineAndColumn(1, 2);
            Tracer.Assert(s.LineIndex == 1);
            Tracer.Assert(s.ColumnIndex == 2);

            s = source.FromLineAndColumn(2, 12);
            Tracer.Assert(s.LineIndex == 2);
            Tracer.Assert(s.ColumnIndex == 6);

            s = source.FromLineAndColumn(100, 12);
            Tracer.Assert(s.LineIndex == 9);
            Tracer.Assert(s.ColumnIndex == 0);
        }
    }
}