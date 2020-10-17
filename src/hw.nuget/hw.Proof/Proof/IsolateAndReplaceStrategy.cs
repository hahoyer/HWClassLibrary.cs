using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    sealed class IsolateAndReplaceStrategy
        : Dumpable
            , IStrategy
    {
        readonly IEnumerable<KeyValuePair<string, ParsedSyntax>> Definitions;
        readonly int VariableCount;

        internal IsolateAndReplaceStrategy(ClauseSyntax clauseSyntax, int variableCount)
        {
            VariableCount = variableCount;
            Definitions = clauseSyntax.GetDefinitions(variableCount);
        }

        Set<ParsedSyntax> IStrategy.Apply(ParsedSyntax parsedSyntax)
        {
            if(parsedSyntax.Variables.Count() > VariableCount)
                return Set<ParsedSyntax>.Empty;
            return parsedSyntax.Replace(Definitions) - parsedSyntax;
        }
    }
}