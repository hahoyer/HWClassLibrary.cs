using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.PrioParser;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : TokenFactory<TokenClass>
    {
        protected static readonly hw.Parser.Scanner Scanner = new Scanner();

        protected override TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
        protected override TokenClass GetEndOfText() { return new EndOfTextToken(); }

        sealed class SyntaxError : TokenClass
        {
            [EnableDump]
            readonly string _message;
            public SyntaxError(string message) { _message = message; }
            protected override ParsedSyntax Create(ParsedSyntax left, SourcePart part, ParsedSyntax right)
            {
                NotImplementedMethod(left, part, right);
                return null;
            }
        }
    }

    sealed class MainTokenFactory : TokenFactory
    {
        public static readonly Control Instance = new Control
        {
            TokenFactory = new MainTokenFactory(),
            PrioTable = PrioTable,
            Scanner = Scanner
        };

        MainTokenFactory() {}

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x = x.ParenthesisLevel(new[] {"(", "[", "{", PrioTable.BeginOfText}, new[] {")", "]", "}", PrioTable.EndOfText});
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass> {{"-->", new SwitchToken()}};
            return result;
        }

        protected override TokenClass GetTokenClass(string name) { return new MainToken(name); }
    }

    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly Control Instance = new Control
        {
            TokenFactory = new NestedTokenFactory(),
            PrioTable = PrioTable,
            Scanner = Scanner
        };

        NestedTokenFactory() {}

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left("-->");
                x += PrioTable.Left(PrioTable.Any);
                x = x.ParenthesisLevel(new[] {"(", "[", "{", PrioTable.BeginOfText}, new[] {")", "]", "}", PrioTable.EndOfText});
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass>();
            return result;
        }

        protected override TokenClass GetTokenClass(string name) { return new NestedToken(name); }
    }

    sealed class SwitchToken : NamedToken
    {
        public SwitchToken()
            : base("-->") {}
        public override bool IsMain { get { return true; } }
        protected override ParsedSyntax Create(ParsedSyntax left, SourcePart part, ParsedSyntax right)
        {
            if(right == null && left == null)
                return new Syntax(null, this, part, null);

            Tracer.Assert(left == null, ()=>left.Dump());
            return right;
        }
        protected override Control Next { get { return NestedTokenFactory.Instance; } }
    }

    abstract class NamedToken : TokenClass
    {
        [EnableDump]
        readonly string _name;
        protected NamedToken(string name) { _name = name; }
        public abstract bool IsMain { get; }
        protected override ParsedSyntax Create(ParsedSyntax left, SourcePart part, ParsedSyntax right)
        {
            return new Syntax((Syntax) left, this, part, (Syntax) right);
        }
    }

    sealed class MainToken : NamedToken
    {
        public MainToken(string name)
            : base(name)
        {}
        public override bool IsMain { get { return true; } }
    }

    sealed class NestedToken : NamedToken
    {
        public NestedToken(string name)
            : base(name)
        {}
        public override bool IsMain { get { return false; } }
    }

    sealed class Syntax : ParsedSyntax
    {
        public readonly Syntax Left;
        public readonly NamedToken TokenClass;
        public readonly Syntax Right;

        public Syntax(Syntax left, NamedToken tokenClass, SourcePart part, Syntax right)
            : base(part)
        {
            Left = left;
            TokenClass = tokenClass;
            Right = right;
        }
    }

    sealed class EndOfTextToken : TokenClass
    {
        protected override ParsedSyntax Create(ParsedSyntax left, SourcePart part, ParsedSyntax right)
        {
            Tracer.Assert(right == null);
            return left;
        }
    }
}