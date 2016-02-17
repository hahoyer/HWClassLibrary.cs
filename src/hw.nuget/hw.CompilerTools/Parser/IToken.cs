using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public interface IToken
    {
        [DisableDump]
        SourcePart SourcePart { get; }
        [DisableDump]
        string Id { get; }
        [DisableDump]
        WhiteSpaceToken[] PrecededWith { get; }
        [DisableDump]
        SourcePart Characters { get; }
        [DisableDump]
        bool? IsBracketAndLeftBracket{ get; }
    }
}