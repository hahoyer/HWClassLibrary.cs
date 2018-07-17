using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using hw.DebugFormatter;

namespace hw.Helper
{
    [Serializable]
    public sealed class SmbFile
    {
        /// <summary>
        ///     constructs a FileInfo
        /// </summary>
        /// <param name="name"> the filename </param>
        /// <param name="autoCreateDirectories"></param>
        internal static SmbFile Create(string name, bool autoCreateDirectories)
            => new SmbFile(name, autoCreateDirectories);

        /// <summary>
        ///     Gets the directory of the source file that called this function
        /// </summary>
        /// <param name="depth"> The depth. </param>
        /// <returns> </returns>
        public static string SourcePath(int depth)
            => new FileInfo(SourceFileName(depth + 1)).DirectoryName;

        /// <summary>
        ///     Gets the name of the source file that called this function
        /// </summary>
        /// <param name="depth"> stack depths of the function used. </param>
        /// <returns> </returns>
        public static string SourceFileName(int depth)
        {
            var sf = new StackTrace(true).GetFrame(depth + 1);
            return sf.GetFileName();
        }

        /// <summary>
        ///     Gets list of files that match given path and pattern
        /// </summary>
        /// <param name="filePattern"></param>
        /// <returns></returns>
        public static string[] Select(string filePattern)
        {
            var namePattern = filePattern.Split('\\').Last();
            var path = filePattern.Substring(0, filePattern.Length - namePattern.Length - 1);
            return System.IO.Directory.GetFiles(path, namePattern);
        }

        /// <summary>
        ///     Ensure, that all directories are existent when writing to file.
        ///     Can be modified at any time.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        [EnableDumpExcept(true)]
        public bool AutoCreateDirectories;

        readonly string _name;
        FileSystemInfo _fileInfoCache;

        SmbFile(string name, bool autoCreateDirectories)
        {
            _name = name;
            AutoCreateDirectories = autoCreateDirectories;
        }

        public SmbFile() => _name = "";

        /// <summary>
        ///     considers the file as a string. If file existe it should be a text file
        /// </summary>
        /// <value> the content of the file if existing else null. </value>
        [DisableDump]
        public string String
        {
            get
            {
                if(!File.Exists(_name))
                    return null;

                using(var f = File.OpenText(_name))
                    return f.ReadToEnd();
            }
            set
            {
                CheckedEnsureDirectoryOfFileExists();
                using(var f = File.CreateText(_name))
                    f.Write(value);
            }
        }

        public string ModifiedDateString => ModifiedDate.DynamicShortFormat(true);

        /// <summary>
        ///     considers the file as a byte array
        /// </summary>
        [DisableDump]
        public byte[] Bytes
        {
            get
            {
                var f = Reader;
                var result = new byte[Size];
                f.Read(result, 0, (int) Size);
                f.Close();
                return result;
            }
            set
            {
                var f = File.OpenWrite(_name);
                f.Write(value, 0, value.Length);
                f.Close();
            }
        }

