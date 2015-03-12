using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;

namespace hw.Proof
{
    sealed class VariableSyntax : ParsedSyntax, IComparableEx<VariableSyntax>
    {
        internal readonly string Name;

        public VariableSyntax(IToken token, string name)
            : base(token)
        {
            Name = name;
        }

        int IComparableEx<VariableSyntax>.CompareToEx(VariableSyntax other)
        {
            return String.CompareOrdinal(Name, other.Name);
        }

        [DisableDump]
        internal override Set<string> Variables
        {
            get
            {
                return new Set<string>
                {
                    Name
                };
            }
        }

        internal override bool IsDistinct(ParsedSyntax other)
        {
            return IsDistinct((VariableSyntax) other);
        }
        internal override ParsedSyntax IsolateFromEquation(string variable, ParsedSyntax otherSite)
        {
            return Equal(Token, otherSite);
        }
        internal override ParsedSyntax IsolateFromSum(string variable, ParsedSyntax other)
        {
            if(Name == variable)
                return other;
            return null;
        }
        internal override Set<ParsedSyntax> Replace
            (IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            var result = definitions.Where(d => d.Key == Name).Select(d => d.Value).ToSet();
            result.Add(this);
            return result;
        }

        internal override ParsedSyntax CombineForPlus(ParsedSyntax other)
        {
            return other.CombineForPlus(this);
        }
        internal override ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue)
        {
            return other.CombineForPlus(this, otherValue);
        }

        internal override ParsedSyntax CombineForPlus
            (ParsedSyntax other, BigRational otherValue, BigRational thisValue)
        {
            return other.CombineForPlus(this, thisValue, otherValue);
        }

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


        internal override string SmartDump(ISmartDumpToken @operator) { return Name; }

        bool IsDistinct(VariableSyntax other) { return Name != other.Name; }
    }
}