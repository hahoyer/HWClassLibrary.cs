using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface ILexer
    {
        int WhiteSpace(SourcePosn sourcePosn);
        int? Number(SourcePosn sourcePosn);
        int? Text(SourcePosn sourcePosn);
        int? Any(SourcePosn sourcePosn);
    }
}