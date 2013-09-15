#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Reni.Parser;
using Reni.Proof.TokenClasses;

namespace Reni.Proof
{
    abstract class ParsedSyntax : ParsedSyntaxBase, IComparable<ParsedSyntax>
    {
        protected ParsedSyntax(TokenData token)
            : base(token) { }

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
        internal bool IsSimpleVariable { get { return this is VariableSyntax; } }

        [DisableDump]
        internal virtual bool IsNegative { get { return false; } }

        internal ParsedSyntax Associative<TOperation>(TOperation operation, TokenData token, ParsedSyntax other)
            where TOperation : IAssociative { return operation.CombineAssosiative(token, new[] {this, other}); }

        internal virtual bool IsDistinct(ParsedSyntax other)
        {
            NotImplementedMethod(other);
            return false;
        }

        [UsedImplicitly]
        internal string SmartDumpText { get { return SmartDump(null); } }

        [DisableDump]
        internal virtual BigRational Factor { get { return 1; } }

        internal virtual string SmartDump(ISmartDumpToken @operator)
        {
            NotImplementedMethod(@operator);
            return GetNodeDump();
        }

        protected virtual ParsedSyntax IsolateClause(string variable)
        {
            NotImplementedMethod(variable);
            return null;
        }

        int IComparable<ParsedSyntax>.CompareTo(ParsedSyntax other) { return CompareTo(other); }

        internal virtual ParsedSyntax IsolateFromEquation(string variable, ParsedSyntax otherSite) { return null; }

        internal ParsedSyntax Minus(IEnumerable<ParsedSyntax> others) { return others.Aggregate(this, (x, y) => x.Minus(y)); }

        internal ParsedSyntax Minus(ParsedSyntax other) { return Minus(null, other); }
        internal ParsedSyntax Minus(TokenData token, ParsedSyntax other) { return Plus(token, other.Negative()); }
        internal ParsedSyntax Negative() { return Times(-1); }
        internal virtual ParsedSyntax Times(BigRational value)
        {
            if(value == 0)
                return new NumberSyntax(0);
            if(value == 1)
                return this;
            return new FactorSyntax(this, value);
        }

        internal virtual ParsedSyntax IsolateFromSum(string variable, ParsedSyntax other) { return null; }

        internal ParsedSyntax Equal(TokenData token, ParsedSyntax other)
        {
            var difference = CompareTo(other);
            if(difference == 0)
                return TrueSyntax.Instance;
            if(this is PlusSyntax || other is PlusSyntax)
                return Minus(token, other).Normalize().DefaultEqual(token, new NumberSyntax(0));
            return DefaultEqual(token, other);
        }

        protected virtual ParsedSyntax Normalize()
        {
            NotImplementedMethod();
            return null;
        }

        EqualSyntax DefaultEqual(TokenData token, ParsedSyntax other) { return new EqualSyntax(this, token, other); }

        internal ParsedSyntax Plus(TokenData token, ParsedSyntax otherSite) { return Associative(Main.TokenFactory.Plus, token, otherSite); }

        int? GenericCompareTo<T>(ParsedSyntax other)
            where T : ParsedSyntax, IComparableEx<T>
        {
            if(this is T)
            {
                if(other is T)
                    return ((T) this).CompareToEx((T) other);
                return 1;
            }
            if(other is T)
                return -1;

            return null;
        }

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

        internal ParsedSyntax Pair(IPair @operator, ParsedSyntax right) { return @operator.Pair(this, right); }
        protected Set<ParsedSyntax> DefaultReplace() { return this.ToSet(); }

        internal int CompareTo(ParsedSyntax right)
        {
            var result = Variables.Count().CompareTo(right.Variables.Count());
            if(result != 0)
                return result;
            var v1 = Variables.OrderBy(x => x).ToArray();
            var v2 = right.Variables.OrderBy(x => x).ToArray();
            result = v1.Zip(v2, String.CompareOrdinal).FirstOrDefault(x => x != 0);
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
            result = potentialResult == null ? VirtualCompareTo(right) : potentialResult.Value;
            return result;
        }

        internal static readonly IComparer<ParsedSyntax> Comparer = new ComparerClass();

        sealed class ComparerClass : IComparer<ParsedSyntax>
        {
            public int Compare(ParsedSyntax x, ParsedSyntax y) { return x.CompareTo(y); }
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

        internal virtual ParsedSyntax CombineForPlus(VariableSyntax other, BigRational otherValue, BigRational thisValue)
        {
            NotImplementedMethod(other, otherValue, thisValue);
            return null;
        }

        internal KeyValuePair<string, ParsedSyntax> GetDefinition(string variable) { return new KeyValuePair<string, ParsedSyntax>(variable, IsolateClause(variable)); }
    }

    interface IComparableEx<in T>
    {
        int CompareToEx(T other);
    }
}