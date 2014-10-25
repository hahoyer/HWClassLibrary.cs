using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;

namespace hw.Parser
{
    public interface IParsedSyntax : IIconKeyProvider
    {
        [DisableDump]
        TokenData Token { get; }

        TokenData FirstToken { get; }
        TokenData LastToken { get; }
        string Dump();
        string GetNodeDump();
    }
}