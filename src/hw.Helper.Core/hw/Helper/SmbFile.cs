using System.Diagnostics;
using System.Text;
using hw.DebugFormatter;

// ReSharper disable CheckNamespace

namespace hw.Helper;

/// <summary>
///     Lightweight file-and-directory façade over <see cref="System.IO" /> with conveniences for
///     reading text/byte content, recursive enumeration, version info and IDE-clickable position
///     formatting. Construct via <see cref="StringExtender.ToSmbFile" />.
/// </summary>
[Serializable]
[PublicAPI]
public sealed class SmbFile
{
    /// <summary>
    ///     When <c>true</c>, the parent directory is created on demand before any write so that
    ///     callers do not need to set up the path explicitly.
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    [EnableDumpExcept(true)]
    public bool AutoCreateDirectories;

    readonly string InternalName;

    FileSystemInfo? FileInfoCache;
    FileVersionInfo? FileVersionInfoCache;

    /// <summary>
    ///     Full text content of the file. Reading returns <c>null</c> if the file does not exist;
    ///     writing creates or overwrites the file (creating its parent directory when
    ///     <see cref="AutoCreateDirectories" /> is set).
    /// </summary>
    [DisableDump]
    public string? String
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

    /// <summary>Last-write timestamp formatted via <see cref="DateTimeExtender" />'s short form.</summary>
    public string ModifiedDateString => ModifiedDate.DynamicShortFormat(true);

