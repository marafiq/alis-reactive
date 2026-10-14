namespace Alis.Reactive.PlanModel
{
    /// <summary>A comparison a condition makes; <see cref="UnaryCondition"/> and <see cref="BinaryCondition"/> write each one.</summary>
    internal sealed class CompareOperator
    {
        private CompareOperator(string name)
        {
            Name = name;
        }

        internal string Name { get; }

        internal static CompareOperator Eq { get; } = new CompareOperator("eq");
        internal static CompareOperator Neq { get; } = new CompareOperator("neq");
        internal static CompareOperator Gt { get; } = new CompareOperator("gt");
        internal static CompareOperator Gte { get; } = new CompareOperator("gte");
        internal static CompareOperator Lt { get; } = new CompareOperator("lt");
        internal static CompareOperator Lte { get; } = new CompareOperator("lte");
        internal static CompareOperator Truthy { get; } = new CompareOperator("truthy");
        internal static CompareOperator Falsy { get; } = new CompareOperator("falsy");
        internal static CompareOperator IsNull { get; } = new CompareOperator("is-null");
        internal static CompareOperator NotNull { get; } = new CompareOperator("not-null");
        internal static CompareOperator IsEmpty { get; } = new CompareOperator("is-empty");
        internal static CompareOperator NotEmpty { get; } = new CompareOperator("not-empty");
        internal static CompareOperator In { get; } = new CompareOperator("in");
        internal static CompareOperator NotIn { get; } = new CompareOperator("not-in");
        internal static CompareOperator Between { get; } = new CompareOperator("between");
        internal static CompareOperator Contains { get; } = new CompareOperator("contains");
        internal static CompareOperator StartsWith { get; } = new CompareOperator("starts-with");
        internal static CompareOperator EndsWith { get; } = new CompareOperator("ends-with");
        internal static CompareOperator Matches { get; } = new CompareOperator("matches");
        internal static CompareOperator MinLength { get; } = new CompareOperator("min-length");
        internal static CompareOperator ArrayContains { get; } = new CompareOperator("array-contains");

        public override string ToString() => Name;
    }
}
