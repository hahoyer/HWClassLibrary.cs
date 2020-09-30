using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Proof
{
    static class ParsedSyntaxExtender
    {
        internal static bool IsDistinct(this IEnumerable<ParsedSyntax> x2, IEnumerable<ParsedSyntax> x1)
            => x1.All(target => x2.All(xx => IsDistinct(target, xx)));

        internal static bool IsDistinct(ParsedSyntax target, ParsedSyntax y) 
            => target.GetType() != y.GetType() || target.IsDistinct(y);

        internal static ParsedSyntax CombineAssosiative<TOperation>
            (this TOperation operation, IToken token, IEnumerable<ParsedSyntax> target)
            where TOperation : IAssociative
        {
            var xx = operation.ToList(target);
            switch(xx.Count())
            {
                case 0:
                    return operation.Empty;
                case 1:
                    return xx.First();
            }

            return operation.Syntax(token, xx);
        }

        internal static Set<ParsedSyntax> ToList(this IAssociative @operator, IEnumerable<ParsedSyntax> set)
        {
            var selectMany =
                set.SelectMany(parsedSyntax => ToList(@operator, parsedSyntax)).ToArray();
            var parsedSyntaxs =
                selectMany.OrderBy(parsedSyntax => parsedSyntax, ParsedSyntax.Comparer).ToArray();
            var result = new List<ParsedSyntax>();
            var current = parsedSyntaxs.First();
            for(var i = 1; i < parsedSyntaxs.Count(); i++)
                current = ComineCheck(@operator, result, current, parsedSyntaxs[i]);
            if(!@operator.IsEmpty(current))
                result.Add(current);

            return result.ToArray().ToSet<ParsedSyntax>();
        }

        internal static Set<ParsedSyntax> ToList(this IAssociative @operator, ParsedSyntax parsedSyntax)
        {
            var associativeSyntax = parsedSyntax as AssociativeSyntax;
            if(associativeSyntax != null && associativeSyntax.Operator == @operator)
                return @operator.ToList(associativeSyntax.Set);
            return parsedSyntax.ToSet();
        }

        internal static bool HasDistinctElements(this IEnumerable<ParsedSyntax> set)
        {
            var varses = set.Select(target => target.Variables).ToArray();
            // ReSharper disable once UnusedVariable
            var xx = varses.Aggregate
            (
                "",
                (current, varse) =>
                    current + "(" + varse.Aggregate("", (target, y) => target + " " + y) + ")");
            var vars = Set<string>.Empty;
            foreach(var varse in varses)
                if((vars & varse).IsEmpty)
                    vars |= varse;
                else
                    return false;
            return true;
        }

        static ParsedSyntax ComineCheck
        (
            IAssociative @operator,
            ICollection<ParsedSyntax> result,
            ParsedSyntax current,
            ParsedSyntax parsedSyntax
        )
        {
            if(!(current.Variables & parsedSyntax.Variables).IsEmpty)
            {
                var newCurrent = @operator.Combine(current, parsedSyntax);
                if(newCurrent != null)
                    return newCurrent;
            }

            if(!@operator.IsEmpty(current))
                result.Add(current);
            return parsedSyntax;
        }
    }
}