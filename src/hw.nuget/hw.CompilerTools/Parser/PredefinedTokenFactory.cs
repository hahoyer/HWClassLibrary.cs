using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class PredefinedTokenFactory<TTreeItem> : ScannerTokenType<TTreeItem>
        where TTreeItem : class, ISourcePart
    {
        readonly ValueCache<FunctionCache<string, IParserTokenType<TTreeItem>>>
            PredefinedTokenClassesCache;

        protected PredefinedTokenFactory()
        {
            PredefinedTokenClassesCache =
                new ValueCache<FunctionCache<string, IParserTokenType<TTreeItem>>>(GetDictionary);
        }


        FunctionCache<string, IParserTokenType<TTreeItem>> GetDictionary()
            => new FunctionCache<string, IParserTokenType<TTreeItem>>
            (
                GetPredefinedTokenClasses().ToDictionary(item => item.PrioTableId),
                GetTokenClass
            );

        protected sealed override IParserTokenType<TTreeItem> GetParserTokenType(string id)
            => PredefinedTokenClassesCache.Value[id];

        protected abstract IEnumerable<IParserTokenType<TTreeItem>> GetPredefinedTokenClasses();
        protected abstract IParserTokenType<TTreeItem> GetTokenClass(string name);
    }
}