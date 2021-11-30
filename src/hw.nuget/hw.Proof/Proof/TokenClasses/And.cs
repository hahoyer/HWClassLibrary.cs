using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof.TokenClasses
{
    sealed class And : ParserTokenType, IAssociative, ISmartDumpToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            (left != null).Assert();
            (right != null).Assert();

            return left.Associative(this, token, right);
        }

        protected override string Id => "&";

        [DisableDump]
        bool IAssociative.IsVariablesProvider => true;

        [DisableDump]
        ParsedSyntax IAssociative.Empty => TrueSyntax.Instance;

        AssociativeSyntax IAssociative.Syntax(IToken token, Set<ParsedSyntax> target)
            => new AndSyntax(this, token, target);

        ParsedSyntax IAssociative.Combine(ParsedSyntax left, ParsedSyntax right) => null;
        bool IAssociative.IsEmpty(ParsedSyntax parsedSyntax) => parsedSyntax is TrueSyntax;
        string IAssociative.SmartDump(Set<ParsedSyntax> set) => SmartDump(this, set);

        string ISmartDumpToken.SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst)
            => isFirst ? "" : " & ";

        [DisableDump]
        bool ISmartDumpToken.IsIgnoreSignSituation => false;
    }
}