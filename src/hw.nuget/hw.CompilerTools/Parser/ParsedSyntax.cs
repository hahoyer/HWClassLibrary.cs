using System;
using System.Collections.Generic;
using hw.Helper;
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

        readonly SourcePart _additionalSourcePart;
        [DisableDump]
        public readonly SourcePart Token;

        protected ParsedSyntax(SourcePart token, SourcePart additionalSourcePart= null)
        {
            _additionalSourcePart = additionalSourcePart;
            Token = token;
        }

        protected ParsedSyntax(SourcePart token, int objectId, SourcePart additionalSourcePart= null)
            : base(objectId)
        {
            _additionalSourcePart = additionalSourcePart;
            Token = token;
        }


        [DisableDump]
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }

        protected override string GetNodeDump() { return SourcePart.Name; }
        protected virtual string FilePosition() { return SourcePart.FilePosition; }
        internal string FileErrorPosition(string errorTag)
        {
            return SourcePart.FileErrorPosition(errorTag);
        }

        string IGraphTarget.Title { get { return Token.Name; } }
        IGraphTarget[] IGraphTarget.Children { get { return Children.ToArray<IGraphTarget>(); } }

        [DisableDump]
        protected virtual ParsedSyntax[] Children { get { return new ParsedSyntax[0]; } }
        [DisableDump]
        public SourcePart SourcePart
        {
            get
            {
                return _additionalSourcePart
                    + Token
                    + Children
                        .Where(item => item != null)
                        .Select(item => item.SourcePart)
                        .Aggregate();
            }
        }
    }
}