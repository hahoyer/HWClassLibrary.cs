using System.Collections;
using hw.Helper;

namespace hw.Scanner64;

public sealed class StringSourceProvider(string data) : ISourceProvider, ITextProvider
{
    ITextProvider ISourceProvider.Data => this;
    string? ISourceProvider.Identifier => null;
    bool ISourceProvider.IsPersistent => false;
    long ISourceProvider.Length => data.Length;
    IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => data.GetEnumerator();
    string ITextProvider.this[Range64 range] => data[range.ToInt];
    char ITextProvider.this[Index64 index] => data[index.ToInt];
}