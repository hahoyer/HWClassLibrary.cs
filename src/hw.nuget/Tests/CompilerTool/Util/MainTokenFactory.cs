using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : TokenFactory<TokenClass<Syntax>, Syntax>
    {
        protected static IScanner<Syntax> Scanner(TokenFactory t)
            => new Scanner<Syntax>(new ReniLexer(), t);

        protected override TokenClass<Syntax> GetError(Match.IError message)
            => new SyntaxError(message);

        protected override TokenClass<Syntax> GetEndOfText() => new EndOfText();

        sealed class SyntaxError : TokenClass<Syntax>
        {
            [EnableDump]
            readonly Match.IError _message;
            public SyntaxError(Match.IError message) { _message = message; }
            public override string Id => "<error>";

            protected override Syntax Create(Syntax left, IToken token, Syntax right)
                => new ErrorSyntax(left, token, right, _message);
        }
    }

    sealed class MainTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Instance = new PrioParser<Syntax>
            (PrioTable, Scanner(new MainTokenFactory()), new BeginOfText());

        MainTokenFactory() { }

        internal static string[] RightBrackets => new[] {")", "}", PrioTable.EndOfText};
        internal static string[] LeftBrackets => new[] {"(", "{", PrioTable.BeginOfText};

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.Left("*");
                x += PrioTable.Left("+");
                x += PrioTable.Left(";");
                x += PrioTable.BracketParallels(LeftBrackets, RightBrackets);
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override IEnumerable<TokenClass<Syntax>> GetPredefinedTokenClasses()
            => new TokenClass<Syntax>[]
            {
                new SwitchToken(),
                new LeftParenthesis("("),
                new RightParenthesis(")"),
                new LeftParenthesis("{"),
                new RightParenthesis("}")
            };

        protected override TokenClass<Syntax> GetTokenClass(string name) => new MainToken(name);

        protected override TokenClass<Syntax> GetNumber() { throw new NotImplementedException(); }
        protected override TokenClass<Syntax> GetText() { throw new NotImplementedException(); }
    }


    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly IParser<Syntax> Instance = new PrioParser<Syntax>
            (PrioTable, Scanner(new NestedTokenFactory()), new BeginOfText());

        NestedTokenFactory() { }

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x += PrioTable.BracketParallels
                    (MainTokenFactory.LeftBrackets, MainTokenFactory.RightBrackets);
                Tracer.FlaggedLine("\n" + x.Dump() + "\n");
                x.Title = Tracer.MethodHeader();
                return x;
            }
        }

        protected override IEnumerable<TokenClass<Syntax>> GetPredefinedTokenClasses()
            => new TokenClass<Syntax>[]
            {
                SwitchToken.Instance,
                new LeftParenthesis("("),
                new RightParenthesis(")")
            };

        protected override TokenClass<Syntax> GetTokenClass(string name) => new NestedToken(name);

        protected override TokenClass<Syntax> GetNumber() { throw new NotImplementedException(); }
        protected override TokenClass<Syntax> GetText() { throw new NotImplementedException(); }
    }

    sealed class SwitchToken : NamedToken
    {
        public static readonly SwitchToken Instance = new SwitchToken();

        public SwitchToken()
            : base("-->") {}

        public override bool IsMain => true;

        protected override Syntax Create(Syntax left, IToken token, Syntax right) => null;

        protected override ISubParser<Syntax> Next => NestedTokenFactory.Instance.Convert(Converter)
            ;

        static IType<Syntax> Converter(Syntax arg) => new SyntaxBoxToken(arg);

        sealed class SyntaxBoxToken : TokenClass<Syntax>
        {
            [EnableDump]
            readonly Syntax _content;
            public SyntaxBoxToken(Syntax content) { _content = content; }
            public override string Id => "<box>";

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
            => new NamedTreeSyntax(left, this, token, right);

        public override string Id => Name;
    }

    sealed class MainToken : NamedToken
    {
        public MainToken(string name)
            : base(name) {}

        [DisableDump]
        public override bool IsMain => true;
    }

    sealed class NestedToken : NamedToken
    {
        public NestedToken(string name)
            : base(name) {}

        public override bool IsMain => false;
    }

    abstract class Syntax : ParsedSyntax, IBinaryTreeItem
    {
        protected Syntax(IToken token)
            : base(token) {}

        IBinaryTreeItem IBinaryTreeItem.Left => Left;
        public abstract Syntax Left { get; }

        string IBinaryTreeItem.TokenId => TokenClassName;
        public abstract string TokenClassName { get; }
        public virtual bool TokenClassIsMain => true;
        IBinaryTreeItem IBinaryTreeItem.Right => Right;
        public abstract Syntax Right { get; }

        public virtual IEnumerable<IToken> Tokens { get { yield return Token; } }

        protected override string Dump(bool isRecursion) => this.TreeDump();

        public abstract Syntax RightParenthesis(string id, IToken token, Syntax right);
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

        public override IEnumerable<IToken> Tokens
        {
            get
            {
                var result = Enumerable.Empty<IToken>();

                if(Left != null)
                    result = result.Concat(Left.Tokens);
                result = result.Concat(base.Tokens);
                if(Right != null)
                    result = result.Concat(Right.Tokens);
                return result;
            }
        }
    }

    sealed class ErrorSyntax : TreeSyntax
    {
        readonly Match.IError _message;

        public ErrorSyntax(Syntax left, IToken token, Syntax right, Match.IError message)
            : base(left, token, right)
        {
            _message = message;
        }

        public override string TokenClassName => "?" + _message;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
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

        public override string TokenClassName => _tokenClass.Name;
        public override bool TokenClassIsMain => _tokenClass.IsMain;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
            => new RightParenthesisSyntax(id, this, token, right);
    }

    [DebuggerDisplay("{NodeDump}")]
    sealed class RightParenthesisSyntax : TreeSyntax
    {
        readonly string Id;

        public RightParenthesisSyntax(string id, Syntax left, IToken part, Syntax right)
            : base(left, part, right)
        {
            Id = id;
        }

        public override string TokenClassName => "?)?";
        public override bool TokenClassIsMain => false;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }

    [DebuggerDisplay("{NodeDump}")]
    sealed class LeftParenthesisSyntax : TreeSyntax
    {
        public LeftParenthesisSyntax
            (Syntax left, IToken part, Syntax right)
            : base(left, part, right) {}

        public override string TokenClassName => "<(>";
        public override bool TokenClassIsMain => false;

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            if(right == null && Left == null)
                return new ParenthesisSyntax(Token, Right, token);

            NotImplementedMethod(id, token, right);
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

        public override string TokenClassName => IsValid ? "()" : "?(?";

        public override IEnumerable<IToken> Tokens => new[] {Leftpart}.Concat(base.Tokens);

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }

        internal bool IsValid
        {
            get
            {
                var left = Leftpart.Id;
                if(left == "")
                    left = PrioTable.BeginOfText;

                var right = Token.Id;
                if(right == "")
                    right = PrioTable.EndOfText;

                var index = MainTokenFactory.RightBrackets
                    .IndexWhere(item =>
                    {
                        return item == right;
                    })
                    .AssertValue();
                return MainTokenFactory.LeftBrackets[index] == left;
            }
        }
    }

    sealed class NamelessSyntax : TreeSyntax
    {
        public NamelessSyntax(Syntax left, IToken token, Syntax right)
            : base(left, token, right) {}

        public override string TokenClassName => "<nameless>";

        public override Syntax RightParenthesis(string id, IToken token, Syntax right)
        {
            NotImplementedMethod(id, token, right);
            return null;
        }
    }

    sealed class LeftParenthesis : TokenClass<Syntax>
    {
        public LeftParenthesis(string id) { Id = id; }
        public override string Id { get; }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => new LeftParenthesisSyntax(left, token, right);
    }

    sealed class BeginOfText : TokenClass<Syntax>
    {
        public override string Id => PrioTable.BeginOfText;

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => new LeftParenthesisSyntax(left, token, right);
    }

    sealed class EndOfText : TokenClass<Syntax>, IBracketMatch<Syntax>
    {
        public override string Id => PrioTable.EndOfText;

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            if(left != null)
                return left.RightParenthesis(Id, token, right);
            NotImplementedMethod(left, token, right);
            return null;
        }

        sealed class Matched : TokenClass<Syntax>
        {
            protected override Syntax Create(Syntax left, IToken token, Syntax right)
            {
                if(right == null)
                {
                    var result = (ParenthesisSyntax)left;
                    if(result.Right == null)
                        return result.Left;

                    NotImplementedMethod(left, token, right);
                    return null;
                }

                NotImplementedMethod(left, token, right);
                return null;
            }

            public override string Id => "<source>";
        }

        IType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
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
                return left.RightParenthesis(Id, token, right);
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}