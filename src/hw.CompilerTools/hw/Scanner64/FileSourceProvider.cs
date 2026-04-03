using System.Collections;
using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Scanner64;

public sealed class FileSourceProvider : ISourceProvider, ITextProvider
{
    sealed class Enumerator(FileSourceProvider parent) : IEnumerator<char>
    {
        long Cursor;
        void IDisposable.Dispose() { }

        object IEnumerator.Current => parent.File[Cursor];

        bool IEnumerator.MoveNext()
        {
            if(Cursor >= parent.File.Parent.Size)
                return false;
            Cursor++;
            return true;
        }

        void IEnumerator.Reset() => Cursor = 0;

        char IEnumerator<char>.Current => parent.File[Cursor];
    }

    readonly SmbFileBuffer File;

    public FileSourceProvider(SmbFile file, int? blockSize = null)
        => File = new(file, blockSize);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => new Enumerator(this);

    ITextProvider ISourceProvider.Data => this;

    string ISourceProvider.Identifier => File.Parent.FullName;

    bool ISourceProvider.IsPersistent => true;
    long ISourceProvider.Length => File.Parent.Size;
    string ITextProvider.this[Range64 range] => File[range];
    char ITextProvider.this[Index64 index] => File[index];
}