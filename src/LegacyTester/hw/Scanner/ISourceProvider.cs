// ReSharper disable CheckNamespace

namespace hw.Scanner;

public interface ISourceProvider
{
    string Data { get; }
    bool IsPersistent { get; }
}