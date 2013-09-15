#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2012 - 2013 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Parser;

namespace Reni.Parser
{
    abstract class Scanner : Dumpable
    {
        protected abstract int WhiteSpace(SourcePosn sourcePosn);
        protected abstract int? Number(SourcePosn sourcePosn);
        protected abstract int? Text(SourcePosn sourcePosn);
        protected abstract int? Any(SourcePosn sourcePosn);

        internal Item<IParsedSyntax> CreateToken(SourcePosn sourcePosn, ITokenFactory tokenFactory)
        {
            try
            {
                sourcePosn.Position += WhiteSpace(sourcePosn);
                return CreateAndAdvance(sourcePosn, sp => sp.IsEnd ? (int?) 0 : null, tokenFactory.EndOfText)
                    ?? CreateAndAdvance(sourcePosn, Number, tokenFactory.Number)
                    ?? CreateAndAdvance(sourcePosn, Text, tokenFactory.Text)
                    ?? CreateAndAdvance(sourcePosn, Any, tokenFactory.TokenClass)
                    ?? WillReturnNull(sourcePosn);
            }
            catch(Exception exception)
            {
                return CreateAndAdvance(exception.SourcePosn, sp => exception.Length, exception.TokenClass);
            }
        }
        Item<IParsedSyntax> WillReturnNull(SourcePosn sourcePosn)
        {
            NotImplementedMethod(sourcePosn);
            return null;
        }

        internal sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            public readonly IType<IParsedSyntax> TokenClass;
            public readonly int Length;

            public Exception(SourcePosn sourcePosn, IType<IParsedSyntax> tokenClass, int length)
            {
                SourcePosn = sourcePosn;
                TokenClass = tokenClass;
                Length = length;
            }
        }

        static Item<IParsedSyntax> CreateAndAdvance(SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, IType<IParsedSyntax> tokenClass) { return CreateAndAdvance(sourcePosn, getLength, (sp, l) => tokenClass); }
        static Item<IParsedSyntax> CreateAndAdvance(SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<string, IType<IParsedSyntax>> getTokenClass) { return CreateAndAdvance(sourcePosn, getLength, (sp, l) => getTokenClass(sp.SubString(0, l))); }
       
        static Item<IParsedSyntax> CreateAndAdvance(SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<SourcePosn, int, IType<IParsedSyntax>> getTokenClass)
        {
            var length = getLength(sourcePosn);
            if(length == null)
                return null;

            var result = new Item<IParsedSyntax>
                (
                getTokenClass(sourcePosn, length.Value), 
                TokenData.Span(sourcePosn, length.Value  )
                );
            sourcePosn.Position += length.Value;
            return result;
        }
    }
}