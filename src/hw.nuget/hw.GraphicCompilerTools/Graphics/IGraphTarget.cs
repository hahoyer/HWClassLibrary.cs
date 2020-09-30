namespace hw.Graphics
{
    public interface IGraphTarget
    {
        string Title { get; }
        IGraphTarget[] Children { get; }
    }
}