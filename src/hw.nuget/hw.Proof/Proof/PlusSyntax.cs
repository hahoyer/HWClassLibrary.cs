using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Proof.TokenClasses;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class PlusSyntax : AssociativeSyntax, IComparableEx<PlusSyntax>
    {
        public PlusSyntax(Plus @operator, IToken token, Set<ParsedSyntax> set)
            : base(@operator, token, set)
        {
            set.All(target => !(target is PlusSyntax)).Assert();
        }

        internal override ParsedSyntax IsolateFromEquation(string variable, ParsedSyntax otherSite)
        {
            var contains = Set.Where(target => target.Variables.Contains(variable)).ToArray();
            var notContains = Set.Where(target => !target.Variables.Contains(variable));
            if(contains.Count() != 1)
                return null;

            return contains.First().IsolateFromSum(variable, otherSite.Minus(notContains));
        }

        internal override ParsedSyntax Times(BigRational value)
        {
            return Operator.CombineAssosiative(Token, Set.Select(target => target.Times(value)));
        }

        protected override ParsedSyntax Normalize()
        {
            var factor = Set.First().Factor;
            if(factor == 1)
                return this;
            return Times(1 / factor);
        }

        int IComparableEx<PlusSyntax>.CompareToEx(PlusSyntax other)
        {
            if(other == null)
                return 1;

            var result = Set.Count().CompareTo(other.Set.Count());
            if(result != 0)
                return result;

            var s1 = Set.OrderBy(target => target, Comparer).ToArray();
            var s2 = other.Set.OrderBy(target => target, Comparer).ToArray();

            for(var i = 0; i < s1.Length; i++)
            {
                result = s1[i].CompareTo(s2[i]);
                if(result != 0)
                    return result;
            }

            return result;
        }

        internal override Set<ParsedSyntax> Replace
            (IEnumerable<KeyValuePair<string, ParsedSyntax>> definitions)
        {
            var replaced = Set.Select(summand => summand.Replace(definitions));
            var results =
                replaced.Aggregate
                    (
                        new[] {Set<ParsedSyntax>.Empty},
                        (result, variant) =>
                            result.SelectMany
                                (soFar => variant.Select(newElement => soFar | newElement))
                                .ToArray())
                    .Select(target => Operator.CombineAssosiative(Token, target))
                    .ToSet();
            return results;
        }
    }
}