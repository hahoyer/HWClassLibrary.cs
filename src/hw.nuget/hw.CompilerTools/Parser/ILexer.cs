using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface ILexer
    {
        Func<SourcePosn, int?>[] WhiteSpace { get; }
        int? Number(SourcePosn sourcePosn);
        int? Text(SourcePosn sourcePosn);
        int? Any(SourcePosn sourcePosn);
        Match.IError InvalidCharacterError { get; }
    }
}