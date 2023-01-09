using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Helper;

[Serializable]
[PublicAPI]
public sealed class SmbFile
{
    /// <summary>
    ///     Ensure, that all directories are existent when writing to file.
    ///     Can be modified at any time.
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    [EnableDumpExcept(true)]
    public bool AutoCreateDirectories;

    readonly string InternalName;

    FileSystemInfo FileInfoCache;

    /// <summary>
    ///     considers the file as a string. If file exists it should be a text file
    /// </summary>
    /// <value> the content of the file if existing else null. </value>
    [DisableDump]
    public string String
    {
        get
        {
            if(!File.Exists(InternalName))
                return null;

            using var reader = File.OpenText(InternalName);
            return reader.ReadToEnd();
        }
        set
        {
            CheckedEnsureDirectoryOfFileExists();
            using var writer = File.CreateText(InternalName);
            writer.Write(value);
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
            var actualSize = f.Read(result, 0, (int)Size);
            (actualSize == Size).Assert();
            f.Close();
            return result;
        }
        set
        {
            var f = File.OpenWrite(InternalName);
            f.Write(value, 0, value.Length);
            f.Close();
        }
    }

    [DisableDump]
    public FileStream Reader
        => new(InternalName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    /// <summary>
    ///     Size of file in bytes
    /// </summary>
    [DisableDump]
    public long Size => ((FileInfo)FileSystemInfo).Length;

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
                ((DirectoryInfo)FileSystemInfo).GetFileSystemInfos("dummy");
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
    public bool IsDirectory => System.IO.Directory.Exists(InternalName);

    /// <summary>
    ///     Content of directory, one line for each file
    /// </summary>
    [DisableDump]
    public string DirectoryString => GetDirectoryString();

    [DisableDump]
    public SmbFile[] Items => GetItems().Select(f => Create(f.FullName, AutoCreateDirectories)).ToArray();

    [EnableDumpExcept(false)]
    public bool IsLocked
    {
        get
        {
            try
            {
                File.OpenRead(InternalName).Close();
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

    [DisableDump]
    public FileSystemInfo FileSystemInfo
    {
        get
        {
            if(FileInfoCache != null)
                return FileInfoCache;

            FileInfoCache = IsDirectory
                ? new DirectoryInfo(InternalName)
                : new FileInfo(InternalName);

            return FileInfoCache;
        }
    }

    /// <summary>
    ///     Gets the directory of the source file that called this function
    /// </summary>
    /// <returns> </returns>
    [PublicAPI]
    public static string SourcePath => GetSourcePath(1);

    /// <summary>
    ///     Gets the name of the source file that called this function
    /// </summary>
    /// <returns> </returns>
    [PublicAPI]
    public static string SourceFileName => GetSourceFileName(1);

    /// <summary>
    ///     Gets the source file that called this function
    /// </summary>
    [PublicAPI]
    public static SmbFile SourceFile => GetSourceFile(1);

    /// <summary>
    ///     Gets the folder of the source file that called this function
    /// </summary>
    [PublicAPI]
    public static SmbFile SourceFolder => GetSourceFolder(1);

    public SmbFile() => InternalName = "";

    SmbFile(string internalName, bool autoCreateDirectories)
    {
        InternalName = internalName;
        AutoCreateDirectories = autoCreateDirectories;
    }

    public override string ToString() => FullName;

    /// <summary>
    ///     Gets the directory of the source file that called this function
    /// </summary>
    /// <param name="depth"> The depth. </param>
    /// <returns> </returns>
    [PublicAPI]
    public static string GetSourcePath(int depth = 0)
    {
        var sourceFileName = GetSourceFileName(depth + 1);
        return sourceFileName == null? null : new FileInfo(sourceFileName).DirectoryName;
    }

    /// <summary>
    ///     Gets the name of the source file that called this function
    /// </summary>
    /// <param name="depth"> stack depths of the function used. </param>
    /// <returns> </returns>
    [PublicAPI]
    public static string GetSourceFileName(int depth = 0)
    {
        var sf = new StackTrace(true).GetFrame(depth + 1);
        return sf.GetFileName();
    }

    /// <summary>
    ///     Gets the source file that called this function
    /// </summary>
    /// <param name="depth"> stack depths of the function used. </param>
    /// <returns> </returns>
    [PublicAPI]
    public static SmbFile GetSourceFile(int depth = 0)
        => GetSourceFileName(depth + 1)?.ToSmbFile();

    /// <summary>
    ///     Gets the folder of the source file that called this function
    /// </summary>
    /// <param name="depth"> stack depths of the function used. </param>
    /// <returns> </returns>
    [PublicAPI]
    public static SmbFile GetSourceFolder(int depth = 0)
        => GetSourcePath(depth + 1)?.ToSmbFile();

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

    public string SubString(long start, int size)
    {
        if(!File.Exists(InternalName))
            return null;

        using var f = Reader;
        f.Position = start;
        var buffer = new byte[size];
        var actualSize = f.Read(buffer, 0, size);
        (actualSize == size).Assert();
        return Encoding.UTF8.GetString(buffer);
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
            IsDirectory.Assert();
        else
        {
            EnsureDirectoryOfFileExists();
            System.IO.Directory.CreateDirectory(FullName);
            FileInfoCache = null;
        }
    }

    /// <summary>
    ///     Delete the file
    /// </summary>
    public void Delete(bool recursive = false)
    {
        if(IsDirectory)
            System.IO.Directory.Delete(InternalName, recursive);
        else
            File.Delete(InternalName);
    }

    /// <summary>
    ///     Move the file
    /// </summary>
    public void Move(string newName)
    {
        if(IsDirectory)
            System.IO.Directory.Move(InternalName, newName);
        else
            File.Move(InternalName, newName);
    }

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

        FullName.Log();
        IEnumerable<string> filePaths = new[] { FullName };
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
            StartInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden, FileName = FullName
            }
        };
        process.Start();
    }

    /// <summary>
    ///     constructs a FileInfo
    /// </summary>
    /// <param name="name"> the filename </param>
    /// <param name="autoCreateDirectories"></param>
    internal static SmbFile Create(string name, bool autoCreateDirectories)
        => new(name, autoCreateDirectories);

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
        => ((DirectoryInfo)FileSystemInfo).GetFileSystemInfos().ToArray();

    public string FilePosition
    (
        TextPart textPart,
        FilePositionTag tag
    )
        => Tracer.FilePosition(FullName, textPart, tag);

    public string FilePosition
    (
        TextPart textPart,
        string tag
    )
        => Tracer.FilePosition(FullName, textPart, tag);

    public static SmbFile operator /(SmbFile file, string tail) => file.PathCombine(tail);
}