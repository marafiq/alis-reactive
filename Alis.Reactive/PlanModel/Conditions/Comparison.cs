using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>An ordering of a value against another of its shape: the six comparisons the grammar has.</summary>
    public sealed class Comparison
    {
        private Comparison(string name, Operator operation, bool isEquality)
        {
            Name = name;
            Operation = operation;
            IsEquality = isEquality;
        }

        /// <summary>The value equals the other.</summary>
        public static Comparison Equal { get; } = new Comparison("eq", Operator.Eq, isEquality: true);

        /// <summary>The value differs from the other.</summary>
        public static Comparison NotEqual { get; } = new Comparison("neq", Operator.Neq, isEquality: true);

        /// <summary>The value is greater than the other.</summary>
        public static Comparison GreaterThan { get; } = new Comparison("gt", Operator.Gt, isEquality: false);

        /// <summary>The value is at least the other.</summary>
        public static Comparison GreaterThanOrEqual { get; } = new Comparison("gte", Operator.Gte, isEquality: false);

        /// <summary>The value is less than the other.</summary>
        public static Comparison LessThan { get; } = new Comparison("lt", Operator.Lt, isEquality: false);

        /// <summary>The value is at most the other.</summary>
        public static Comparison LessThanOrEqual { get; } = new Comparison("lte", Operator.Lte, isEquality: false);

        internal string Name { get; }

        /// <summary>The grammar operator the comparison writes.</summary>
        internal Operator Operation { get; }

        /// <summary>Equal or not equal, which compare whatever both sides hold; the others order the sides.</summary>
        internal bool IsEquality { get; }

        /// <summary>Whether an ordering holds, given how the value compares with the other: negative, zero or positive.</summary>
        internal bool Orders(int order) =>
            this == GreaterThan ? order > 0
            : this == GreaterThanOrEqual ? order >= 0
            : this == LessThan ? order < 0
            : order <= 0;

        public override string ToString() => Name;
    }

    /// <summary>A test of a value alone: whether it is truthy, missing or empty.</summary>
    public sealed class ValueTest
    {
        private ValueTest(string name)
        {
            Name = name;
        }

        /// <summary>The value is truthy: not null, false, zero or empty.</summary>
        public static ValueTest Truthy { get; } = new ValueTest("truthy");

        /// <summary>The value is falsy.</summary>
        public static ValueTest Falsy { get; } = new ValueTest("falsy");

        /// <summary>Nothing is bound.</summary>
        public static ValueTest IsNull { get; } = new ValueTest("is-null");

        /// <summary>A value is bound.</summary>
        public static ValueTest NotNull { get; } = new ValueTest("not-null");

        /// <summary>The value is null, empty text or an empty list.</summary>
        public static ValueTest IsEmpty { get; } = new ValueTest("is-empty");

        /// <summary>The value is not empty.</summary>
        public static ValueTest NotEmpty { get; } = new ValueTest("not-empty");

        internal string Name { get; }

        public override string ToString() => Name;
    }

    /// <summary>A test of text against text: a substring, a prefix, a suffix, or a pattern.</summary>
    public sealed class TextTest
    {
        private TextTest(string name, string method)
        {
            Name = name;
            Method = method;
        }

        /// <summary>The text contains the other.</summary>
        public static TextTest Contains { get; } = new TextTest("contains", "includes");

        /// <summary>The text starts with the other.</summary>
        public static TextTest StartsWith { get; } = new TextTest("starts-with", "startsWith");

        /// <summary>The text ends with the other.</summary>
        public static TextTest EndsWith { get; } = new TextTest("ends-with", "endsWith");

        /// <summary>The text matches the other as a regular expression.</summary>
        public static TextTest Matches { get; } = new TextTest("matches", "");

        internal string Name { get; }

        /// <summary>The JavaScript string method the test calls; empty for a pattern, which the matches operator tests.</summary>
        internal string Method { get; }

        internal bool IsPattern => Method.Length == 0;

        public override string ToString() => Name;
    }
}
