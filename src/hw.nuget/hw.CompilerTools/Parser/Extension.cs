using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public static class Extension
    {
        public static TTreeItem Operation<TTreeItem>
            (this IOperator<TTreeItem> @operator, TTreeItem left, SourcePart token, TTreeItem right) where TTreeItem : class
        {
            return left == null
                ? (right == null ? @operator.Terminal(token) : @operator.Prefix(token, right))
                : (right == null ? @operator.Suffix(left, token) : @operator.Infix(left, token, right));
        }

        public static ISubParser<TTreeItem> Convert<TTreeItem>
            (this IParser<TTreeItem> parser, Func<TTreeItem, IType<TTreeItem>> converter) where TTreeItem : class
        {
            return new SubParser<TTreeItem>(parser, converter);
        }
    }
}