using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class ScannerTokenType : DumpableObject, IScannerTokenType, IParserTokenFactory
    {
        IParserTokenType<TSourcePart> IParserTokenFactory.GetTokenType<TSourcePart>(string id)
            => GetParserTokenType<TSourcePart>(id);

        public IParserTokenFactory ParserTokenFactory => this;
        string IScannerTokenType.Id => GetType().PrettyName();

        protected abstract IParserTokenType<TSourcePart> GetParserTokenType<TSourcePart>(string id)
            where TSourcePart : class, ISourcePartProxy;
    }

    public abstract class ScannerTokenType<TSourcePart> : ScannerTokenType
        where TSourcePart : class, ISourcePartProxy
    {
        protected sealed override IParserTokenType<TSourcePart1> GetParserTokenType<TSourcePart1>
            (string id)
        {
            Tracer.Assert(typeof(TSourcePart) == typeof(TSourcePart1));
            return (IParserTokenType<TSourcePart1>) GetParserTokenType(id);
        }

        protected abstract IParserTokenType<TSourcePart> GetParserTokenType(string id);
    }
}