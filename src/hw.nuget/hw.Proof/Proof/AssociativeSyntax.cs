using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
// ReSharper disable CheckNamespace

namespace hw.Proof
{
    abstract class AssociativeSyntax : ParsedSyntax
    {
        internal readonly IAssociative Operator;
        internal readonly Set<ParsedSyntax> Set;

        protected AssociativeSyntax(IAssociative @operator, IToken token, Set<ParsedSyntax> set)
            : base(token)
        {
            Operator = @operator;
            Set = set;
        }

        [DisableDump]
        internal override sealed Set<string> Variables
        {
            get
            {
                if(Operator.IsVariablesProvider)
                    return Set<string>.Create(Set.SelectMany(target => target.Variables).ToArray());
                return new();
            }
        }

        internal override sealed bool IsDistinct(ParsedSyntax other)
        {
            return IsDistinct((AssociativeSyntax) other);
        }
        internal override string SmartDump(ISmartDumpToken @operator)
        {
            return Operator.SmartDump(Set);
        }

        bool IsDistinct(AssociativeSyntax other)
        {
            if(other.Operator != Operator)
                return true;
            return other.Set.IsDistinct(Set);
        }
    }

    interface ISmartDumpToken
    {
        string SmartDumpListDelim(ParsedSyntax parsedSyntax, bool isFirst);
        bool IsIgnoreSignSituation { get; }
    }

    interface IAssociative
    {
        bool IsVariablesProvider { get; }
        ParsedSyntax Empty { get; }
        string SmartDump(Set<ParsedSyntax> set);
        AssociativeSyntax Syntax(IToken token, Set<ParsedSyntax> target);
        ParsedSyntax Combine(ParsedSyntax left, ParsedSyntax right);
        bool IsEmpty(ParsedSyntax parsedSyntax);
    }
}