using System;
using System.Collections.Generic;
using System.Diagnostics;
using hw.Parser;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : TokenFactory<TokenClass<Syntax>, Syntax>
    {
        protected static IScanner<Syntax> Scanner(TokenFactory t)
        {
            return new Scanner<Syntax>(new ReniLexer(), t);
        }

        protected override TokenClass<Syntax> GetError(Match.IError message)
        {
            return new SyntaxError(message);
        }
        protected override TokenClass<Syntax> GetEndOfText() { return new EndOfText(); }

        sealed class SyntaxError : TokenClass<Syntax>
        {
            [EnableDump]
            readonly Match.IError _message;
            public SyntaxError(Match.IError message) { _message = message; }
            protected override Syntax Create(Syntax left, Token part, Syntax right)
            {
                NotImplementedMethod(left, part, right);
                return null;
            }
        }
    }

    sealed class MainTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Instance = new PrioParser<Syntax>
            (PrioTable, Scanner(new MainTokenFactory()));

        MainTokenFactory() { }

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.Left("*");
                x += PrioTable.Left("+");
                x = x.ParenthesisLevelLeft
                    (
                        new[] {"("},
                        new[] {")"}
                    );
                x += PrioTable.Left(";");
                x = x.ParenthesisLevelLeft
                    (
                        new[] {PrioTable.BeginOfText},
                        new[] {PrioTable.EndOfText}
                    );
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override IDictionary<string, TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            var result = new Dictionary<string, TokenClass<Syntax>>
            {
                {"-->", new SwitchToken()},
                {"(", new LeftParenthesis()},
                {")", new RightParenthesis()}
            };
            return result;
        }

        protected override TokenClass<Syntax> GetTokenClass(string name)
        {
            return new MainToken(name);
        }
        protected override TokenClass<Syntax> GetNumber() { throw new NotImplementedException(); }
        protected override TokenClass<Syntax> GetText() { throw new NotImplementedException(); }
    }

    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Instance = new PrioParser<Syntax>
            (PrioTable, Scanner(new NestedTokenFactory()));

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
                x.Correct(PrioTable.Any, PrioTable.BeginOfText, '=');
                x.Correct(")", PrioTable.BeginOfText, '=');
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override IDictionary<string, TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            var result = new Dictionary<string, TokenClass<Syntax>>
            {
                {"(", new LeftParenthesis()},
                {")", new RightParenthesis()}
            };
            return result;
        }

        protected override TokenClass<Syntax> GetTokenClass(string name)
        {
            return new NestedToken(name);
        }
        protected override TokenClass<Syntax> GetNumber() { throw new NotImplementedException(); }
        protected override TokenClass<Syntax> GetText() { throw new NotImplementedException(); }
    }

    sealed class SwitchToken : NamedToken
    {
        public SwitchToken()
            : base("-->") {}
        public override bool IsMain { get { return true; } }
        protected override Syntax Create(Syntax left, Token part, Syntax right)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }
        protected override ISubParser<Syntax> Next
        {
            get { return NestedTokenFactory.Instance.Convert(Converter); }
        }

        static IType<Syntax> Converter(Syntax arg) { return new SyntaxBoxToken(arg); }

        sealed class SyntaxBoxToken : TokenClass<Syntax>
        {
            [EnableDump]
            readonly Syntax _content;
            public SyntaxBoxToken(Syntax content) { _content = content; }
            protected override Syntax Create(Syntax left, Token part, Syntax right)
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
        protected override Syntax Create(Syntax left, Token part, Syntax right)
        {
            return new NamedTreeSyntax(left, this, part, right);
        }
    }

    sealed class MainToken : NamedToken
    {
        public MainToken(string name)
            : base(name) {}
        [DisableDump]
        public override bool IsMain { get { return true; } }
    }

    sealed class NestedToken : NamedToken
    {
        public NestedToken(string name)
            : base(name) {}

        public override bool IsMain { get { return false; } }
    }

    abstract class Syntax : ParsedSyntax, ITreeItem
    {
        protected Syntax(Token token)
            : base(token) {}
        ITreeItem ITreeItem.Left { get { return Left; } }
        public abstract Syntax Left { get; }

        string ITreeItem.TokenId { get { return TokenClassName; } }
        public abstract string TokenClassName { get; }
        public virtual bool TokenClassIsMain { get { return true; } }
        ITreeItem ITreeItem.Right { get { return Right; } }
        public abstract Syntax Right { get; }
        protected override string Dump(bool isRecursion) { return this.TreeDump(); }

        public virtual Syntax MatchedRightParenthesis(Token token)
        {
            NotImplementedMethod(token);
            return null;
        }

        public virtual Syntax CheckedRightParenthesis(Token part)
        {
            NotImplementedMethod(part);
            return null;
        }
    }

    abstract class TreeSyntax : Syntax
    {
        readonly Syntax _left;
        readonly Syntax _right;
        protected TreeSyntax(Syntax left, Token token, Syntax right)
            : base(token)
        {
            _left = left;
            _right = right;
        }
        public override sealed Syntax Left { get { return _left; } }
        public override sealed Syntax Right { get { return _right; } }
    }

    sealed class NamedTreeSyntax : TreeSyntax
    {
        readonly NamedToken _tokenClass;
        public NamedTreeSyntax(Syntax left, NamedToken tokenClass, Token part, Syntax right)
            : base(left, part, right)
        {
            _tokenClass = tokenClass;
        }
        public override string TokenClassName { get { return _tokenClass.Name; } }
        public override bool TokenClassIsMain { get { return _tokenClass.IsMain; } }
        public override Syntax MatchedRightParenthesis(Token token) { return this; }
        public override Syntax CheckedRightParenthesis(Token part) { return null; }
    }

    [DebuggerDisplay("{NodeDump}")]
    sealed class LeftParenthesisSyntax : TreeSyntax
    {
        public LeftParenthesisSyntax(Syntax left, Token part, Syntax right)
            : base(left, part, right) {}
        public override string TokenClassName { get { return "?(?"; } }
        public override bool TokenClassIsMain { get { return false; } }
        public override Syntax CheckedRightParenthesis(Token token)
        {
            if(Left == null && Right != null)
                return Right;
            return new UnNamedSyntax(Left, token, Right);
        }
    }

    sealed class RightParenthesisSyntax : TreeSyntax
    {
        public RightParenthesisSyntax(Syntax left, Token part, Syntax right)
            : base(left, part, right) {}
        public override string TokenClassName { get { return "?)?"; } }
        public override bool TokenClassIsMain { get { return false; } }
    }

    sealed class MatchedSyntax : TreeSyntax
    {
        public MatchedSyntax(Syntax left, Token part, Syntax right)
            : base(left, part, right) {}
        public override string TokenClassName { get { return "?()?"; } }
        public override bool TokenClassIsMain { get { return false; } }

        public Syntax RightParenthesis(SourcePart part)
        {
            Tracer.Assert(Left == null);
            Tracer.Assert(Right is NamedTreeSyntax);
            return Right;
        }
    }

    sealed class UnNamedSyntax : TreeSyntax
    {
        public UnNamedSyntax(Syntax left, Token part, Syntax right)
            : base(left, part, right) {}
        public override string TokenClassName { get { return ""; } }
        public override Syntax MatchedRightParenthesis(Token token) { return this; }
    }

    sealed class LeftParenthesis : TokenClass<Syntax>
    {
        protected override Syntax Create(Syntax left, Token part, Syntax right)
        {
            return new LeftParenthesisSyntax(left, part, right);
        }
    }

    sealed class EndOfText : TokenClass<Syntax>
    {
        protected override Syntax Create(Syntax left, Token part, Syntax right)
        {
            Tracer.Assert(right == null);
            return left ?? new UnNamedSyntax(null, part, null);
        }
    }

    sealed class RightParenthesis : TokenClass<Syntax>
    {
        sealed class Matched : TokenClass<Syntax>
        {
            protected override Syntax Create(Syntax left, Token part, Syntax right)
            {
                if(right == null && left != null)
                    return left.MatchedRightParenthesis(part);
                return new UnNamedSyntax(left, part, right);
            }
        }

        readonly TokenClass<Syntax> _matchedInstance;
        public RightParenthesis()
        {
            _matchedInstance = new Matched
            {
                Name = Name
            };
        }
        protected override IType<Syntax> NextTypeIfMatched { get { return _matchedInstance; } }
        protected override Syntax Create(Syntax left, Token part, Syntax right)
        {
            if(right == null && left != null)
            {
                var result = left.CheckedRightParenthesis(part);
                if(result != null)
                    return result;
            }
            return new RightParenthesisSyntax(left, part, right);
        }
    }
}