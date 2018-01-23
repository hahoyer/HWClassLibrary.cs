using hw.Helper;

namespace hw.Scanner
{
    public sealed class FileSourceProvider : ISourceProvider
    {
        readonly ValueCache<string> DataCache;
        readonly SmbFile File;

        public FileSourceProvider(SmbFile file, bool useCache = true)
        {
            File = file;
            if(useCache)
                DataCache = new ValueCache<string>(() => File.String);
        }

        string ISourceProvider.Data => DataCache?.Value ?? File.String;
        bool ISourceProvider.IsPersistent => false;
    }
}