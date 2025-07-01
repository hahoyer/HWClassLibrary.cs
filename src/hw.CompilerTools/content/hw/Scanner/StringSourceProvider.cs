

// ReSharper disable CheckNamespace

namespace hw.Scanner;

public sealed class StringSourceProvider(string data) : ISourceProvider
{
    string ISourceProvider.Data => data;
    bool ISourceProvider.IsPersistent => false;
    int ISourceProvider.Length => data.Length;
    string? ISourceProvider.Identifier => null;
}