#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.PrioParser;

namespace hw.Parser
{
    abstract class TokenFactory<TTokenClass> : Dumpable, ITokenFactory
        where TTokenClass : class, IType<IParsedSyntax>, INameProvider
    {
        readonly PrioTable _prioTable;
        readonly ValueCache<FunctionCache<string, TTokenClass>> _tokenClasses;
        readonly ValueCache<TTokenClass> _number;
        readonly ValueCache<TTokenClass> _text;
        readonly ValueCache<TTokenClass> _beginOfText;
        readonly ValueCache<TTokenClass> _endOfText;

        internal TokenFactory(PrioTable prioTable)
        {
            _prioTable = prioTable;
            _endOfText = new ValueCache<TTokenClass>(InternalGetEndOfText);
            _beginOfText = new ValueCache<TTokenClass>(InternalGetBeginOfText);
            _number = new ValueCache<TTokenClass>(InternalGetNumber);
            _tokenClasses = new ValueCache<FunctionCache<string, TTokenClass>>(GetTokenClasses);
            _text = new ValueCache<TTokenClass>(InternalGetText);
        }
        TTokenClass InternalGetBeginOfText()
        {
            var result = GetBeginOfText();
            result.Name = PrioTable.BeginOfText;
            return result;
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

        PrioTable ITokenFactory.PrioTable { get { return _prioTable; } }

        IType<IParsedSyntax> ITokenFactory.TokenClass(string name) { return TokenClasses[name]; }

        IType<IParsedSyntax> ITokenFactory.Number { get { return _number.Value; } }
        IType<IParsedSyntax> ITokenFactory.Text { get { return _text.Value; } }
        IType<IParsedSyntax> ITokenFactory.EndOfText { get { return _endOfText.Value; } }

        protected abstract TTokenClass GetSyntaxError(string message);
        protected abstract FunctionCache<string, TTokenClass> GetPredefinedTokenClasses();

        protected virtual TTokenClass GetEndOfText() { return GetSyntaxError("unexpected end of text".Quote()); }
        protected virtual TTokenClass GetBeginOfText() { return GetSyntaxError("unexpected begin of text".Quote()); }
        protected virtual TTokenClass GetTokenClass(string name) { return GetSyntaxError("invalid symbol: " + name.Quote()); }
        protected virtual TTokenClass GetNumber() { return GetSyntaxError("unexpected number"); }
        protected virtual TTokenClass GetText() { return GetSyntaxError("unexpected string"); }

        FunctionCache<string, TTokenClass> TokenClasses { get { return _tokenClasses.Value; } }
        protected IType<IParsedSyntax> TokenClass(string name) { return ((ITokenFactory) this).TokenClass(name); }
    }
}