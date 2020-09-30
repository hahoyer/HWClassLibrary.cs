namespace hw.Parser
{
    public interface IParserTokenType<TTreeItem>
        where TTreeItem : class
    {
        string PrioTableId { get; }
        TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);
    }

    public interface IBracketMatch<TTreeItem>
        where TTreeItem : class
    {
        IParserTokenType<TTreeItem> Value { get; }
    }
}