using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    [PublicAPI]
    sealed class Holder : Dumpable
    {
        internal ClauseSyntax Statement { get; }

        readonly TwoLayerScanner Scanner = new(Main.Definitions.ScannerTokenFactory);

        readonly string Text;

        public Holder(string text)
        {
            var file = "main.proof".ToSmbFile();
            file.String = text;
            Text = text;
            IParser<ParsedSyntax> prioParser = new PrioParser<ParsedSyntax>
                (Definitions.PrioTable, Scanner, null);
            var parsedSyntax =
                prioParser.Execute(new(file));
            Statement = (ClauseSyntax)parsedSyntax;
        }

        internal Set<string> Variables => Statement.Variables;
    }
}