using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class ScannerTokenType : DumpableObject, IScannerTokenType, IParserTokenFactory
    {
        IParserTokenType<TSourcePart> IParserTokenFactory.GetTokenType<TSourcePart>(string id)
            => GetParserTokenType<TSourcePart>(id);

        IParserTokenFactory IScannerTokenType.ParserTokenFactory => this;
        string IScannerTokenType.Id => GetType().PrettyName();

        protected abstract IParserTokenType<TSourcePart> GetParserTokenType<TSourcePart>(string id)
            where TSourcePart : class, ISourcePartProxy;
    }

    public abstract class ScannerTokenType<TSourcePart> : ScannerTokenType
        where TSourcePart : class, ISourcePartProxy
    {
        protected sealed override IParserTokenType<TSourcePartTarget> GetParserTokenType<TSourcePartTarget>
            (string id)
        {
            Tracer.Assert(typeof(TSourcePart) == typeof(TSourcePartTarget));
            return (IParserTokenType<TSourcePartTarget>) GetParserTokenType(id);
        }

        protected abstract IParserTokenType<TSourcePart> GetParserTokenType(string id);
    }
}