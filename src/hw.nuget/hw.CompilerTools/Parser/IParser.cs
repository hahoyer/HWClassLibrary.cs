using System.Collections.Generic;
using hw.Scanner;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Parser
{
    [PublicAPI]
    public interface IParser<TSourcePart>
        where TSourcePart : class
    {
        bool Trace { get; set; }
        TSourcePart Execute(SourcePosition start, Stack<OpenItem<TSourcePart>> stack = null);
    }

    public interface ISubParser<TSourcePart>
        where TSourcePart : class
    {
        IParserTokenType<TSourcePart> Execute(SourcePosition sourcePosition, Stack<OpenItem<TSourcePart>> stack = null);
    }
}