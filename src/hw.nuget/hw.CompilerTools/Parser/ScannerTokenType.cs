using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class ScannerTokenType : DumpableObject, IScannerTokenType, IParserTokenFactory
    {
        public IParserTokenFactory ParserTokenFactory => this;

        IParserTokenType<TTreeItem> IParserTokenFactory.GetTokenType<TTreeItem>(string id)
            => GetParserTokenType<TTreeItem>(id);

        protected abstract IParserTokenType<TTreeItem> GetParserTokenType<TTreeItem>(string id)
            where TTreeItem : class, ISourcePart;
    }

    public abstract class ScannerTokenType<TTreeItem> : ScannerTokenType
        where TTreeItem : class, ISourcePart
    {
        protected sealed override IParserTokenType<TTreeItem1> GetParserTokenType<TTreeItem1>
            (string id)
        {
            Tracer.Assert(typeof(TTreeItem) == typeof(TTreeItem1));
            return (IParserTokenType<TTreeItem1>) GetParserTokenType(id);
        }

        protected abstract IParserTokenType<TTreeItem> GetParserTokenType(string id);
    }
}