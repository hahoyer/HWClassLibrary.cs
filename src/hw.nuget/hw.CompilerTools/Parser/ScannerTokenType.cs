using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    public abstract class ScannerTokenType
        : DumpableObject
            , IScannerTokenType
            , IParserTokenFactory
    {
        IParserTokenType<TSourcePart> IParserTokenFactory.GetTokenType<TSourcePart>(string id)
            => GetParserTokenType<TSourcePart>(id);

        string IScannerTokenType.Id => GetType().PrettyName();

        IParserTokenFactory IScannerTokenType.ParserTokenFactory => this;

        protected abstract IParserTokenType<TSourcePart> GetParserTokenType<TSourcePart>(string id)
            where TSourcePart : class;
    }

    public abstract class ScannerTokenType<TSourcePart> : ScannerTokenType
        where TSourcePart : class
    {
        protected sealed override IParserTokenType<TSourcePartTarget> GetParserTokenType<TSourcePartTarget>(string id)
        {
            Tracer.Assert(typeof(TSourcePart) == typeof(TSourcePartTarget));
            return (IParserTokenType<TSourcePartTarget>)GetParserTokenType(id);
        }

        protected abstract IParserTokenType<TSourcePart> GetParserTokenType(string id);
    }
}