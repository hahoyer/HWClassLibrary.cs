using System.Collections.Generic;
using System.Linq;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class ClauseSyntax : AssociativeSyntax
    {
        public ClauseSyntax(IAssociative @operator, IToken token, Set<ParsedSyntax> set)
            : base(@operator, token, set) {}

        public IEnumerable<KeyValuePair<string, ParsedSyntax>> GetDefinitions(int variableCount)
        {
            return
                Set.Where(parsedSyntax => parsedSyntax.Variables.Count() <= variableCount)
                    .SelectMany(GetDefinitions)
                    .Where(pair => pair.Value != null);
        }

        static IEnumerable<KeyValuePair<string, ParsedSyntax>> GetDefinitions
            (ParsedSyntax parsedSyntax)
        {
            return parsedSyntax.Variables.Select(parsedSyntax.GetDefinition);
        }

        public string SmartDump() { return SmartDump(null); }
        public ClauseSyntax IsolateAndReplace(int variableCount)
        {
            return Apply(new IsolateAndReplaceStrategy(this, variableCount));
        }

        ClauseSyntax Apply(IStrategy strategy)
        {
            return
                (ClauseSyntax)
                    Operator.CombineAssosiative(Token, Set | Set.SelectMany(strategy.Apply).ToSet());
        }
    }
}