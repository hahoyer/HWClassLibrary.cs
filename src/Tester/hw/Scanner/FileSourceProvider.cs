using hw.Helper;

// ReSharper disable CheckNamespace

namespace hw.Scanner;

public sealed class FileSourceProvider : ISourceProvider
{
    readonly ValueCache<string> DataCache;
    readonly SmbFile File;

    public FileSourceProvider(SmbFile file, bool useCache = true)
    {
        File = file;
        if(useCache)
            DataCache = new(() => File.String);
    }

    string ISourceProvider.Data => DataCache?.Value ?? File.String;
    bool ISourceProvider.IsPersistent => false;
}