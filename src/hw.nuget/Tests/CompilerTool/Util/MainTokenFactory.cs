using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
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
            public override string Id { get { return "<error>"; } }

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
            {
                return new ErrorSyntax(left, token, right, _message);
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
                x += PrioTable.BracketPair("(", ")");
                x += PrioTable.Left(";");
                x += PrioTable.BracketPair(PrioTable.BeginOfText, PrioTable.EndOfText);
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override IEnumerable<TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            return new TokenClass<Syntax>[]
            {
                new SwitchToken(),
                new LeftParenthesis(),
                new RightParenthesis()
            };
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
                x += PrioTable.BracketParallels
                    (new[] {"(", PrioTable.BeginOfText}, new[] {")", PrioTable.EndOfText});
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override IEnumerable<TokenClass<Syntax>> GetPredefinedTokenClasses()
        {
            return new TokenClass<Syntax>[]
            {
                new LeftParenthesis(),
                new RightParenthesis()
            };
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

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
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
            public override string Id { get { return "<box>"; } }

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
            {
                Tracer.Assert(left == null);
                Tracer.Assert(right == null);
                return _content;
            }
        }
    }

    abstract class NamedToken : TokenClass<Syntax>
    {
        internal readonly string Name;
        protected NamedToken(string name) { Name = name; }
        public abstract bool IsMain { get; }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            return new NamedTreeSyntax(left, this, token, right);
        }

        public override string Id { get { return Name; } }
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

    abstract class Syntax : ParsedSyntax, IBinaryTreeItem
    {
        protected Syntax(IToken token)
            : base(token) {}

        IBinaryTreeItem IBinaryTreeItem.Left { get { return Left; } }
        public abstract Syntax Left { get; }

        string IBinaryTreeItem.TokenId { get { return TokenClassName; } }
        public abstract string TokenClassName { get; }
        public virtual bool TokenClassIsMain { get { return true; } }
        IBinaryTreeItem IBinaryTreeItem.Right { get { return Right; } }
        public abstract Syntax Right { get; }
        protected override string Dump(bool isRecursion) { return this.TreeDump(); }

        public virtual Syntax MatchedRightParenthesis(IToken token)
        {
            NotImplementedMethod(token);
            return null;
        }

        public virtual Syntax CheckedRightParenthesis(IToken part)
        {
            NotImplementedMethod(part);
            return null;
        }
    }

    abstract class TreeSyntax : Syntax
    {
        readonly Syntax _left;
        readonly Syntax _right;

        protected TreeSyntax(Syntax left, IToken token, Syntax right)
            : base(token)
        {
            _left = left;
            _right = right;
        }

        public override sealed Syntax Left { get { return _left; } }
        public override sealed Syntax Right { get { return _right; } }
    }

    class ErrorSyntax : TreeSyntax
    {
        readonly Match.IError _message;

        public ErrorSyntax(Syntax left, IToken token, Syntax right, Match.IError message)
            : base(left, token, right)
        {
            _message = message;
        }

        public override string TokenClassName { get { return "?" + _message; } }
    }

    sealed class NamedTreeSyntax : TreeSyntax
    {
        readonly NamedToken _tokenClass;

        public NamedTreeSyntax(Syntax left, NamedToken tokenClass, IToken part, Syntax right)
            : base(left, part, right)
        {
            _tokenClass = tokenClass;
        }

        public override string TokenClassName { get { return _tokenClass.Name; } }
        public override bool TokenClassIsMain { get { return _tokenClass.IsMain; } }
        public override Syntax MatchedRightParenthesis(IToken token) { return this; }
        public override Syntax CheckedRightParenthesis(IToken part) { return null; }
    }

    [DebuggerDisplay("{NodeDump}")]
    sealed class LeftParenthesisSyntax : TreeSyntax
    {
        public LeftParenthesisSyntax(Syntax left, IToken part, Syntax right)
            : base(left, part, right) {}

        public override string TokenClassName { get { return "?(?"; } }
        public override bool TokenClassIsMain { get { return false; } }

        public override Syntax CheckedRightParenthesis(IToken token)
        {
            if(Left == null && Right != null)
                return Right;
            return new UnNamedSyntax(Left, token, Right);
        }
    }

    sealed class RightParenthesisSyntax : TreeSyntax
    {
        public RightParenthesisSyntax(Syntax left, IToken part, Syntax right)
            : base(left, part, right) {}

        public override string TokenClassName { get { return "?)?"; } }
        public override bool TokenClassIsMain { get { return false; } }
    }

    sealed class MatchedSyntax : TreeSyntax
    {
        public MatchedSyntax(Syntax left, IToken part, Syntax right)
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
        public UnNamedSyntax(Syntax left, IToken part, Syntax right)
            : base(left, part, right) {}

        public override string TokenClassName { get { return ""; } }
        public override Syntax MatchedRightParenthesis(IToken token) { return this; }
    }

    sealed class LeftParenthesis : TokenClass<Syntax>
    {
        public override string Id { get { return "("; } }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            return new LeftParenthesisSyntax(left, token, right);
        }
    }

    sealed class EndOfText : TokenClass<Syntax>
    {
        public override string Id { get { return PrioTable.EndOfText; } }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return left ?? new UnNamedSyntax(null, token, null);
        }
    }

    sealed class RightParenthesis : TokenClass<Syntax>
    {
        sealed class Matched : TokenClass<Syntax>
        {
            public override string Id { get { return PrioTable.Any; } }

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
            {
                if(right == null && left != null)
                    return left.MatchedRightParenthesis(token);
                return new UnNamedSyntax(left, token, right);
            }
        }

        readonly TokenClass<Syntax> _matchedInstance;
        public RightParenthesis() { _matchedInstance = new Matched(); }
        public override string Id { get { return ")"; } }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            if(right == null && left != null)
            {
                var result = left.CheckedRightParenthesis(token);
                if(result != null)
                    return result;
            }
            return new RightParenthesisSyntax(left, token, right);
        }
    }
}