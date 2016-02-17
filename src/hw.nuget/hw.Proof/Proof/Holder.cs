using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class Holder : Dumpable
    {
        readonly string _text;
        readonly ClauseSyntax _statement;
        readonly Scanner<ParsedSyntax> _scanner = new Scanner<ParsedSyntax>(new ReniLexer(),Main.TokenFactory);

        public Holder(string text)
        {
            var file = "main.proof".FileHandle();
            file.String = text;
            _text = text;
            IParser<ParsedSyntax> prioParser = new PrioParser<ParsedSyntax>(TokenFactory.PrioTable, _scanner, null);
            var parsedSyntax =
                prioParser.Execute(new Source(file) + 0, null);
            _statement = (ClauseSyntax) parsedSyntax;
        }

        internal Set<string> Variables { get { return _statement.Variables; } }
        internal ClauseSyntax Statement { get { return _statement; } }
    }
}