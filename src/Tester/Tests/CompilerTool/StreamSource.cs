using System.Collections;
using hw.Scanner;

namespace Tester.Tests.CompilerTool;

[UnitTest]
public sealed class StreamSource : DependenceProvider
{
    sealed class StreamProvider(string fileName, int bufferSize = 100_000)
        : DumpableObject, ISourceProvider, ITextProvider
    {
        readonly int Length = (int)long.Min(new FileInfo(fileName).Length, int.MaxValue);

        readonly StreamReader Reader = new(new FileStream(fileName, FileMode.Open, FileAccess.Read),
            bufferSize: bufferSize);

        long CurrentPosition;

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<char>)this).GetEnumerator();

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            SeekIfNeeded(0);
            int ch;
            while((ch = Reader.Read()) >= 0)
            {
                CurrentPosition++;
                yield return (char)ch;
            }
        }

        ITextProvider ISourceProvider.Data => this;
        string? ISourceProvider.Identifier => fileName;
        bool ISourceProvider.IsPersistent => true;
        int ISourceProvider.Length => Length;

        string ITextProvider.this[Range range]
        {
            get
            {
                var (offset, length) = range.GetOffsetAndLength(Length);

                SeekIfNeeded(offset);

                var buffer = new char[length];
                var actualRead = Reader.ReadBlock(buffer, 0, length);
                CurrentPosition += actualRead;

                return new(buffer, 0, actualRead);
            }
        }

        char ITextProvider.this[Index index]
        {
            get
            {
                var offset = index.GetOffset(Length);

                SeekIfNeeded(offset);

                var ch = Reader.Read();
                if(ch < 0)
                    throw new IndexOutOfRangeException($"Index {offset} is out of range");

                CurrentPosition++;
                return (char)ch;
            }
        }

        void SeekIfNeeded(long position)
        {
            if(CurrentPosition == position)
                return;
            
            Reader.BaseStream.Seek(position, SeekOrigin.Begin);
            Reader.DiscardBufferedData();
            CurrentPosition = position;
        }
    }


    [UnitTest]
    public static void ParseLong()
    {
        var testFile = "ParseLong.test".ToSmbFile();

        WriteRandomLines(testFile.FullName, 100_000_000);

        var source = new Source(new StreamProvider(testFile.FullName)) + 0;

        var lineEndOrEnd = "\r\n".Box().Else("\n".Box()).Else("\r".Box()).Else(hw.Scanner.Match.End);
        var count = 0;
        for(; !source.IsEnd; count++)
            source += source.Match(lineEndOrEnd.Find)!.Value;

        $"{count}".Log();

        testFile.Delete();
    }

    static void WriteRandomLines(string filePath, int targetSizeBytes = 1_000_000, int seed = 42)
    {
        var random = new Random(seed);

        using var writer = new StreamWriter(filePath);
        long size = 0;

        while(size < targetSizeBytes)
        {
            var length = random.Next(0, 101);
            var line = string.Create(length, random, (span, rnd) =>
            {
                for(var i = 0; i < span.Length; i++)
                    span[i] = (char)rnd.Next(33, 127); // Druckbare ASCII-Zeichen
            });

            writer.WriteLine(line);
            size += line.Length + Environment.NewLine.Length;
        }
    }
}