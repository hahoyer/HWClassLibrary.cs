using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class VariableSyntax
        : ParsedSyntax
            , IComparableEx<VariableSyntax>
    {
        internal readonly string Name;

        public VariableSyntax(IToken token, string name)
            : base(token)
            => Name = name;

        [DisableDump]
        internal override Set<string> Variables => [Name];

        int IComparableEx<VariableSyntax>.CompareToEx(VariableSyntax other) => string.CompareOrdinal(Name, other.Name);

        internal override bool IsDistinct(ParsedSyntax other) => IsDistinct((VariableSyntax)other);

        internal override ParsedSyntax IsolateFromEquation
            (string variable, ParsedSyntax otherSite) => Equal(Token, otherSite);

        internal override ParsedSyntax IsolateFromSum(string variable, ParsedSyntax other)
        {
            if(Name == variable)
                return other;
            return null;
        }

        internal override Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            var result = definitions.Where(valuePair => valuePair.Key == Name).Select(keyValuePair => keyValuePair.Value).ToSet();
            result.Add(this);
            return result;
        }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other) => other.CombineForPlus(this);

        internal override ParsedSyntax CombineForPlus
            (ParsedSyntax other, BigRational otherValue) => other.CombineForPlus(this, otherValue);

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue, BigRational thisValue)
            => other.CombineForPlus(this, thisValue, otherValue);

        internal override ParsedSyntax CombineForPlus(VariableSyntax other, BigRational thisValue)
        {
            if(Name == other.Name)
                return Times(thisValue + 1);
            return null;
        }

        internal override ParsedSyntax CombineForPlus
            (VariableSyntax other, BigRational otherValue, BigRational thisValue)
        {
            if(Name == other.Name)
                return Times(thisValue + otherValue);
            return null;
        }

        internal override ParsedSyntax CombineForPlus(VariableSyntax other)
        {
            if(Name == other.Name)
                return Times(2);
            return null;
        }


        internal override string SmartDump(ISmartDumpToken @operator) => Name;

        bool IsDistinct(VariableSyntax other) => Name != other.Name;
    }
}