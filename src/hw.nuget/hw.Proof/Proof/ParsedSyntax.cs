using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Proof.TokenClasses;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    abstract class ParsedSyntax
        : DumpableObject
            , IComparable<ParsedSyntax>
    {
        sealed class ComparerClass : IComparer<ParsedSyntax>
        {
            public int Compare(ParsedSyntax target, ParsedSyntax y) => target?.CompareTo(y)?? 0;
        }

        internal static readonly IComparer<ParsedSyntax> Comparer = new ComparerClass();
        protected readonly IToken Token;

        protected ParsedSyntax(IToken token) => Token = token;

        [DisableDump]
        internal virtual Set<string> Variables
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal bool IsSimpleVariable => this is VariableSyntax;

        [DisableDump]
        internal virtual bool IsNegative => false;

        [UsedImplicitly]
        internal string SmartDumpText => SmartDump(null);

        [DisableDump]
        internal virtual BigRational Factor => 1;

        int IComparable<ParsedSyntax>.CompareTo(ParsedSyntax other) => CompareTo(other);

        protected virtual ParsedSyntax IsolateClause(string variable)
        {
            NotImplementedMethod(variable);
            return null;
        }

        protected virtual ParsedSyntax Normalize()
        {
            NotImplementedMethod();
            return null;
        }

        protected Set<ParsedSyntax> DefaultReplace() => this.ToSet();

        internal ParsedSyntax Associative<TOperation>(TOperation operation, IToken token, ParsedSyntax other)
            where TOperation : IAssociative
            => operation.CombineAssosiative(token, new[] {this, other});

        internal virtual bool IsDistinct(ParsedSyntax other)
        {
            NotImplementedMethod(other);
            return false;
        }

        internal virtual string SmartDump(ISmartDumpToken @operator)
        {
            NotImplementedMethod(@operator);
            return GetNodeDump();
        }

        internal virtual ParsedSyntax IsolateFromEquation(string variable, ParsedSyntax otherSite) => null;

        internal ParsedSyntax Minus
            (IEnumerable<ParsedSyntax> others) => others.Aggregate(this, (target, y) => target.Minus(y));

        internal ParsedSyntax Minus(ParsedSyntax other) => Minus(null, other);

        internal ParsedSyntax Minus(IToken token, ParsedSyntax other) => Plus(token, other.Negative());

        internal ParsedSyntax Negative() => Times(-1);

        internal virtual ParsedSyntax Times(BigRational value)
        {
            if(value == 0)
                return new NumberSyntax(0);
            if(value == 1)
                return this;
            return new FactorSyntax(this, value);
        }

        internal virtual ParsedSyntax IsolateFromSum(string variable, ParsedSyntax other) => null;

        internal ParsedSyntax Equal(IToken token, ParsedSyntax other)
        {
            var difference = CompareTo(other);
            if(difference == 0)
                return TrueSyntax.Instance;
            if(this is PlusSyntax || other is PlusSyntax)
                return Minus(token, other).Normalize().DefaultEqual(token, new NumberSyntax(0));
            return DefaultEqual(token, other);
        }

        internal ParsedSyntax Plus
            (IToken token, ParsedSyntax otherSite) => Associative(Definitions.Plus, token, otherSite);

        internal virtual int VirtualCompareTo(ParsedSyntax other)
        {
            NotImplementedMethod(other);
            return 0;
        }

        internal virtual Set<ParsedSyntax> Replace(IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            NotImplementedMethod(definitions);
            return null;
        }

        internal ParsedSyntax Pair(IPair @operator, ParsedSyntax right) => @operator.Pair(this, right);

        internal int CompareTo(ParsedSyntax right)
        {
            var result = Variables.Count().CompareTo(right.Variables.Count());
            if(result != 0)
                return result;
            var v1 = Variables.OrderBy(target => target).ToArray();
            var v2 = right.Variables.OrderBy(target => target).ToArray();
            result = v1.Zip(v2, string.CompareOrdinal).FirstOrDefault(target => target != 0);
            if(result != 0)
                return result;
            var potentialResult = GenericCompareTo<EqualSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<PlusSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<GreatesCommonDenominatorSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<FactorSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<PowerSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<ElementSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<VariableSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<NumberSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<IntegerSyntax>(right);
            if(potentialResult == null)
                potentialResult = GenericCompareTo<TrueSyntax>(right);
            result = potentialResult == null? VirtualCompareTo(right) : potentialResult.Value;
            return result;
        }

        internal virtual ParsedSyntax CombineForPlus(ParsedSyntax other)
        {
            NotImplementedMethod(other);
            return null;
        }

        internal virtual ParsedSyntax CombineForPlus(PowerSyntax other)
        {
            NotImplementedMethod(other);
            return null;
        }

        internal virtual ParsedSyntax CombineForPlus(VariableSyntax other)
        {
            NotImplementedMethod(other);
            return null;
        }

        internal virtual ParsedSyntax CombineForPlus(VariableSyntax other, BigRational thisValue)
        {
            NotImplementedMethod(other, thisValue);
            return null;
        }

        internal virtual ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue)
        {
            NotImplementedMethod(other, otherValue);
            return null;
        }

        internal virtual ParsedSyntax CombineForPlus(PowerSyntax other, BigRational thisValue)
        {
            NotImplementedMethod(other, thisValue);
            return null;
        }

        internal virtual ParsedSyntax CombineForPlus(ParsedSyntax other, BigRational otherValue, BigRational thisValue)
        {
            NotImplementedMethod(other, otherValue, thisValue);
            return null;
        }

        internal virtual ParsedSyntax CombineForPlus
            (VariableSyntax other, BigRational otherValue, BigRational thisValue)
        {
            NotImplementedMethod(other, otherValue, thisValue);
            return null;
        }

        internal KeyValuePair<string, ParsedSyntax> GetDefinition
            (string variable) => new KeyValuePair<string, ParsedSyntax>(variable, IsolateClause(variable));

        EqualSyntax DefaultEqual(IToken token, ParsedSyntax other) => new EqualSyntax(this, token, other);

        int? GenericCompareTo<T>(ParsedSyntax other)
            where T : ParsedSyntax, IComparableEx<T>
        {
            if(this is T)
            {
                if(other is T)
                    return ((T)this).CompareToEx((T)other);
                return 1;
            }

            if(other is T)
                return -1;

            return null;
        }
    }

    interface IComparableEx<in T>
    {
        int CompareToEx(T other);
    }
}