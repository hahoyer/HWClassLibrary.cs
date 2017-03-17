using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using hw.DebugFormatter;

namespace hw.Helper
{
    /// <summary>
    ///     Summary description for File.
    /// </summary>
    [Serializable]
    public sealed class SmbFile
    {
        readonly string _name;
        FileSystemInfo _fileInfoCache;

        /// <summary>
        ///     Ensure, that all directories are existent when writing to file.
        ///     Can be modifierd at any time.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool AutoCreateDirectories;

        /// <summary>
        ///     constructs a FileInfo
        /// </summary>
        /// <param name="name"> the filename </param>
        /// <param name="autoCreateDirectories"></param>
        internal static SmbFile Create(string name, bool autoCreateDirectories)
            => new SmbFile(name, autoCreateDirectories);

        SmbFile(string name, bool autoCreateDirectories)
        {
            _name = name;
            AutoCreateDirectories = autoCreateDirectories;
        }

        public SmbFile() { _name = ""; }

        /// <summary>
        ///     considers the file as a string. If file existe it should be a text file
        /// </summary>
        /// <value> the content of the file if existing else null. </value>
        public string String
        {
            get
            {
                if(!System.IO.File.Exists(_name))
                    return null;

                using(var f = System.IO.File.OpenText(_name))
                    return f.ReadToEnd();
            }
            set
            {
                CheckedEnsureDirectoryOfFileExists();
                using (var f = System.IO.File.CreateText(_name))
                    f.Write(value);
            }
        }

        public void CheckedEnsureDirectoryOfFileExists()
        {
            if(AutoCreateDirectories)
                EnsureDirectoryOfFileExists();
        }

        public void EnsureDirectoryOfFileExists()
            => DirectoryName?.FileHandle().EnsureIsExistentDirectory();

        public void EnsureIsExistentDirectory()
        {
            if(Exists)
                Tracer.Assert(IsDirectory);
            else
            {
                EnsureDirectoryOfFileExists();
                Directory.CreateDirectory(FullName);
            }
        }

        public override string ToString() => FullName;

        /// <summary>
        ///     considers the file as a byte array
        /// </summary>
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
                var f = System.IO.File.OpenWrite(_name);
                f.Write(value, 0, value.Length);
                f.Close();
            }
        }

        public FileStream Reader => System.IO.File.OpenRead(_name);

        /// <summary>
        ///     Size of file in bytes
        /// </summary>
        public long Size => ((FileInfo) FileSystemInfo).Length;

        /// <summary>
        ///     Gets the full path of the directory or file.
        /// </summary>
        public string FullName => FileSystemInfo.FullName;

        public string DirectoryName => Path.GetDirectoryName(FullName);
        public string Extension => Path.GetExtension(FullName);

        /// <summary>
        ///     Gets the name of the directory or file without path.
        /// </summary>
        public string Name => FileSystemInfo.Name;

        /// <summary>
        ///     Gets a value indicating whether a file exists.
        /// </summary>
        public bool Exists => FileSystemInfo.Exists;

        /// <summary>
        ///     Gets a value indicating whether a file exists.
        /// </summary>
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
        ///     Delete the file
        /// </summary>
        public void Delete(bool recursive = false)
        {
            if(IsDirectory)
                Directory.Delete(_name, recursive);
            else
                System.IO.File.Delete(_name);
        }

        /// <summary>
        ///     Move the file
        /// </summary>
        public void Move(string newName)
        {
            if(IsDirectory)
                Directory.Move(_name, newName);
            else
                System.IO.File.Move(_name, newName);
        }

        /// <summary>
        ///     returns true if it is a directory
        /// </summary>
        public bool IsDirectory => Directory.Exists(_name);

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
        public string DirectoryString => GetDirectoryString();

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

        public SmbFile[] Items
        {
            get
            {
                return GetItems().Select(f => Create(f.FullName, AutoCreateDirectories)).ToArray();
            }
        }

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
            return Directory
                .GetFiles
                (filePattern.Substring(0, filePattern.Length - namePattern.Length - 1), namePattern);
        }

        public bool IsLocked
        {
            get
            {
                try
                {
                    System.IO.File.OpenRead(_name).Close();
                    return false;
                }
                catch(IOException)
                {
                    return true;
                }

                //file is not locked
            }
        }

        public DateTime ModifiedDate => FileSystemInfo.LastWriteTime;

        public void CopyTo(string destinationPath)
        {
            if(IsDirectory)
            {
                destinationPath.FileHandle().EnsureIsExistentDirectory();
                foreach(var sourceSubFile in Items)
                {
                    var destinationSubPath = destinationPath.PathCombine(sourceSubFile.Name);
                    sourceSubFile.CopyTo(destinationSubPath);
                }
            }
            else
                System.IO.File.Copy(FullName, destinationPath);
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
    }
}