    /// <summary>
    ///     Full binary content of the file. Reading uses the file's reported size; writing
    ///     overwrites and respects <see cref="AutoCreateDirectories" />.
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
            CheckedEnsureDirectoryOfFileExists();
            var f = File.OpenWrite(InternalName);
            f.Write(value, 0, value.Length);
            f.Close();
        }
    }

    /// <summary>Opens the file for read while still allowing concurrent readers and writers.</summary>
    [DisableDump]
    public FileStream Reader
        => new(InternalName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    /// <summary>File size in bytes; only valid when this instance refers to an existing file.</summary>
    [DisableDump]
    public long Size => ((FileInfo)FileSystemInfo).Length;

    /// <summary>Absolute path of the file or directory.</summary>
    public string FullName => FileSystemInfo.FullName;

    /// <summary>The containing directory as another <see cref="SmbFile" />, or <c>null</c> for a root.</summary>
    [DisableDump]
    public SmbFile? Directory => DirectoryName?.ToSmbFile();

    /// <summary>Path of the containing directory, or <c>null</c> for a root.</summary>
    [DisableDump]
    public string? DirectoryName => Path.GetDirectoryName(FullName);

    /// <summary>File extension including the leading dot, or empty when there is none.</summary>
    [DisableDump]
    public string Extension => Path.GetExtension(FullName);

    /// <summary>Name component (file name or directory name) without any path prefix.</summary>
    [DisableDump]
    public string Name => FileSystemInfo.Name;

    /// <summary>Whether the underlying file system entry currently exists.</summary>
    public bool Exists => FileSystemInfo.Exists;

    /// <summary>
    ///     Whether the directory exists and behaves like a mounted reparse point. Returns <c>false</c>
    ///     for ordinary directories and for reparse points whose target is unavailable.
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

    /// <summary>Whether this entry is an existing directory.</summary>
    public bool IsDirectory => System.IO.Directory.Exists(InternalName);

    /// <summary>Newline-separated list of immediate child names; only meaningful for directories.</summary>
    [DisableDump]
    public string DirectoryString => GetDirectoryString();

    /// <summary>Immediate children of the directory as <see cref="SmbFile" /> instances; only meaningful for directories.</summary>
    [DisableDump]
    public SmbFile[] Items => GetItems().Select(f => Create(f.FullName, AutoCreateDirectories)).ToArray();

    /// <summary>
    ///     <c>true</c> when the file cannot currently be opened for read (e.g. another process holds
    ///     an exclusive lock). Implemented by attempting to open the file, so the call is not free.
    /// </summary>
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

    /// <summary>Last-write timestamp of the underlying entry.</summary>
    [DisableDump]
    public DateTime ModifiedDate => FileSystemInfo.LastWriteTime;

    /// <summary>Parsed file version from the executable's metadata, or <c>null</c> if not present.</summary>
    [DisableDump]
    public Version? FileVersion
        => FileVersionInfo.FileVersion == null? null : new(FileVersionInfo.FileVersion);

    /// <summary>Parsed product version from the executable's metadata, or <c>null</c> if not present.</summary>
    [DisableDump]
    public Version? ProductVersion
        => FileVersionInfo.ProductVersion == null? null : new(FileVersionInfo.ProductVersion);

    /// <summary>Cached <see cref="System.Diagnostics.FileVersionInfo" /> for this file.</summary>
    [DisableDump]
    public FileVersionInfo FileVersionInfo
        => FileVersionInfoCache ??= FileVersionInfo.GetVersionInfo(InternalName);

    /// <summary>
    ///     Cached <see cref="System.IO.FileSystemInfo" /> backing the other properties; resolves to
    ///     a <see cref="DirectoryInfo" /> for directories and a <see cref="FileInfo" /> otherwise.
    /// </summary>
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

    /// <summary>Directory containing the source file of the immediate caller.</summary>
    [PublicAPI]
    public static string? SourcePath => GetSourcePath(1);

    /// <summary>Source file path of the immediate caller, as recorded in the PDB.</summary>
    [PublicAPI]
    public static string? SourceFileName => GetSourceFileName(1);

    /// <summary>Source file of the immediate caller as an <see cref="SmbFile" />.</summary>
    [PublicAPI]
    public static SmbFile? SourceFile => GetSourceFile(1);

    /// <summary>Source folder of the immediate caller as an <see cref="SmbFile" />.</summary>
    [PublicAPI]
    public static SmbFile? SourceFolder => GetSourceFolder(1);

    /// <summary>Empty default constructor required for <see cref="SerializableAttribute" /> support.</summary>
    public SmbFile() => InternalName = "";

    SmbFile(string internalName, bool autoCreateDirectories)
    {
        InternalName = internalName;
        AutoCreateDirectories = autoCreateDirectories;
    }

    /// <inheritdoc />
    public override string ToString() => FullName;

    /// <summary>
    ///     Returns the source-file directory of the caller <paramref name="depth" /> frames above
    ///     this call, as recorded in the PDB.
    /// </summary>
    /// <param name="depth"> Number of additional frames to skip. </param>
    [PublicAPI]
    public static string? GetSourcePath(int depth = 0)
    {
        var sourceFileName = GetSourceFileName(depth + 1);
        return sourceFileName == null? null : new FileInfo(sourceFileName).DirectoryName;
    }

    /// <summary>
    ///     Returns the source-file name of the caller <paramref name="depth" /> frames above this
    ///     call.
    /// </summary>
    /// <param name="depth"> Number of additional frames to skip. </param>
    [PublicAPI]
    public static string? GetSourceFileName(int depth = 0)
    {
        var sf = new StackTrace(true).GetFrame(depth + 1);
        return sf?.GetFileName();
    }

    /// <summary>Returns the source file of the caller <paramref name="depth" /> frames above this call.</summary>
    /// <param name="depth"> Number of additional frames to skip. </param>
    [PublicAPI]
    public static SmbFile? GetSourceFile(int depth = 0)
        => GetSourceFileName(depth + 1)?.ToSmbFile();

    /// <summary>Returns the source folder of the caller <paramref name="depth" /> frames above this call.</summary>
    /// <param name="depth"> Number of additional frames to skip. </param>
    [PublicAPI]
    public static SmbFile? GetSourceFolder(int depth = 0)
        => GetSourcePath(depth + 1)?.ToSmbFile();

    /// <summary>
    ///     Lists files matching <paramref name="filePattern" /> (a path with a final wildcard segment,
    ///     e.g. <c>C:\Logs\*.txt</c>).
    /// </summary>
    public static string[] Select(string filePattern)
    {
        var namePattern = filePattern.Split('\\').Last();
        var path = filePattern.Substring(0, filePattern.Length - namePattern.Length - 1);
        return System.IO.Directory.GetFiles(path, namePattern);
    }

    /// <summary>
    ///     Reads at most <paramref name="size" /> bytes starting at <paramref name="start" /> and
    ///     decodes them as UTF-8. Returns <c>null</c> when the file does not exist.
    /// </summary>
    /// <param name="start"> Byte offset to read from. </param>
    /// <param name="size"> Maximum number of bytes to read. </param>
    public string? SubString(long start, int size)
    {
        if(!File.Exists(InternalName))
            return null;

        using var f = Reader;
        f.Position = start;
        var buffer = new byte[size];
        var _ = f.Read(buffer);
        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    ///     Creates the parent directory if <see cref="AutoCreateDirectories" /> is enabled. Otherwise
    ///     does nothing — the caller has opted out of automatic creation.
    /// </summary>
    public void CheckedEnsureDirectoryOfFileExists()
    {
        if(AutoCreateDirectories)
            EnsureDirectoryOfFileExists();
    }

    /// <summary>Unconditionally creates the parent directory of this file if it does not exist.</summary>
    public void EnsureDirectoryOfFileExists()
        => DirectoryName?.ToSmbFile(false).EnsureIsExistentDirectory();

    /// <summary>
    ///     Ensures this path is an existing directory, creating it (and any missing ancestors) when
    ///     necessary. Asserts when the path already exists but is a file.
    /// </summary>
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

    /// <summary>Deletes this entry; for directories, <paramref name="recursive" /> chooses whether non-empty directories are removed too.</summary>
    public void Delete(bool recursive = false)
    {
        if(IsDirectory)
            System.IO.Directory.Delete(InternalName, recursive);
        else
            File.Delete(InternalName);
    }

    /// <summary>Renames or relocates this entry to <paramref name="newName" />.</summary>
    public void Move(string newName)
    {
        if(IsDirectory)
            System.IO.Directory.Move(InternalName, newName);
        else
            File.Move(InternalName, newName);
    }

    /// <summary>
    ///     Copies this entry to <paramref name="destinationPath" />. Directories are copied
    ///     recursively, creating the destination directory as needed.
    /// </summary>
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

    /// <summary>
    ///     Like <see cref="Items" />, but swallows any I/O exception (e.g. access-denied, mount-point
    ///     failure) and returns an empty array instead. Suited for best-effort traversal.
    /// </summary>
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

        return [];
    }

    /// <summary>
    ///     Depth-first enumeration of this entry and (for directories) every descendant. Errors
    ///     traversing a sub-directory are silently skipped, matching <see cref="GuardedItems" />.
    /// </summary>
    public IEnumerable<SmbFile> RecursiveItems()
    {
        yield return this;

        if(!IsDirectory)
            yield break;

        FullName.Log(LogLevel.Trace);
        IEnumerable<string?> filePaths = [FullName];
        while(true)
        {
            var newList = new List<string?>();
            var items =
                filePaths.SelectMany(s => s?.ToSmbFile(AutoCreateDirectories).GuardedItems() ?? [])
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

    /// <summary>Joins <paramref name="item" /> onto this path; equivalent to <see cref="op_Division" />.</summary>
    public SmbFile PathCombine(string item) => FullName.PathCombine(item).ToSmbFile();

    /// <summary>
    ///     Whether <paramref name="subFile" />'s path starts with this path (case-insensitive prefix
    ///     match — does not check that it is actually a sub-entry on disk).
    /// </summary>
    public bool Contains(SmbFile subFile) => subFile.FullName.StartsWith(FullName, true, null);

    /// <summary>Launches the file with its associated handler in a hidden window.</summary>
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
        => ((DirectoryInfo)FileSystemInfo).GetFileSystemInfos();

    /// <summary>
    ///     Renders an IDE-clickable Visual Studio location for <paramref name="textPart" /> inside
    ///     this file, tagged with <paramref name="tag" />.
    /// </summary>
    public string FilePosition
    (
        TextPart? textPart,
        FilePositionTag tag
    )
        => Tracer.FilePosition(FullName, textPart, tag);

    /// <inheritdoc cref="FilePosition(TextPart?, FilePositionTag)" />
    public string FilePosition
    (
        TextPart textPart,
        string tag
    )
        => Tracer.FilePosition(FullName, textPart, tag);

    /// <summary>Path-join operator; <c>file / "sub.txt"</c> is shorthand for <c>file.PathCombine("sub.txt")</c>.</summary>
    public static SmbFile operator /(SmbFile file, string tail) => file.PathCombine(tail);
}