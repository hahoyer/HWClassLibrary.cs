using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IToken
    {
        SourcePosn Start { get; }
        SourcePart SourcePart { get; }
        string Id { get; }
        WhiteSpaceToken[] PrecededWith { get; }
        SourcePart Characters { get; }
    }
}