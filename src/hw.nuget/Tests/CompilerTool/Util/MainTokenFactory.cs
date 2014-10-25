using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : TokenFactory<TokenClass<Syntax>, Syntax>
    {
        protected static readonly IScanner<Syntax> Scanner = new Scanner<Syntax>(new Proof.ReniLexer());

        protected override TokenClass<Syntax> GetSyntaxError(string message) { return new SyntaxError(message); }
        protected override TokenClass<Syntax> GetEndOfText() { return new EndOfTextToken(); }

        sealed class SyntaxError : TokenClass<Syntax>
        {
            [EnableDump]
            readonly string _message;
            public SyntaxError(string message) { _message = message; }
            protected override Syntax Create(Syntax left, SourcePart part, Syntax right)
            {
                NotImplementedMethod(left, part, right);
                return null;
            }
        }
    }

    sealed class MainTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Parser = new PrioParser<Syntax>(PrioTable,Scanner,new MainTokenFactory());

        MainTokenFactory() { }

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

        protected override FunctionCache<string, TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass<Syntax>> {{"-->", new SwitchToken()}};
            return result;
        }

        protected override TokenClass<Syntax> GetTokenClass(string name) { return new MainToken(name); }
    }

    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly ISubParser<Syntax> ParserInstance = new Parser();

        sealed class Parser : DumpableObject, ISubParser<Syntax>
        {
            IType<Syntax> ISubParser<Syntax>.Execute(SourcePosn sourcePosn, Stack<OpenItem<Syntax>> stack)
            {
                NotImplementedMethod(sourcePosn, stack);
                return null;
            }
        }

        NestedTokenFactory() { }

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

        protected override FunctionCache<string, TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass<Syntax>>();
            return result;
        }

        protected override TokenClass<Syntax> GetTokenClass(string name) { return new NestedToken(name); }
    }

    sealed class SwitchToken : NamedToken
    {
        public SwitchToken()
            : base("-->")
        {}
        public override bool IsMain { get { return true; } }
        protected override Syntax Create(Syntax left, SourcePart part, Syntax right)
        {
            if(right == null && left == null)
                return new Syntax(null, this, part, null);

            Tracer.Assert(left == null, () => left.Dump());
            return right;
        }
        protected override ISubParser<Syntax> Next { get { return NestedTokenFactory.ParserInstance; } }
    }

    abstract class NamedToken : TokenClass<Syntax>
    {
        [EnableDump]
        readonly string _name;
        protected NamedToken(string name) { _name = name; }
        public abstract bool IsMain { get; }
        protected override Syntax Create(Syntax left, SourcePart part, Syntax right)
        {
            return new Syntax(left, this, part, right);
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

    sealed class EndOfTextToken : TokenClass<Syntax>
    {
        protected override Syntax Create(Syntax left, SourcePart part, Syntax right)
        {
            Tracer.Assert(right == null);
            return left;
        }
    }
}