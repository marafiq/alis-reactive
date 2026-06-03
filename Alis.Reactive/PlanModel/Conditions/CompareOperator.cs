using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class CompareOperator : PlanString
    {
        internal static readonly string[] EqualityValues =
        {
            CompareOp.Eq,
            CompareOp.Neq,
        };

        internal static readonly string[] OrderedValues =
        {
            CompareOp.Gt,
            CompareOp.Gte,
            CompareOp.Lt,
            CompareOp.Lte,
        };

        internal static readonly string[] UnaryValues =
        {
            CompareOp.Truthy,
            CompareOp.Falsy,
            CompareOp.IsNull,
            CompareOp.NotNull,
            CompareOp.IsEmpty,
            CompareOp.NotEmpty,
        };

        internal static readonly string[] MembershipValues =
        {
            CompareOp.In,
            CompareOp.NotIn,
        };

        internal static readonly string[] RangeValues =
        {
            CompareOp.Between,
        };

        internal static readonly string[] TextValues =
        {
            CompareOp.Contains,
            CompareOp.StartsWith,
            CompareOp.EndsWith,
        };

        internal static readonly string[] RegexValues =
        {
            CompareOp.Matches,
        };

        internal static readonly string[] TextLengthValues =
        {
            CompareOp.MinLength,
        };

        internal static readonly string[] CollectionItemValues =
        {
            CompareOp.ArrayContains,
        };

        internal static CompareOperator Eq { get; } = new CompareOperator(CompareOp.Eq);
        internal static CompareOperator Neq { get; } = new CompareOperator(CompareOp.Neq);
        internal static CompareOperator Gt { get; } = new CompareOperator(CompareOp.Gt);
        internal static CompareOperator Gte { get; } = new CompareOperator(CompareOp.Gte);
        internal static CompareOperator Lt { get; } = new CompareOperator(CompareOp.Lt);
        internal static CompareOperator Lte { get; } = new CompareOperator(CompareOp.Lte);
        internal static CompareOperator Truthy { get; } = new CompareOperator(CompareOp.Truthy);
        internal static CompareOperator Falsy { get; } = new CompareOperator(CompareOp.Falsy);
        internal static CompareOperator IsNull { get; } = new CompareOperator(CompareOp.IsNull);
        internal static CompareOperator NotNull { get; } = new CompareOperator(CompareOp.NotNull);
        internal static CompareOperator IsEmpty { get; } = new CompareOperator(CompareOp.IsEmpty);
        internal static CompareOperator NotEmpty { get; } = new CompareOperator(CompareOp.NotEmpty);
        internal static CompareOperator In { get; } = new CompareOperator(CompareOp.In);
        internal static CompareOperator NotIn { get; } = new CompareOperator(CompareOp.NotIn);
        internal static CompareOperator Between { get; } = new CompareOperator(CompareOp.Between);
        internal static CompareOperator Contains { get; } = new CompareOperator(CompareOp.Contains);
        internal static CompareOperator StartsWith { get; } = new CompareOperator(CompareOp.StartsWith);
        internal static CompareOperator EndsWith { get; } = new CompareOperator(CompareOp.EndsWith);
        internal static CompareOperator Matches { get; } = new CompareOperator(CompareOp.Matches);
        internal static CompareOperator MinLength { get; } = new CompareOperator(CompareOp.MinLength);
        internal static CompareOperator ArrayContains { get; } = new CompareOperator(CompareOp.ArrayContains);

        private static readonly Dictionary<string, CompareOperator> Known =
            new Dictionary<string, CompareOperator>(StringComparer.Ordinal)
            {
                { CompareOp.Eq, Eq },
                { CompareOp.Neq, Neq },
                { CompareOp.Gt, Gt },
                { CompareOp.Gte, Gte },
                { CompareOp.Lt, Lt },
                { CompareOp.Lte, Lte },
                { CompareOp.Truthy, Truthy },
                { CompareOp.Falsy, Falsy },
                { CompareOp.IsNull, IsNull },
                { CompareOp.NotNull, NotNull },
                { CompareOp.IsEmpty, IsEmpty },
                { CompareOp.NotEmpty, NotEmpty },
                { CompareOp.In, In },
                { CompareOp.NotIn, NotIn },
                { CompareOp.Between, Between },
                { CompareOp.Contains, Contains },
                { CompareOp.StartsWith, StartsWith },
                { CompareOp.EndsWith, EndsWith },
                { CompareOp.Matches, Matches },
                { CompareOp.MinLength, MinLength },
                { CompareOp.ArrayContains, ArrayContains },
            };

        private CompareOperator(string value) : base(value, nameof(value)) { }

        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal bool RequiresRightOperand => Array.IndexOf(UnaryValues, Value) < 0;

    }
}
