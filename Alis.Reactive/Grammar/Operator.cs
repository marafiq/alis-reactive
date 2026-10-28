using System.Collections.Generic;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// The closed operator table (grammar.md 2.2). Each operator gives the answer C# gives for the
    /// developer's expression over the converted values.
    /// </summary>
    internal sealed class Operator
    {
        private static readonly List<Operator> All = new List<Operator>();

        internal static readonly Operator Eq = Define("eq");
        internal static readonly Operator Neq = Define("neq");
        internal static readonly Operator Lt = Define("lt");
        internal static readonly Operator Lte = Define("lte");
        internal static readonly Operator Gt = Define("gt");
        internal static readonly Operator Gte = Define("gte");
        internal static readonly Operator And = Define("and");
        internal static readonly Operator Or = Define("or");
        internal static readonly Operator Not = Define("not");
        internal static readonly Operator Add = Define("add");
        internal static readonly Operator Sub = Define("sub");
        internal static readonly Operator Mul = Define("mul");
        internal static readonly Operator Div = Define("div");
        internal static readonly Operator Mod = Define("mod");
        internal static readonly Operator Neg = Define("neg");
        internal static readonly Operator Coalesce = Define("coalesce");
        internal static readonly Operator Cond = Define("cond");
        internal static readonly Operator Truthy = Define("truthy");
        internal static readonly Operator Empty = Define("empty");
        internal static readonly Operator Matches = Define("matches");
        internal static readonly Operator ToNumber = Define("toNumber");
        internal static readonly Operator ToDate = Define("toDate");
        internal static readonly Operator ToBoolean = Define("toBoolean");
        internal static readonly Operator ToText = Define("toText");
        internal static readonly Operator ToArray = Define("toArray");
        internal static readonly Operator Finite = Define("finite");
        internal static readonly Operator Order = Define("order");
        internal static readonly Operator Merge = Define("merge");

        private Operator(string token)
        {
            Token = token;
        }

        internal string Token { get; }

        internal static IReadOnlyList<Operator> Table => All;

        private static Operator Define(string token)
        {
            var op = new Operator(token);
            All.Add(op);
            return op;
        }
    }
}
