using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : TokenFactory<TokenClass<Syntax>, Syntax>
    {
        protected static readonly IScanner<Syntax> Scanner = new Scanner<Syntax>(new ReniLexer());

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
        public static readonly IParser<Syntax> Instance = new PrioParser<Syntax>(PrioTable, Scanner, new MainTokenFactory());

        MainTokenFactory() { }

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x = x.ParenthesisLevelRight
                    (
                        new[] {"("},
                        new[] {")"}
                    );
                x = x.ParenthesisLevelLeft
                            (
                                new[] { PrioTable.BeginOfText },
                                new[] { PrioTable.EndOfText }
                            );
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override FunctionCache<string, TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass<Syntax>>
            {
                {"-->", new SwitchToken()},
                {"(", new LeftParenthesis()},
                {")", new RightParenthesis()}
            };
            return result;
        }

        protected override TokenClass<Syntax> GetTokenClass(string name) { return new MainToken(name); }
    }

    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Instance = new PrioParser<Syntax>(PrioTable, Scanner, new NestedTokenFactory());

        NestedTokenFactory() { }

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x = x.ParenthesisLevelLeft
                    (
                        new[] {"(", PrioTable.BeginOfText},
                        new[] {")", PrioTable.EndOfText}
                    );
                x.Correct(PrioTable.Any, PrioTable.BeginOfText, '-');
                x.Correct(")", PrioTable.BeginOfText, '-');
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override FunctionCache<string, TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass<Syntax>>
            {
                {"(", new LeftParenthesis()},
                {")", new RightParenthesis()}
            };
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
        protected override ISubParser<Syntax> Next { get { return NestedTokenFactory.Instance.Convert(Converter); } }

        IType<Syntax> Converter(Syntax arg) { return new SyntaxBoxToken(arg); }

        sealed class SyntaxBoxToken : TokenClass<Syntax>
        {
            [EnableDump]
            readonly Syntax _content;
            public SyntaxBoxToken(Syntax content) { _content = content; }
            protected override Syntax Create(Syntax left, SourcePart part, Syntax right)
            {
                Tracer.Assert(left == null);
                Tracer.Assert(right == null);
                return _content;
            }
        }
    }

    abstract class NamedToken : TokenClass<Syntax>
    {
        protected NamedToken(string name) { Name = name; }
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
        [DisableDump]
        public override bool IsMain { get { return true; } }
    }

    sealed class NestedToken : NamedToken
    {
        public NestedToken(string name)
            : base(name)
        {}

        public override bool IsMain { get { return false; } }
    }

    [DebuggerDisplay("{NodeDump}")]
    sealed class Syntax : ParsedSyntax, ITreeItem
    {
        static readonly NamedToken _emptyTokenClass = new MainToken("");

        public readonly Syntax Left;
        public readonly NamedToken TokenClass;
        public readonly Syntax Right;

        public Syntax(Syntax left, NamedToken tokenClass, SourcePart part, Syntax right)
            : base(part)
        {
            Left = left;
            TokenClass = tokenClass ?? _emptyTokenClass;
            Right = right;
        }

        protected override string Dump(bool isRecursion) { return this.TreeDump(); }
        ITreeItem ITreeItem.Left { get { return Left; } }
        string ITreeItem.TokenId
        {
            get
            {
                var result = TokenClass.Name;
                if(result.In("(", ")"))
                    return result.Quote();
                return result;
            }
        }
        ITreeItem ITreeItem.Right { get { return Right; } }
    }

    sealed class EndOfTextToken : TokenClass<Syntax>
    {
        protected override Syntax Create(Syntax left, SourcePart part, Syntax right)
        {
            Tracer.Assert(right == null);
            return left;
        }
    }

    sealed class LeftParenthesis : TokenClass<Syntax>
    {
        protected override Syntax Create(Syntax left, SourcePart token, Syntax right)
        {
            if(left == null && right != null)
                return right;
            return new Syntax(left, null, token, right);
        }
    }

    sealed class RightParenthesis : TokenClass<Syntax>
    {
        protected override IType<Syntax> Match(Syntax other) { return other.TokenClass; }
        protected override Syntax Create(Syntax left, SourcePart token, Syntax right)
        {
            if (left != null && right == null)
                return left;
            return new Syntax(left, null, token, right);
        }
    }
}