using hw.Helper;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class TrueSyntax
        : ParsedSyntax
            , IComparableEx<TrueSyntax>
    {
        static readonly ValueCache<ParsedSyntax> InstanceCache = new(Create);

        TrueSyntax()
            : base(null) { }

        internal static ParsedSyntax Instance => InstanceCache.Value;

        internal override Set<string> Variables => Set<string>.Empty;
        int IComparableEx<TrueSyntax>.CompareToEx(TrueSyntax other) => 0;
        static ParsedSyntax Create() => new TrueSyntax();
    }
}