using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public interface IPosition<T, TPart>
        where T : class
    {
        Item<T, TPart> GetItemAndAdvance(Stack<OpenItem<T, TPart>> stack);
        TPart Span(IPosition<T, TPart> end);
    }
}