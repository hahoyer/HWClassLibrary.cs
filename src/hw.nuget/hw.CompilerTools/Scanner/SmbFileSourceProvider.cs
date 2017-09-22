using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace hw.Scanner
{
    public sealed class SmbFileSourceProvider : ISourceProvider
    {
        readonly ValueCache<string> DataCache;
        readonly SmbFile File;

        public SmbFileSourceProvider(SmbFile file, bool useCache = true)
        {
            File = file;
            if(useCache)
                DataCache = new ValueCache<string>(() => File.String);
        }

        string ISourceProvider.Data => DataCache?.Value ?? File.String;
        bool ISourceProvider.IsPersistent => false;
    }
}