        [DisableDump]
        public FileStream Reader
            => new FileStream(_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        /// <summary>
        ///     Size of file in bytes
        /// </summary>
        [DisableDump]
        public long Size => ((FileInfo) FileSystemInfo).Length;

        /// <summary>
        ///     Gets the full path of the directory or file.
        /// </summary>
        public string FullName => FileSystemInfo.FullName;

        [DisableDump]
        public SmbFile Directory => DirectoryName.ToSmbFile();

        [DisableDump]
        public string DirectoryName => Path.GetDirectoryName(FullName);

        [DisableDump]
        public string Extension => Path.GetExtension(FullName);

        /// <summary>
        ///     Gets the name of the directory or file without path.
        /// </summary>
        [DisableDump]
        public string Name => FileSystemInfo.Name;

        /// <summary>
        ///     Gets a value indicating whether a file exists.
        /// </summary>
        public bool Exists => FileSystemInfo.Exists;

        /// <summary>
        ///     Gets a value indicating whether a file exists.
        /// </summary>
        [DisableDump]
        public bool IsMounted
        {
            get
            {
                if(!Exists)
                    return false;
                if((FileSystemInfo.Attributes & FileAttributes.ReparsePoint) == 0)
                    return false;

                try
                {
                    ((DirectoryInfo) FileSystemInfo).GetFileSystemInfos("dummy");
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     returns true if it is a directory
        /// </summary>
        public bool IsDirectory => System.IO.Directory.Exists(_name);

        FileSystemInfo FileSystemInfo
        {
            get
            {
                if(_fileInfoCache != null)
                    return _fileInfoCache;

                _fileInfoCache = IsDirectory
                    ? (FileSystemInfo) new DirectoryInfo(_name)
                    : new FileInfo(_name);

                return _fileInfoCache;
            }
        }

        /// <summary>
        ///     Content of directory, one line for each file
        /// </summary>
        [DisableDump]
        public string DirectoryString => GetDirectoryString();

        [DisableDump]
        public SmbFile[] Items
        {
            get {return GetItems().Select(f => Create(f.FullName, AutoCreateDirectories)).ToArray();}
        }

        [EnableDumpExcept(false)]
        public bool IsLocked
        {
            get
            {
                try
                {
                    File.OpenRead(_name).Close();
                    return false;
                }
                catch(IOException)
                {
                    return true;
                }

                //file is not locked
            }
        }

        [DisableDump]
        public DateTime ModifiedDate => FileSystemInfo.LastWriteTime;

        public string SubString(long start, int size)
        {
            if(!File.Exists(_name))
                return null;

            using(var f = Reader)
            {
                f.Position = start;
                var buffer = new byte[size];
                f.Read(buffer, 0, size);
                return Encoding.UTF8.GetString(buffer);
            }
        }

        public void CheckedEnsureDirectoryOfFileExists()
        {
            if(AutoCreateDirectories)
                EnsureDirectoryOfFileExists();
        }

        public void EnsureDirectoryOfFileExists()
            => DirectoryName?.ToSmbFile(false).EnsureIsExistentDirectory();

        public void EnsureIsExistentDirectory()
        {
            if(Exists)
                Tracer.Assert(IsDirectory);
            else
            {
                EnsureDirectoryOfFileExists();
                System.IO.Directory.CreateDirectory(FullName);
                _fileInfoCache = null;
            }
        }

        public override string ToString() => FullName;

        /// <summary>
        ///     Delete the file
        /// </summary>
        public void Delete(bool recursive = false)
        {
            if(IsDirectory)
                System.IO.Directory.Delete(_name, recursive);
            else
                File.Delete(_name);
        }

        /// <summary>
        ///     Move the file
        /// </summary>
        public void Move(string newName)
        {
            if(IsDirectory)
                System.IO.Directory.Move(_name, newName);
            else
                File.Move(_name, newName);
        }

        string GetDirectoryString()
        {
            var result = "";
            foreach(var fi in GetItems())
            {
                result += fi.Name;
                result += "\n";
            }

            return result;
        }

        FileSystemInfo[] GetItems()
            => ((DirectoryInfo) FileSystemInfo).GetFileSystemInfos().ToArray();

        public void CopyTo(string destinationPath)
        {
            if(IsDirectory)
            {
                destinationPath.ToSmbFile().EnsureIsExistentDirectory();
                foreach(var sourceSubFile in Items)
                {
                    var destinationSubPath = destinationPath.PathCombine(sourceSubFile.Name);
                    sourceSubFile.CopyTo(destinationSubPath);
                }
            }
            else
                File.Copy(FullName, destinationPath);
        }

        public SmbFile[] GuardedItems()
        {
            try
            {
                if(IsDirectory)
                    return Items;
            }
            catch
            {
                // ignored
            }

            return new SmbFile[0];
        }

        public IEnumerable<SmbFile> RecursiveItems()
        {
            yield return this;

            if(!IsDirectory)
                yield break;

            Tracer.Line(FullName);
            IEnumerable<string> filePaths = new[] {FullName};
            while(true)
            {
                var newList = new List<string>();
                var items =
                    filePaths.SelectMany(s => s.ToSmbFile(AutoCreateDirectories).GuardedItems())
                        .ToArray();
                foreach(var item in items)
                {
                    yield return item;

                    if(item.IsDirectory)
                        newList.Add(item.FullName);
                }

                if(!newList.Any())
                    yield break;

                filePaths = newList;
            }
        }

        public SmbFile PathCombine(string item) => FullName.PathCombine(item).ToSmbFile();

        public bool Contains(SmbFile subFile) => subFile.FullName.StartsWith(FullName, true, null);

        public void InitiateExternalProgram()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = FullName
                }
            };
            process.Start();
        }
    }
}