using System;
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

        internal static readonly Operator Eq = Define("eq", 2, 2);
        internal static readonly Operator Neq = Define("neq", 2, 2);
        internal static readonly Operator Lt = Define("lt", 2, 2);
        internal static readonly Operator Lte = Define("lte", 2, 2);
        internal static readonly Operator Gt = Define("gt", 2, 2);
        internal static readonly Operator Gte = Define("gte", 2, 2);
        internal static readonly Operator And = Define("and", 2, int.MaxValue);
        internal static readonly Operator Or = Define("or", 2, int.MaxValue);
        internal static readonly Operator Not = Define("not", 1, 1);
        internal static readonly Operator Add = Define("add", 2, 2);
        internal static readonly Operator Sub = Define("sub", 2, 2);
        internal static readonly Operator Mul = Define("mul", 2, 2);
        internal static readonly Operator Div = Define("div", 2, 2);
        internal static readonly Operator Mod = Define("mod", 2, 2);
        internal static readonly Operator Neg = Define("neg", 1, 1);
        internal static readonly Operator Coalesce = Define("coalesce", 2, int.MaxValue);
        internal static readonly Operator Cond = Define("cond", 3, 3);
        internal static readonly Operator Truthy = Define("truthy", 1, 1);
        internal static readonly Operator Empty = Define("empty", 1, 1);
        internal static readonly Operator Matches = Define("matches", 3, 3);
        internal static readonly Operator ToNumber = Define("toNumber", 1, 1);
        internal static readonly Operator ToDate = Define("toDate", 1, 1);
        internal static readonly Operator ToBoolean = Define("toBoolean", 1, 1);
        internal static readonly Operator ToText = Define("toText", 1, 1);
        internal static readonly Operator ToArray = Define("toArray", 1, 1);
        internal static readonly Operator Finite = Define("finite", 1, 1);
        internal static readonly Operator Order = Define("order", 2, 2);

        private readonly int _minArity;
        private readonly int _maxArity;

        private Operator(string token, int minArity, int maxArity)
        {
            Token = token;
            _minArity = minArity;
            _maxArity = maxArity;
        }

        internal string Token { get; }

        internal static IReadOnlyList<Operator> Table => All;

        internal void RequireArity(int count)
        {
            if (count < _minArity || count > _maxArity)
                throw new ArgumentException(
                    $"Operator '{Token}' takes {Describe()} operand(s), not {count}.");
        }

        private string Describe() =>
            _maxArity == int.MaxValue ? _minArity + " or more"
            : _minArity == _maxArity ? _minArity.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : _minArity + " to " + _maxArity;

        private static Operator Define(string token, int minArity, int maxArity)
        {
            var op = new Operator(token, minArity, maxArity);
            All.Add(op);
            return op;
        }
    }
}
