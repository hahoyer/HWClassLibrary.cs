using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    abstract class TokenFactory<TTokenClass, TTreeItem> : Dumpable, ITokenFactory<TTreeItem>
        where TTokenClass : class, Scanner<TTreeItem>.IType, IUniqueIdProvider
        where TTreeItem : class, ISourcePart
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
            return new FunctionCache<string, TTokenClass>
                (ScanPredefinedTokenClasses(), GetTokenClass);
        }

        IDictionary<string, TTokenClass> ScanPredefinedTokenClasses()
        {
            return GetPredefinedTokenClasses().ToDictionary(item => item.Value);
        }

        Scanner<TTreeItem>.IType ITokenFactory<TTreeItem>.TokenClass(string name)
        {
            return TokenClass(name);
        }

        Scanner<TTreeItem>.IType ITokenFactory<TTreeItem>.Number { get { return _number.Value; } }
        Scanner<TTreeItem>.IType ITokenFactory<TTreeItem>.Text { get { return _text.Value; } }
        Scanner<TTreeItem>.IType ITokenFactory<TTreeItem>.EndOfText { get { return _endOfText.Value; } }
        Scanner<TTreeItem>.IType ITokenFactory<TTreeItem>.Error(Match.IError error)
        {
            return GetError(error);
        }

        protected abstract TTokenClass GetError(Match.IError message);
        protected abstract IEnumerable<TTokenClass> GetPredefinedTokenClasses();
        protected abstract TTokenClass GetEndOfText();
        protected abstract TTokenClass GetTokenClass(string name);
        protected abstract TTokenClass GetNumber();
        protected abstract TTokenClass GetText();

        FunctionCache<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
        protected Scanner<TTreeItem>.IType TokenClass(string name) { return TokenClasses[name]; }
    }
}