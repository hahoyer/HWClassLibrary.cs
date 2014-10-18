using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public static class Extension
    {
        public static T Parse<T>(this IPosition<T> current, PrioTable prioTable, Stack<OpenItem<T>> stack = null) where T : class
        {
            if(stack == null)
            {
                stack = new Stack<OpenItem<T>>();
                stack.Push(OpenItem<T>.StartItem(current));
            }

            do
            {
                var item = current.GetItemAndAdvance(stack);
                T result = null;
                do
                {
                    var topItem = stack.Peek();
                    var relation = topItem.Relation(item.Name, prioTable);

                    if(relation != '+')
                        result = stack.Pop().Create(result);

                    if(relation == '-')
                        continue;

                    if(item.IsEnd)
                        return result;

                    stack.Push(new OpenItem<T>(result, item, relation == '='));
                    result = null;
                } while(result != null);
            } while(true);
        }

        public static T Operation<T>(this IOperator<T> @operator, T left, IOperatorPart token, T right)
            where T : class
        {
            return left == null
                ? (right == null ? @operator.Terminal(token) : @operator.Prefix(token, right))
                : (right == null ? @operator.Suffix(left, token) : @operator.Infix(left, token, right));
        }
    }
}