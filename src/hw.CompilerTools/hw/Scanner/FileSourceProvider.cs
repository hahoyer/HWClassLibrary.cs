using System.Collections;
using hw.Helper;
using static System.Runtime.InteropServices.JavaScript.JSType;

// ReSharper disable CheckNamespace

namespace hw.Scanner;

public sealed class FileSourceProvider : ISourceProvider, ITextProvider
{
    readonly ValueCache<string?>? DataCache;
    readonly SmbFile File;

    public FileSourceProvider(SmbFile file, bool useCache = true)
    {
        File = file;
        if(useCache)
            DataCache = new(() => File.String);
    }

    ITextProvider ISourceProvider.Data => this;

    string Data => DataCache?.Value ?? File.String ?? "";

    bool ISourceProvider.IsPersistent => true;
    int ISourceProvider.Length => (int)long.Min(File.Size, int.MaxValue);

    string ISourceProvider.Identifier => File.FullName;

    IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => Data.GetEnumerator();
    string ITextProvider.this[Range range] => Data[range];
    char ITextProvider.this[Index index] => Data[index];
}

