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
                x += PrioTable.Left(";");
                x += PrioTable.BracketPair("(", ")");
                x += PrioTable.BracketPair("{", "}");
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
                new LeftParenthesis("("),
                new RightParenthesis(")"),
                new LeftParenthesis("{"),
                new RightParenthesis("}")
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
                SwitchToken.Instance,
                new LeftParenthesis("("),
                new RightParenthesis(")")
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
        public static readonly SwitchToken Instance = new SwitchToken();

        public SwitchToken()
            : base("-->") {}

        public override bool IsMain { get { return true; } }

        protected override Syntax Create(Syntax left, IToken token, Syntax right) { return null; }

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

        public abstract Syntax RightParenthesis(IToken token, Syntax right);
    }

    abstract class TreeSyntax : Syntax
    {
        protected TreeSyntax(Syntax left, IToken token, Syntax right)
            : base(token)
        {
            Left = left;
            Right = right;
        }

        public sealed override Syntax Left { get; }
        public sealed override Syntax Right { get; }
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

        public override Syntax RightParenthesis(IToken token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }
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

        public override Syntax RightParenthesis(IToken token, Syntax right)
        {
            return new RightParenthesisSyntax(this, token, right);
            NotImplementedMethod(token, right);
            return null;
        }
    }

    [DebuggerDisplay("{NodeDump}")]
    sealed class RightParenthesisSyntax : TreeSyntax
    {
        public readonly LeftParenthesis TokenClass;

        public RightParenthesisSyntax(Syntax left, IToken part, Syntax right)
            : base(left, part, right) {}

        public override string TokenClassName { get { return "?)?"; } }
        public override bool TokenClassIsMain { get { return false; } }

        public override Syntax RightParenthesis(IToken token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }
    }

    [DebuggerDisplay("{NodeDump}")]
    sealed class LeftParenthesisSyntax : TreeSyntax
    {
        public readonly LeftParenthesis TokenClass;

        public LeftParenthesisSyntax
            (LeftParenthesis tokenClass, Syntax left, IToken part, Syntax right)
            : base(left, part, right)
        {
            TokenClass = tokenClass;
        }

        public override string TokenClassName { get { return "?(?"; } }
        public override bool TokenClassIsMain { get { return false; } }

        public override Syntax RightParenthesis(IToken token, Syntax right)
        {
            if("({".Substring(")}".IndexOf(token.Id), 1) != TokenClass.Id)
            {
                if(right == null)
                    return this;

                NotImplementedMethod(token, right);
                return null;
            }

            if(right == null && Left == null)
                return new ParenthesisSyntax(Token, Right, token);

            NotImplementedMethod(token, right);
            return null;
        }
    }

    sealed class ParenthesisSyntax : TreeSyntax
    {
        internal readonly IToken Leftpart;

        public ParenthesisSyntax(IToken leftpart, Syntax middle, IToken rightPart)
            : base(middle, rightPart, null)
        {
            Leftpart = leftpart;
        }

        public override string TokenClassName => "()";

        public override Syntax RightParenthesis(IToken token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }
    }

    sealed class NamelessSyntax : TreeSyntax
    {
        public NamelessSyntax(Syntax left, IToken token, Syntax right)
            : base(left, token, right) {}

        public override string TokenClassName => "<nameless>";

        public override Syntax RightParenthesis(IToken token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }
    }

    sealed class LeftParenthesis : TokenClass<Syntax>
    {
        public LeftParenthesis(string id) { Id = id; }
        public override string Id { get; }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            return new LeftParenthesisSyntax(this, left, token, right);
        }
    }

    sealed class EndOfText : TokenClass<Syntax>
    {
        public override string Id { get { return PrioTable.EndOfText; } }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return left;
        }
    }

    sealed class RightParenthesis : TokenClass<Syntax>, IBracketMatch<Syntax>
    {
        public RightParenthesis(string id) { Id = id; }
        public override string Id { get; }

        sealed class Matched : TokenClass<Syntax>
        {
            protected override Syntax Create(Syntax left, IToken token, Syntax right)
                => right == null ? left : new NamelessSyntax(left, token, right);

            public override string Id => "()";
        }

        IType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            if(left != null)
                return left.RightParenthesis(token, right);
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}