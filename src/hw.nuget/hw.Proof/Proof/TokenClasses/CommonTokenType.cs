using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    abstract class CommonTokenType : ScannerTokenType<ParsedSyntax>,
        IUniqueIdProvider,
        IParserTokenType<ParsedSyntax>
    {
        protected virtual ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        ParsedSyntax IParserTokenType<ParsedSyntax>.Create
            (ParsedSyntax left, IToken token, ParsedSyntax right) => Syntax(left, token, right);

        string IParserTokenType<ParsedSyntax>.PrioTableId => Id;

        string IUniqueIdProvider.Value => Id;

        protected abstract string Id { get; }

        protected override IParserTokenType<ParsedSyntax> GetParserTokenType(string id) => this;
    }
}