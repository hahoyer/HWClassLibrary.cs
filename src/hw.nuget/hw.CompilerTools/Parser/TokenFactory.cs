using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class TokenFactory<TTokenClass, TTreeItem> : Dumpable, ITokenFactory<TTreeItem>
        where TTokenClass : class, IType<TTreeItem>, INameProvider
        where TTreeItem : class
    {
        readonly ValueCache<FunctionCache<string, TTokenClass>> _tokenClasses;
        readonly ValueCache<TTokenClass> _number;
        readonly ValueCache<TTokenClass> _text;
        readonly ValueCache<TTokenClass> _endOfText;

        internal TokenFactory()
        {
            _endOfText = new ValueCache<TTokenClass>(InternalGetEndOfText);
            _number = new ValueCache<TTokenClass>(InternalGetNumber);
            _tokenClasses = new ValueCache<FunctionCache<string, TTokenClass>>(GetTokenClasses);
            _text = new ValueCache<TTokenClass>(InternalGetText);
        }
        TTokenClass InternalGetEndOfText()
        {
            var result = GetEndOfText();
            result.Name = PrioTable.EndOfText;
            return result;
        }

        FunctionCache<string, TTokenClass> GetTokenClasses()
        {
            var result = new FunctionCache<string, TTokenClass>(GetPredefinedTokenClasses(), InternalGetTokenClass);
            foreach(var pair in result)
                pair.Value.Name = pair.Key;
            return result;
        }

        TTokenClass InternalGetTokenClass(string name)
        {
            var result = GetTokenClass(name);
            result.Name = name;
            return result;
        }

        TTokenClass InternalGetNumber()
        {
            var result = GetNumber();
            result.Name = "<number>";
            return result;
        }

        TTokenClass InternalGetText()
        {
            var result = GetText();
            result.Name = "<Text>";
            return result;
        }

        IType<TTreeItem> ITokenFactory<TTreeItem>.TokenClass(string name) { return TokenClass(name); }

        IType<TTreeItem> ITokenFactory<TTreeItem>.Number { get { return _number.Value; } }
        IType<TTreeItem> ITokenFactory<TTreeItem>.Text { get { return _text.Value; } }
        IType<TTreeItem> ITokenFactory<TTreeItem>.EndOfText { get { return _endOfText.Value; } }
        IType<TTreeItem> ITokenFactory<TTreeItem>.Error(Match.IError error) { return GetSyntaxError(error.ToString()); }

        protected abstract TTokenClass GetSyntaxError(string message);
        protected abstract FunctionCache<string, TTokenClass> GetPredefinedTokenClasses();

        protected virtual TTokenClass GetEndOfText() { return GetSyntaxError("unexpected end of text".Quote()); }
        protected virtual TTokenClass GetTokenClass(string name) { return GetSyntaxError("invalid symbol: " + name.Quote()); }
        protected virtual TTokenClass GetNumber() { return GetSyntaxError("unexpected number"); }
        protected virtual TTokenClass GetText() { return GetSyntaxError("unexpected string"); }

        FunctionCache<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
        protected IType<TTreeItem> TokenClass(string name) { return TokenClasses[name]; }
    }
}