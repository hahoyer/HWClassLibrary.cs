using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Graphics;
using hw.Scanner;
using JetBrains.Annotations;

namespace hw.Parser
{
    public abstract class ParsedSyntax : DumpableObject, IGraphTarget, IIconKeyProvider
    {
        [UsedImplicitly]
        internal static bool IsDetailedDumpRequired = true;

        readonly SourcePart _token;

        protected ParsedSyntax(SourcePart token) { _token = token; }

        protected ParsedSyntax(SourcePart token, int nextObjectId)
            : base(nextObjectId) { _token = token; }


        [DisableDump]
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }

        [DisableDump]
        internal SourcePart Token { get { return _token; } }

        protected override string GetNodeDump() { return Token.Name; }
        protected virtual string FilePosition() { return Token.FilePosition; }
        internal string FileErrorPosition(string errorTag) { return Token.FileErrorPosition(errorTag); }

        string IGraphTarget.Title { get { return Token.Name; } }
        IGraphTarget[] IGraphTarget.Children { get { return Children.ToArray<IGraphTarget>(); } }

        [DisableDump]
        protected virtual ParsedSyntax[] Children { get { return new ParsedSyntax[0]; } }
    }
}