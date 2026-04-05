namespace hw.Helper;

[PublicAPI]
public class SmbFileBuffer
{
    public readonly SmbFile Parent;
    long IndexValue;
    int BlockSizeValue = 100_000_000;
    ValueCache<string> BufferCache;

    public int BlockSize
    {
        get => BlockSizeValue;
        set
        {
            if(BlockSizeValue == value)
                return;
            BlockSizeValue = value;
            BufferCache.IsValid = false;
        }
    }

    long Index
    {
        set
        {
            if(IndexValue == value)
                return;
            IndexValue = value;
            BufferCache.IsValid = false;
        }
    }

    public string Buffer => BufferCache.Value;

    public string this[Range64 position]
    {
        get
        {
            Index = position.Start.Value / BlockSizeValue;
            return Buffer[(int)(position.Start.Value % BlockSizeValue)..(int)(position.End.Value % BlockSizeValue)];
        }
    }

    public char this[Index64 position]
    {
        get
        {
            Index = position.Value / BlockSizeValue;
            return Buffer[(int)(position.Value % BlockSizeValue)];
        }
    }

    public char this[long position]
    {
        get
        {
            Index = position / BlockSizeValue;
            return Buffer[(int)(position % BlockSizeValue)];
        }
    }

    public SmbFileBuffer(SmbFile parent, int? blockSize = null)
    {
        Parent = parent;
        BufferCache = new(() => Parent.SubString(BlockSize * IndexValue, BlockSize) ?? "");
        if(blockSize != null)
            BlockSizeValue = blockSize.Value;
    }
}