// ReSharper disable CheckNamespace

using hw.Helper;

namespace hw.Scanner64;

public interface ISourceProvider
{
    ITextProvider Data { get; }
    bool IsPersistent { get; }
    long Length { get; }
    string? Identifier { get; }
}

public interface IMultiSourceProvider
{
    SourcePosition Position(long position, bool isEnd);
}

public interface ITextProvider: IEnumerable<char>
{
    string this[Range64 range] { get; }
    char this[Index64 index] { get; }
}

