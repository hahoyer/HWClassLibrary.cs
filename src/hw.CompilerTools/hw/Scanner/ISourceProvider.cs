// ReSharper disable CheckNamespace

namespace hw.Scanner;

public interface ISourceProvider
{
    string? Data { get; }
    bool IsPersistent { get; }
    int Length { get; }
    string? Identifier { get; }
}

public interface IMultiSourceProvider
{
    SourcePosition Position(int position, bool isEnd);
}
