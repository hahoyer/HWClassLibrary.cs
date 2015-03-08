using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class IntegerSyntax : ParsedSyntax, IComparableEx<IntegerSyntax>
    {
        public IntegerSyntax(Token token)
            : base(token) { }

        [DisableDump]
        internal override Set<string> Variables { get { return new Set<string>(); } }

        internal override string SmartDump(ISmartDumpToken @operator) { return Token.Name; }
        public int CompareToEx(IntegerSyntax other) { return 0; }
    }
}