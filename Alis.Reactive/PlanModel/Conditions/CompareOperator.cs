using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>An ordering of a value against another of its shape: the six comparisons the grammar has.</summary>
    internal sealed class CompareOperator
    {
        private CompareOperator(string name, Operator operation)
        {
            Name = name;
            Operation = operation;
        }

        internal string Name { get; }

        /// <summary>The grammar operator the comparison writes.</summary>
        internal Operator Operation { get; }

        internal static CompareOperator Eq { get; } = new CompareOperator("eq", Operator.Eq);
        internal static CompareOperator Neq { get; } = new CompareOperator("neq", Operator.Neq);
        internal static CompareOperator Gt { get; } = new CompareOperator("gt", Operator.Gt);
        internal static CompareOperator Gte { get; } = new CompareOperator("gte", Operator.Gte);
        internal static CompareOperator Lt { get; } = new CompareOperator("lt", Operator.Lt);
        internal static CompareOperator Lte { get; } = new CompareOperator("lte", Operator.Lte);

        public override string ToString() => Name;
    }

    /// <summary>A test of a value alone: whether it is truthy, missing or empty.</summary>
    internal sealed class ValueTest
    {
        private ValueTest(string name)
        {
            Name = name;
        }

        internal string Name { get; }

        internal static ValueTest Truthy { get; } = new ValueTest("truthy");
        internal static ValueTest Falsy { get; } = new ValueTest("falsy");
        internal static ValueTest IsNull { get; } = new ValueTest("is-null");
        internal static ValueTest NotNull { get; } = new ValueTest("not-null");
        internal static ValueTest IsEmpty { get; } = new ValueTest("is-empty");
        internal static ValueTest NotEmpty { get; } = new ValueTest("not-empty");

        public override string ToString() => Name;
    }

    /// <summary>A test of text against text: a substring, a prefix, a suffix, or a pattern.</summary>
    internal sealed class TextTest
    {
        private TextTest(string name, string method)
        {
            Name = name;
            Method = method;
        }

        internal string Name { get; }

        /// <summary>The JavaScript string method the test calls; empty for a pattern, which the matches operator tests.</summary>
        internal string Method { get; }

        internal bool IsPattern => Method.Length == 0;

        internal static TextTest Contains { get; } = new TextTest("contains", "includes");
        internal static TextTest StartsWith { get; } = new TextTest("starts-with", "startsWith");
        internal static TextTest EndsWith { get; } = new TextTest("ends-with", "endsWith");
        internal static TextTest Matches { get; } = new TextTest("matches", "");

        public override string ToString() => Name;
    }
}
