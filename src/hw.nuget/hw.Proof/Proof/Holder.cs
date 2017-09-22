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
        readonly TwoLayerScanner _scanner
            = new TwoLayerScanner(Main.Definitions.ScannerTokenFactory);

        public Holder(string text)
        {
            var file = "main.proof".ToSmbFile();
            file.String = text;
            _text = text;
            IParser<ParsedSyntax> prioParser = new PrioParser<ParsedSyntax>
                (Definitions.PrioTable, _scanner, null);
            var parsedSyntax =
                prioParser.Execute(new Source(file) + 0, null);
            Statement = (ClauseSyntax) parsedSyntax;
        }

        internal Set<string> Variables => Statement.Variables;
        internal ClauseSyntax Statement { get; }
    }
}