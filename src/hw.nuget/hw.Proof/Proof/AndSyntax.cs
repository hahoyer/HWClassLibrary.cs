using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class AndSyntax : AssociativeSyntax
    {
        public AndSyntax(IAssociative @operator, IToken token, Set<ParsedSyntax> set)
            : base(@operator, token, set) { }
    }
}