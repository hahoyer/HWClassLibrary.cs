using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class AndSyntax : AssociativeSyntax
    {
        public AndSyntax(IAssociative @operator, IToken token, Set<ParsedSyntax> set)
            : base(@operator, token, set) { }
    }
}