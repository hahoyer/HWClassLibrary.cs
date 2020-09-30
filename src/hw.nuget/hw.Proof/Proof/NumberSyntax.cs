using System;
using System.Collections.Generic;
using System.Numerics;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;

namespace hw.Proof
{
    sealed class NumberSyntax : ParsedSyntax, IComparableEx<NumberSyntax>
    {
        internal readonly BigRational Value;

        internal NumberSyntax(IToken token)
            : base(token)
        {
            Value = BigInteger.Parse(token.Characters.Id);
        }

        internal NumberSyntax(BigRational value)
            : base(null)
        {
            Value = value;
        }

        [DisableDump]
        internal override Set<string> Variables { get { return new Set<string>(); } }

        internal override bool IsDistinct(ParsedSyntax other)
        {
            throw new NotImplementedException();
        }
        internal override string SmartDump(ISmartDumpToken @operator) { return Value.ToString(); }
        internal override ParsedSyntax Times(BigRational value)
        {
            return new NumberSyntax(Value * value);
        }
        internal override Set<ParsedSyntax> Replace
            (IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            return DefaultReplace();
        }
        public int CompareToEx(NumberSyntax other) { return Value.CompareTo(other.Value); }
    }
}