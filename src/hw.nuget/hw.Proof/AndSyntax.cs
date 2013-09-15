using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class AndSyntax : AssociativeSyntax
    {
        public AndSyntax(IAssociative @operator, TokenData token, Set<ParsedSyntax> set)
            : base(@operator, token, set) { }
    }
}