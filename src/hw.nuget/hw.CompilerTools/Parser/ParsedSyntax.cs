using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Graphics;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    public abstract class ParsedSyntax : DumpableObject, IGraphTarget, IIconKeyProvider, ISourcePart
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

        [DisableDump]
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }

        protected override string GetNodeDump() { return SourcePart.Id; }
        protected virtual string FilePosition() { return SourcePart.FilePosition; }
        internal string FileErrorPosition(string errorTag)
        {
            return SourcePart.FileErrorPosition(errorTag);
        }

        string IGraphTarget.Title { get { return Token.Id; } }
        IGraphTarget[] IGraphTarget.Children { get { return null; } }

        public SourcePart SourcePart { get { return Token.SourcePart; } }
        SourcePart ISourcePart.All { get { return SourcePart; } }
    }
}