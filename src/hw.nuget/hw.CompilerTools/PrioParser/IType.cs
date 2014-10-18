using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.PrioParser
{
    public interface IType<T> : IType
    {
        T Create(T left, IPart part, T right, bool isMatch);
        string PrioTableName { get; }
        bool IsEnd { get; }
    }

    public interface IType
    {}

    interface IRescannable<T>
        where T : class
    {
        Item<T> Execute(IPart part, SourcePosn sourcePosn, Stack<OpenItem<T>> stack);
    }
}