#pragma warning disable IDE0130
namespace hw.Helper;

/// <summary>
///     Random-access view of a file as a virtual string of UTF-8 characters with a two-block LRU
///     read cache. Indexing reads only the block(s) covering the requested position, so files much
///     larger than memory can be sampled efficiently.
/// </summary>
[PublicAPI]
public class SmbFileBuffer
{
    /// <summary>The file being read.</summary>
    public readonly SmbFile Parent;

    long[] IndexValue;
    int BlockSizeValue = 100_000_000;
    ValueCache<string>[] BufferCache;
    int CurrentBufferIndex;

    /// <summary>
    ///     Size of one cache block in bytes. Setting a new size invalidates both cached blocks.
    ///     Default is 100 MB.
    /// </summary>
    public int BlockSize
    {
        get => BlockSizeValue;
        set
        {
            if(BlockSizeValue == value)
                return;
            BlockSizeValue = value;
            BufferCache[0].IsValid = false;
            BufferCache[1].IsValid = false;
        }
    }

    long Index
    {
        get => IndexValue[CurrentBufferIndex];
        set
        {
            if(IndexValue[CurrentBufferIndex] == value)
                return;

            CurrentBufferIndex = 1 - CurrentBufferIndex;
            if(IndexValue[CurrentBufferIndex] == value)
                return;

            IndexValue[CurrentBufferIndex] = value;
            BufferCache[CurrentBufferIndex].IsValid = false;
        }
    }

    string Buffer => BufferCache[CurrentBufferIndex].Value;

    /// <summary>Reads the slice of the file covered by <paramref name="position" /> as UTF-8 text.</summary>
    public string this[Range64 position]
    {
        get
        {
            var start = position.Start.GetOffset(Parent.Size);
            Index = start / BlockSizeValue;
            var positionStart = (int)(start % BlockSizeValue);

            var end = position.End.GetOffset(Parent.Size);
            var indexEnd = end / BlockSizeValue;
            var positionEnd = (int)(end % BlockSizeValue);
            var result = "";
            while(Index < indexEnd)
            {
                result += Buffer[positionStart..];
                Index++;
                positionStart = 0;
            }

            return result + Buffer[positionStart..positionEnd];
        }
    }

    /// <summary>Reads the single character at <paramref name="position" />.</summary>
    public char this[Index64 position]
    {
        get
        {
            Index = position.Value / BlockSizeValue;
            return Buffer[(int)(position.Value % BlockSizeValue)];
        }
    }

    /// <summary>Reads the single character at byte offset <paramref name="position" />.</summary>
    public char this[long position]
    {
        get
        {
            Index = position / BlockSizeValue;
            return Buffer[(int)(position % BlockSizeValue)];
        }
    }

    /// <param name="parent"> The file to read from. </param>
    /// <param name="blockSize"> Optional override for the cache block size; defaults to 100 MB. </param>
    public SmbFileBuffer(SmbFile parent, int? blockSize = null)
    {
        Parent = parent;
        IndexValue = new long[2];
        BufferCache = new ValueCache<string>[2];
        for (var i = 0; i < 2; i++)
        {
            var index = i;
            BufferCache[i] = new(() => Parent.SubString(BlockSize * IndexValue[index], BlockSize) ?? "");
        }   
        
        if(blockSize != null)
            BlockSizeValue = blockSize.Value;
    }
}