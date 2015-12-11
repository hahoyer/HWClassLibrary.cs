using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using hw.DebugFormatter;

namespace hw.Helper
{
    /// <summary>
    ///     Summary description for File.
    /// </summary>
    [Serializable]
    public sealed class File
    {
        readonly string _name;

        Uri _uriCache;

        public Uri Uri { get { return _uriCache ?? (_uriCache = new Uri(_name)); } }

        public bool IsFTP { get { return Uri.Scheme == Uri.UriSchemeFtp; } }

        /// <summary>
        ///     constructs a FileInfo
        /// </summary>
        /// <param name="name"> the filename </param>
        internal static File Create(string name) { return new File(name); }

        File(string name) { _name = name; }

        public File() { _name = ""; }

        /// <summary>
        ///     considers the file as a string. If file existe it should be a text file
        /// </summary>
        /// <value> the content of the file if existing else null. </value>
        public string String
        {
            get
            {
                if(System.IO.File.Exists(_name))
                {
                    var f = System.IO.File.OpenText(_name);
                    var result = f.ReadToEnd();
                    f.Close();
                    return result;
                }

                try
                {
                    if(Uri.Scheme == Uri.UriSchemeHttp)
                        return StringFromHTTP;
                }
                catch
                {}
                return null;
            }
            set
            {
                var f = System.IO.File.CreateText(_name);
                f.Write(value);
                f.Close();
            }
        }

        public void AssumeDirectoryOfFileExists()
        {
            var dn = DirectoryName;
            if(dn == null || dn.FileHandle().Exists)
                return;
            Directory.CreateDirectory(dn);
        }

        string StringFromHTTP
        {
            get
            {
                var req = WebRequest.Create(Uri.AbsoluteUri);
                var resp = req.GetResponse();
                var stream = resp.GetResponseStream();
                Tracer.Assert(stream != null);
                var streamReader = new StreamReader(stream);
                var result = streamReader.ReadToEnd();
                return result;
            }
        }

        public override string ToString() { return FullName; }

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

        public FileStream Reader { get { return System.IO.File.OpenRead(_name); } }

        /// <summary>
        ///     Size of file in bytes
        /// </summary>
        public long Size { get { return ((FileInfo) FileSystemInfo).Length; } }

        /// <summary>
        ///     Gets the full path of the directory or file.
        /// </summary>
        public string FullName { get { return FileSystemInfo.FullName; } }
        public string DirectoryName { get { return Path.GetDirectoryName(FullName); } }
        public string Extension { get { return Path.GetExtension(FullName); } }

        /// <summary>
        ///     Gets the name of the directory or file without path.
        /// </summary>
        public string Name { get { return FileSystemInfo.Name; } }

        /// <summary>
        ///     Gets a value indicating whether a file exists.
        /// </summary>
        public bool Exists { get { return FileSystemInfo.Exists; } }

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
        public bool IsDirectory { get { return Directory.Exists(_name); } }

        FileSystemInfo _fileInfoCache;

        FileSystemInfo FileSystemInfo
        {
            get
            {
                if(_fileInfoCache == null)
                    if(IsDirectory)
                        _fileInfoCache = new DirectoryInfo(_name);
                    else
                        _fileInfoCache = new FileInfo(_name);
                return _fileInfoCache;
            }
        }

        /// <summary>
        ///     Content of directory, one line for each file
        /// </summary>
        public string DirectoryString { get { return GetDirectoryString(); } }

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

        FileSystemInfo[] GetItems() { return ((DirectoryInfo) FileSystemInfo).GetFileSystemInfos().ToArray(); }
        public File[] Items { get { return GetItems().Select(f => Create(f.FullName)).ToArray(); } }

        /// <summary>
        ///     Gets the directory of the source file that called this function
        /// </summary>
        /// <param name="depth"> The depth. </param>
        /// <returns> </returns>
        public static string SourcePath(int depth) { return new FileInfo(SourceFileName(depth + 1)).DirectoryName; }

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
                .GetFiles(filePattern.Substring(0, filePattern.Length - namePattern.Length - 1), namePattern);
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
    }
}