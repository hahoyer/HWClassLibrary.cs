using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;

namespace hw.Proof.TokenClasses
{
    abstract class ParserTokenType : DumpableObject,
        IUniqueIdProvider,
        IParserTokenType<ParsedSyntax>
    {
        protected virtual ParsedSyntax Syntax(ParsedSyntax left, IToken token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected static string SmartDump(ISmartDumpToken @operator, Set<ParsedSyntax> set)
        {
            var result = "";
            var isFirst = true;
            foreach (var parsedSyntax in set)
            {
                result += @operator.SmartDumpListDelim(parsedSyntax, isFirst);
                result += parsedSyntax.SmartDump(@operator);
                isFirst = false;
            }

            return "(" + result + ")";
        }

        ParsedSyntax IParserTokenType<ParsedSyntax>.Create
            (ParsedSyntax left, IToken token, ParsedSyntax right) => Syntax(left, token, right);

        string IParserTokenType<ParsedSyntax>.PrioTableId => Id;

        string IUniqueIdProvider.Value => Id;

        protected abstract string Id { get; }
    }
}