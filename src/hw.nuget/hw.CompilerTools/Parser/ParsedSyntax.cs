using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    public abstract class ParsedSyntax : DumpableObject, ISourcePart
    {
        [UsedImplicitly]
        internal static bool IsDetailedDumpRequired = true;

        [DisableDump]
        public readonly IToken Token;

        protected ParsedSyntax(IToken token) { Token = token; }

        protected ParsedSyntax(IToken token, int objectId)
            : base(objectId)
        {
            Token = token;
        }

        protected override string GetNodeDump() => SourcePart.Id;
        protected virtual string FilePosition() => SourcePart.FilePosition;

        internal string FileErrorPosition(string errorTag) => SourcePart.FileErrorPosition(errorTag);

        public SourcePart SourcePart => Token.SourcePart();

        SourcePart ISourcePart.All => SourcePart;
    }
}