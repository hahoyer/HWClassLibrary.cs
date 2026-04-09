namespace hw.Helper;

[PublicAPI]
public class SmbFileBuffer
{
    public readonly SmbFile Parent;
    long[] IndexValue;
    int BlockSizeValue = 100_000_000;
    ValueCache<string>[] BufferCache;
    int CurrentBufferIndex;

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