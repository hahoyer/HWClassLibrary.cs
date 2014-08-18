using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public interface IType<T>
    {
        T Create(T left, IPart<T> part, T right, bool isMatch);
        string PrioTableName { get; }
        bool IsEnd { get; }
    }
}