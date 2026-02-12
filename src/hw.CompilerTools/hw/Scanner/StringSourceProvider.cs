// ReSharper disable CheckNamespace

using System.Collections;

namespace hw.Scanner;

public sealed class StringSourceProvider(string data) : ISourceProvider, ITextProvider
{
    ITextProvider ISourceProvider.Data => this;
    string? ISourceProvider.Identifier => null;
    bool ISourceProvider.IsPersistent => false;
    int ISourceProvider.Length => data.Length;
    IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => data.GetEnumerator();
    string ITextProvider.this[Range range] => data[range];
    char ITextProvider.this[Index index] => data[index];
}