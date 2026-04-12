using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for conditional predicates evaluated when the plan executes. Not constructed in application code.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Condition>))]
    public abstract class Condition
    {
        private protected Condition() { }

        internal static Condition Compare(ValueProducer left, string op, ValueProducer right = null, Shape shape = null, Shape itemShape = null) =>
            new CompareCondition(left, op, right, shape, itemShape);

        internal static Condition All(params Condition[] terms) =>
            new AllCondition(new List<Condition>(terms));

        internal static Condition Any(params Condition[] terms) =>
            new AnyCondition(new List<Condition>(terms));

        internal static Condition Not(Condition term) =>
            new NotCondition(term);

        internal static Condition Confirm(string message) =>
            new ConfirmCondition(message);

        internal static readonly Condition None = new NoneCondition();

        internal bool IsNone => this is NoneCondition;
    }

    /// <summary>Compares two values using a relational operator.</summary>
    public sealed class CompareCondition : Condition
    {
        /// <summary>Gets the kind. Always <c>"compare"</c>.</summary>
        public string Kind => "compare";
        /// <summary>Gets the left-hand operand.</summary>
        public ValueProducer Left { get; }
        /// <summary>Gets the comparison operator (eq, neq, gt, gte, lt, lte, truthy, empty, contains, startsWith, endsWith).</summary>
        public string Op { get; }
        /// <summary>Gets the right-hand operand. <see cref="ValueProducer.None"/> for unary operators.</summary>
        public ValueProducer Right { get; }
        /// <summary>Gets the expected type shape for comparison. <see cref="PlanModel.Shape.None"/> when not specified.</summary>
        public Shape Shape { get; }
        /// <summary>Gets the element type shape used by collection operators such as <c>contains</c>. <see cref="PlanModel.Shape.None"/> for non-collection comparisons.</summary>
        public Shape ItemShape { get; }

        internal CompareCondition(ValueProducer left, string op, ValueProducer right = null, Shape shape = null, Shape itemShape = null)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Right = right ?? ValueProducer.None;
            Shape = shape ?? Shape.None;
            ItemShape = itemShape ?? Shape.None;
        }
    }

    /// <summary>Logical AND: all child conditions must be true.</summary>
    public sealed class AllCondition : Condition
    {
        /// <summary>Gets the kind. Always <c>"all"</c>.</summary>
        public string Kind => "all";
        /// <summary>Gets the child conditions that must all be true.</summary>
        public IReadOnlyList<Condition> Terms { get; }

        internal AllCondition(List<Condition> terms)
        {
            Terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    /// <summary>Logical OR: at least one child condition must be true.</summary>
    public sealed class AnyCondition : Condition
    {
        /// <summary>Gets the kind. Always <c>"any"</c>.</summary>
        public string Kind => "any";
        /// <summary>Gets the child conditions where at least one must be true.</summary>
        public IReadOnlyList<Condition> Terms { get; }

        internal AnyCondition(List<Condition> terms)
        {
            Terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    /// <summary>Logical NOT: inverts a single child condition.</summary>
    public sealed class NotCondition : Condition
    {
        /// <summary>Gets the kind. Always <c>"not"</c>.</summary>
        public string Kind => "not";
        /// <summary>Gets the condition to invert.</summary>
        public Condition Term { get; }

        internal NotCondition(Condition term)
        {
            Term = term ?? throw new ArgumentNullException(nameof(term));
        }
    }

    /// <summary>Prompts the user for confirmation before the reaction proceeds.</summary>
    public sealed class ConfirmCondition : Condition
    {
        /// <summary>Gets the kind. Always <c>"confirm"</c>.</summary>
        public string Kind => "confirm";
        /// <summary>Gets the confirmation message shown to the user.</summary>
        public string Message { get; }

        internal ConfirmCondition(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }

    /// <summary>Sentinel for "no guard specified." Evaluates to true (no restriction). Not constructed in application code.</summary>
    public sealed class NoneCondition : Condition
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public string Kind => "none";

        internal NoneCondition() { }
    }
}
