using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class PredefinedTokenFactory<TSourcePart> : ScannerTokenType<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        readonly ValueCache<FunctionCache<string, IParserTokenType<TSourcePart>>>
            PredefinedTokenClassesCache;

        protected PredefinedTokenFactory()
        {
            PredefinedTokenClassesCache =
                new ValueCache<FunctionCache<string, IParserTokenType<TSourcePart>>>(GetDictionary);
        }


        FunctionCache<string, IParserTokenType<TSourcePart>> GetDictionary()
            => new FunctionCache<string, IParserTokenType<TSourcePart>>
            (
                GetPredefinedTokenClasses().ToDictionary(item => item.PrioTableId),
                GetTokenClass
            );

        protected sealed override IParserTokenType<TSourcePart> GetParserTokenType(string id)
            => PredefinedTokenClassesCache.Value[id];

        protected abstract IEnumerable<IParserTokenType<TSourcePart>> GetPredefinedTokenClasses();
        protected abstract IParserTokenType<TSourcePart> GetTokenClass(string name);
    }
}