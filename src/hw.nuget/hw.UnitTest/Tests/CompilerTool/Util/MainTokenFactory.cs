using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Parser;
using hw.PrioParser;
using hw.Proof.TokenClasses;
using hw.Scanner;

namespace hw.Tests.CompilerTool.Util
{
    abstract class TokenFactory : TokenFactory<TokenClass<TokenData>>
    {
        public static readonly hw.Parser.Scanner Scanner = new Scanner();

        internal TokenFactory(PrioTable prioTable)
            : base(prioTable)
        {}
        protected override TokenClass<TokenData> GetSyntaxError(string message) { return new SyntaxError(message); }
        protected override TokenClass<TokenData> GetEndOfText() { return new EndOfTextToken(); }
        protected override TokenClass<TokenData> GetBeginOfText() { return new BeginOfTextToken(); }

        sealed class SyntaxError : TokenClass<TokenData>
        {
            [EnableDump]
            readonly string _message;
            public SyntaxError(string message) { _message = message; }
            protected override IParsedSyntax Create(IParsedSyntax left, TokenData part, IParsedSyntax right)
            {
                NotImplementedMethod(left, part, right);
                return null;
            }
        }
    }

    sealed class MainTokenFactory : TokenFactory
    {
        public static readonly MainTokenFactory Instance = new MainTokenFactory();

        MainTokenFactory()
            : base(PrioTable) { }
       
        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);
                x = x.ParenthesisLevel(new[] { "(", "[", "{", PrioTable.BeginOfText }, new[] { ")", "]", "}", PrioTable.EndOfText });
                Tracer.FlaggedLine("\n" + x + "\n");
                return x;
            }
        }

        protected override FunctionCache<string, TokenClass<TokenData>> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass<TokenData>> {{"-->", new SwitchToken()}};
            return result;
        }

        protected override TokenClass<TokenData> GetTokenClass(string name) { return new MainToken(name); }
    }

    sealed class NestedTokenFactory : TokenFactory
    {
        public static readonly NestedTokenFactory Instance = new NestedTokenFactory();

        NestedTokenFactory()
            : base(PrioTable) { }
       
        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left("-->", PrioTable.Any);
                x = x.ParenthesisLevel(new[] { "(", "[", "{", PrioTable.BeginOfText }, new[] { ")", "]", "}", PrioTable.EndOfText });
                x.Correct("-->", "-->", '=');
                Tracer.FlaggedLine("\n" + x + "\n");
                return x;
            }
        }

        protected override FunctionCache<string, TokenClass<TokenData>> GetPredefinedTokenClasses()
        {
            var result = new FunctionCache<string, TokenClass<TokenData>>();
            return result;
        }

        protected override TokenClass<TokenData> GetTokenClass(string name) { return new NestedToken(name); }
    }

    sealed class SwitchToken : TokenClass<TokenData>, ISubParser
    {
        protected override IParsedSyntax Create(IParsedSyntax left, TokenData part, IParsedSyntax right)
        {
            Tracer.Assert(right == null);
            return left;
        }
        Item<IParsedSyntax, TokenData> ISubParser.Execute
            (TokenData part, SourcePosn sourcePosn, Stack<OpenItem<IParsedSyntax, TokenData>> stack)
        {
            stack.Push(OpenItem<IParsedSyntax, TokenData>.StartItem(this, part));
            var result = (Syntax)Position.Parse(sourcePosn, NestedTokenFactory.Instance, TokenFactory.Scanner, stack);

            Tracer.Assert(result.Left == null);
            Tracer.Assert(result.Right == null);
            return new Item<IParsedSyntax, TokenData>(result.TokenClass,result.Part);
        }
    }

    sealed class BeginOfTextToken : TokenClass<TokenData>
    {
        protected override bool AcceptsMatch { get { return true; } }
        protected override IParsedSyntax Create(IParsedSyntax left, TokenData part, IParsedSyntax right)
        {
            NotImplementedMethod(left, part, right);
            return null;
        }
    }

    abstract class NamedToken : TokenClass<TokenData>
    {
        [EnableDump]
        readonly string _name;
        protected NamedToken(string name) { _name = name; }
        public abstract bool IsMain { get; }
        protected override IParsedSyntax Create(IParsedSyntax left, TokenData part, IParsedSyntax right)
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

    sealed class NestedToken : NamedToken, ISubParser
    {
        public NestedToken(string name)
            : base(name)
        { }
        public override bool IsMain { get { return false; } }
        Item<IParsedSyntax, TokenData> ISubParser.Execute
            (TokenData part, SourcePosn sourcePosn, Stack<OpenItem<IParsedSyntax, TokenData>> stack)
        {
            var t = stack.Pop();
            stack.Push(new OpenItem<IParsedSyntax, TokenData>(new Syntax((Syntax) t.Left, this, part, null), t.Item, false));
            return t.Item;
        }
    }

    sealed class Syntax : DumpableObject, IParsedSyntax
    {
        public readonly Syntax Left;
        public readonly NamedToken TokenClass;
        internal readonly TokenData Part;
        public readonly Syntax Right;

        public Syntax(Syntax left, NamedToken tokenClass, TokenData part, Syntax right)
        {
            Left = left;
            TokenClass = tokenClass;
            Part = part;
            Right = right;
        }
        string IIconKeyProvider.IconKey { get { return "Symbol"; } }
        TokenData IParsedSyntax.Token { get { return Part; } }
        TokenData IParsedSyntax.FirstToken { get { return Left == null ? Part : ((IParsedSyntax) Left).FirstToken; } }
        TokenData IParsedSyntax.LastToken { get { return Right == null ? Part : ((IParsedSyntax) Right).LastToken; } }
        string IParsedSyntax.GetNodeDump() { return GetNodeDump(); }
    }

    sealed class EndOfTextToken : TokenClass<TokenData>
    {
        protected override IParsedSyntax Create(IParsedSyntax left, TokenData part, IParsedSyntax right)
        {
            Tracer.Assert(right == null);
            return left;
        }
        protected override bool IsEnd { get { return true; } }
    }
}