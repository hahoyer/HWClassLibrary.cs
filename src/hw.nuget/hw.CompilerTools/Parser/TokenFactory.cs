using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class TokenFactory<TTokenClass, TTreeItem> : Dumpable, ITokenFactory<TTreeItem>
        where TTokenClass : class, IType<TTreeItem>
        where TTreeItem : class
    {
        readonly ValueCache<FunctionCache<string, TTokenClass>> _tokenClasses;
        readonly ValueCache<TTokenClass> _number;
        readonly ValueCache<TTokenClass> _text;
        readonly ValueCache<TTokenClass> _endOfText;

        internal TokenFactory()
        {
            _endOfText = new ValueCache<TTokenClass>(GetEndOfText);
            _number = new ValueCache<TTokenClass>(GetNumber);
            _tokenClasses = new ValueCache<FunctionCache<string, TTokenClass>>(GetTokenClasses);
            _text = new ValueCache<TTokenClass>(GetText);
        }

        FunctionCache<string, TTokenClass> GetTokenClasses()
        {
            return new FunctionCache<string, TTokenClass>(GetPredefinedTokenClasses(), GetTokenClass);
        }

        IType<TTreeItem> ITokenFactory<TTreeItem>.TokenClass(string name) { return TokenClass(name); }

        IType<TTreeItem> ITokenFactory<TTreeItem>.Number { get { return _number.Value; } }
        IType<TTreeItem> ITokenFactory<TTreeItem>.Text { get { return _text.Value; } }
        IType<TTreeItem> ITokenFactory<TTreeItem>.EndOfText { get { return _endOfText.Value; } }
        IType<TTreeItem> ITokenFactory<TTreeItem>.Error(Match.IError error) { return GetError(error); }

        protected abstract TTokenClass GetError(Match.IError message);
        protected abstract IDictionary<string, TTokenClass> GetPredefinedTokenClasses();
        protected abstract TTokenClass GetEndOfText();
        protected abstract TTokenClass GetTokenClass(string name);
        protected abstract TTokenClass GetNumber();
        protected abstract TTokenClass GetText();

        FunctionCache<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
        protected IType<TTreeItem> TokenClass(string name) { return TokenClasses[name]; }
    }

}