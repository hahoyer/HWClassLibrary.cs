using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace hw.Scanner
{
    public sealed class FileSourceProvider : ISourceProvider
    {
        readonly ValueCache<string> DataCache;
        readonly File File;

        public FileSourceProvider(File file, bool useCache = true)
        {
            File = file;
            if(useCache)
                DataCache = new ValueCache<string>(() => File.String);
        }

        string ISourceProvider.Data => DataCache?.Value ?? File.String;
        bool ISourceProvider.IsPersistent => false;
    }
}