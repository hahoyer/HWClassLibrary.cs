using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Proof
{
    sealed class Holder : Dumpable
    {
        readonly string _text;
        readonly ClauseSyntax _statement;
        readonly Parser.Scanner _scanner = new Scanner();

        public Holder(string text)
        {
            var file = "main.proof".FileHandle();
            file.String = text;
            _text = text;
            var parsedSyntax =
                new Control {TokenFactory = Main.TokenFactory, Scanner = _scanner, PrioTable = TokenFactory.PrioTable}.Parse
                    (new Source(file));
            _statement = (ClauseSyntax) parsedSyntax;
        }

        internal Set<string> Variables { get { return _statement.Variables; } }
        internal ClauseSyntax Statement { get { return _statement; } }
    }
}