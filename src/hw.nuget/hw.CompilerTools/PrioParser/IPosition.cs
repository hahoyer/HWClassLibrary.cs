
using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public interface IPosition<T>
        where T : class
    {
        Item<T> GetItemAndAdvance(Stack<OpenItem<T>> stack);
        IPart Span(IPosition<T> end);
    }
}