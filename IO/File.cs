using System;
using System.Diagnostics;
using System.IO;

namespace HWClassLibrary.IO
{
    /// <summary>
    /// Summary description for File.
    /// </summary>
    public class File
    {
        private string _name;

        private Uri _uriCache = null;
        
        Uri Uri
        {
            get
            {
                if(_uriCache == null)
                    _uriCache = new Uri(_name);   
                return _uriCache;
            }
        }

        bool IsFTP {get{return Uri.Scheme == Uri.UriSchemeFtp;}}

        /// <summary>
        /// constructs a FileInfo
        /// </summary>
        /// <param name="name">the filename</param>
        public static File m(string name)
        {
            return new File(name);
        }

        private File(string name)
        {
            _name = name;
        }

        /// <summary>
        /// considers the file as a string. If file existe it should be a text file
        /// </summary>
        /// <value>the content of the file if existing else null.</value>
        public string String
        {
            get
            {
                if (!System.IO.File.Exists(_name))
                    return null;
                StreamReader f = System.IO.File.OpenText(_name);
                string result = f.ReadToEnd();
                f.Close();
                return result;
            }
            set
            {
                StreamWriter f = System.IO.File.CreateText(_name);
                f.Write(value);
                f.Close();
            }
        }

        /// <summary>
        /// considers the file as a byte array
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                FileStream f = System.IO.File.OpenRead(_name);
                byte[] result = new byte[Size];
                f.Read(result, 0, (int) Size);
                f.Close();
                return result;
            }
            set
            {
                FileStream f = System.IO.File.OpenWrite(_name);
                f.Write(value, 0, value.Length);
                f.Close();
            }
        }

        /// <summary>
        /// Size of file in bytes
        /// </summary>
        public long Size { get { return ((FileInfo)FileSystemInfo).Length; } }

        /// <summary>
        /// Gets the full path of the directory or file.
        /// </summary>
        public string FullName { get { return FileSystemInfo.FullName; } }

        /// <summary>
        /// Gets a value indicating whether a file exists.
        /// </summary>
        public bool Exists { get { return FileSystemInfo.Exists; } }

        /// <summary>
        /// Delete the file
        /// </summary>
        public void Delete()
        {
            System.IO.File.Delete(_name);
        }

        /// <summary>
        /// returns true if it is a directory
        /// </summary>
        public bool IsDirectory{get{return System.IO.Directory.Exists(_name);}}

        private FileSystemInfo _fileInfoCache = null;

        FileSystemInfo FileSystemInfo
        {
            get
            {
                if (_fileInfoCache == null)
                {
                    if (IsDirectory)
                        _fileInfoCache = new DirectoryInfo(_name);
                    else
                        _fileInfoCache = new FileInfo(_name);
                }
                return _fileInfoCache;
            }
        }
        /// <summary>
        /// Content of directory, one line for each file
        /// </summary>
        public string Directory { get { return GetDirectoryString(); } }

        private string GetDirectoryString()
        {
            string result = "";
            foreach (FileInfo fi in ((DirectoryInfo)FileSystemInfo).GetFiles())
            {
                result += fi.Name;
                result += "\n";
            }
            return result;
        }

        /// <summary>
        /// Gets the directory of the source file that called this function
        /// </summary>
        /// <param name="depth">The depth.</param>
        /// <returns></returns>
        public static string SourcePath(int depth)
        {
            return new FileInfo(SourceFileName(depth + 1)).DirectoryName;
        }

        /// <summary>
        /// Gets the name of the source file that called this function
        /// </summary>
        /// <param name="depth">stack depths of the function used.</param>
        /// <returns></returns>
        public static string SourceFileName(int depth)
        {
            StackFrame sf = new StackTrace(true).GetFrame(depth + 1);
            return sf.GetFileName();
        }
    }
}