using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    public static class Extension
    {
        public static TTreeItem Operation<TTreeItem>
            (this IOperator<TTreeItem> @operator, TTreeItem left, IToken token, TTreeItem right) where TTreeItem : class
        {
            return left == null
                ? (right == null ? @operator.Terminal(token) : @operator.Prefix(token, right))
                : (right == null ? @operator.Suffix(left, token) : @operator.Infix(left, token, right));
        }

        public static ISubParser<TTreeItem> Convert<TTreeItem>
            (this IParser<TTreeItem> parser, Func<TTreeItem, IType<TTreeItem>> converter) where TTreeItem : class, ISourcePart {
            return new SubParser<TTreeItem>(parser, converter);
        }
        
        internal static string TreeDump<TTreeItem>(TTreeItem value) where TTreeItem : class
        {
            var t = value as IBinaryTreeItem;
            if(t == null)
                return Tracer.Dump(value);

            return TreeDump(t);
        }
        
        public static string TreeDump(this IBinaryTreeItem value)
        {
            if(value == null)
                return "<null>";

            var result = "(";
            result += TreeDump(value.Left);
            result += " ";
            result += value.TokenId;
            result += " ";
            result += TreeDump(value.Right);
            result += ")";
            return result;
        }
    }
}