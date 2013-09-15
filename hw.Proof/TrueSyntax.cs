using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;

namespace Reni.Proof.TokenClasses
{
    internal sealed class TrueSyntax : ParsedSyntax, IComparableEx<TrueSyntax>
    {
        internal static ParsedSyntax Instance { get { return _instance.Value; } }
        private static readonly ValueCache<ParsedSyntax> _instance = new ValueCache<ParsedSyntax>(Create);
        private static ParsedSyntax Create() { return new TrueSyntax(); }

        private TrueSyntax()
            : base(null) { }

        internal override Set<string> Variables { get { return Set<string>.Empty; } }
        int IComparableEx<TrueSyntax>.CompareToEx(TrueSyntax other) { return 0; }
    }
}