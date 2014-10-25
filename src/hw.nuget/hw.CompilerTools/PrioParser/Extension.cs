using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.PrioParser
{
    public static class Extension
    {
        public static T Parse<T, TPart>
            (this IPosition<T, TPart> current, PrioTable prioTable, Stack<OpenItem<T, TPart>> stack = null)
            where T : class
        {
            if(stack == null)
            {
                stack = new Stack<OpenItem<T, TPart>>();
                stack.Push(OpenItem<T, TPart>.StartItem(current));
            }

            else
            {
                
            }

            var startLevel = stack.Count;

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

                    if(startLevel > stack.Count)
                        return result;

                    stack.Push(new OpenItem<T, TPart>(result, item, relation == '='));
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