// ReSharper disable CheckNamespace

namespace hw.Scanner;

public interface ISourceProvider
{
    ITextProvider? Data { get; }
    bool IsPersistent { get; }
    int Length { get; }
    string? Identifier { get; }
}

public interface IMultiSourceProvider
{
    SourcePosition Position(int position, bool isEnd);
}

public interface ITextProvider: IEnumerable<char>
{
    string this[Range range] { get; }
    char this[Index index] { get; }
}

