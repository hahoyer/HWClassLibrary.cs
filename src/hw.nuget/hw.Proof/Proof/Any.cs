using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Proof
{
    sealed class Any : PredefinedTokenFactory<ParsedSyntax>
    {
        protected override IParserTokenType<ParsedSyntax> GetTokenClass(string name)
            => Definitions.GetTokenClass(name);

        protected override IEnumerable<IParserTokenType<ParsedSyntax>> GetPredefinedTokenClasses()
            => Definitions.PredefinedTokenClasses;
    }
}