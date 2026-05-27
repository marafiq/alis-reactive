using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for conditional predicates evaluated when the plan executes. Not constructed in application code.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ConditionGraph>))]
    public abstract class ConditionGraph
    {
        private protected ConditionGraph() { }

        internal static ConditionGraph Compare(CompareOperator op, ComparisonOperands operands) =>
            new CompareCondition(op, operands);

        internal static ConditionGraph All(params ConditionGraph[] terms) =>
            new AllCondition(CompositeTerms("all", terms));

        internal static ConditionGraph Any(params ConditionGraph[] terms) =>
            new AnyCondition(CompositeTerms("any", terms));

        internal static ConditionGraph Not(ConditionGraph term) =>
            new NotCondition(term);

        internal static ConditionGraph Confirm(string message) =>
            new ConfirmCondition(message);

        private static IReadOnlyList<ConditionGraph> CompositeTerms(string composition, IEnumerable<ConditionGraph> terms)
        {
            if (composition == null) throw new ArgumentNullException(nameof(composition));
            if (terms == null) throw new ArgumentNullException(nameof(terms));

            var items = new List<ConditionGraph>();
            foreach (var term in terms)
            {
                if (term == null) throw new ArgumentException("ConditionGraph term must not be null.", nameof(terms));
                items.Add(term);
            }

            if (items.Count == 0)
                throw new ArgumentException(
                    $"Composite condition '{composition}' requires at least one term.",
                    nameof(terms));

            return items;
        }
    }

    /// <summary>Compares two values using a relational operator.</summary>
    [JsonConverter(typeof(CompareConditionJsonConverter))]
    public sealed class CompareCondition : ConditionGraph
    {
        private readonly CompareOperator _op;
        private readonly ComparisonOperands _operands;

        /// <summary>Gets the kind. Always <c>"compare"</c>.</summary>
        public string Kind => "compare";
        /// <summary>Gets the left-hand operand.</summary>
        public ValueProducer Left => _operands.Left;
        /// <summary>Gets the comparison operator (eq, neq, gt, gte, lt, lte, truthy, empty, contains, startsWith, endsWith).</summary>
        public string Op => _op.Value;
        /// <summary>Gets the expected type shape for comparison. <see cref="PlanModel.Shape.None"/> when not specified.</summary>
        public Shape Shape => _operands.ShapeForJson;
        /// <summary>Gets the element type shape used by collection operators such as <c>contains</c>. <see cref="PlanModel.Shape.None"/> for non-collection comparisons.</summary>
        public Shape ItemShape => _operands.ItemShapeForJson;

        internal ComparisonRightOperand RightOperand => _operands.Right;

        internal CompareCondition(CompareOperator op, ComparisonOperands operands)
        {
            _op = op ?? throw new ArgumentNullException(nameof(op));
            _operands = operands ?? throw new ArgumentNullException(nameof(operands));
            if (_op.RequiresRightOperand != _operands.HasRightOperand)
                throw new ArgumentException(
                    "Comparison operator '" + _op.Value + "' requires " +
                    (_op.RequiresRightOperand ? "a right operand." : "no right operand."),
                    nameof(operands));
        }
    }

    internal sealed class CompareConditionJsonConverter : JsonConverter<CompareCondition>
    {
        public override void Write(Utf8JsonWriter writer, CompareCondition value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            WriteProperty(writer, options, "left", value.Left);
            writer.WriteString("op", value.Op);
            WriteProperty(writer, options, "right", value.RightOperand);
            WriteProperty(writer, options, "shape", value.Shape);
            WriteProperty(writer, options, "itemShape", value.ItemShape);
            writer.WriteEndObject();
        }

        public override CompareCondition Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");

        private static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    internal sealed class ComparisonOperands
    {
        private readonly ComparisonRightOperand _right;

        private ComparisonOperands(
            ValueProducer left,
            ComparisonRightOperand right,
            Shape shape,
            Shape itemShape)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            ShapeForJson = shape ?? throw new ArgumentNullException(nameof(shape));
            ItemShapeForJson = itemShape ?? throw new ArgumentNullException(nameof(itemShape));
        }

        internal ValueProducer Left { get; }
        internal ComparisonRightOperand Right => _right;
        internal bool HasRightOperand => _right.HasValue;
        internal Shape ShapeForJson { get; }
        internal Shape ItemShapeForJson { get; }

        internal static ComparisonOperands Unary(ValueProducer left, Shape shape) =>
            new ComparisonOperands(
                left,
                ComparisonRightOperand.Absent,
                shape,
                Shape.None);

        internal static ComparisonOperands Binary(ValueProducer left, ValueProducer right, Shape shape) =>
            new ComparisonOperands(
                left,
                ComparisonRightOperand.Present(right),
                shape,
                Shape.None);

        internal static ComparisonOperands CollectionItem(
            ValueProducer left,
            ValueProducer right,
            Shape collectionShape,
            Shape itemShape) =>
            new ComparisonOperands(
                left,
                ComparisonRightOperand.Present(right),
                collectionShape,
                itemShape);
    }

    [JsonConverter(typeof(ComparisonRightOperandJsonConverter))]
    internal abstract class ComparisonRightOperand
    {
        internal static ComparisonRightOperand Absent { get; } =
            new AbsentComparisonRightOperand();

        internal static ComparisonRightOperand Present(ValueProducer value) =>
            new PresentComparisonRightOperand(value);

        public abstract string Kind { get; }
        internal abstract bool HasValue { get; }

        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);
    }

    internal sealed class PresentComparisonRightOperand : ComparisonRightOperand
    {
        private readonly ValueProducer _value;

        internal PresentComparisonRightOperand(ValueProducer value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal ValueProducer Value => _value;

        public override string Kind => "value";
        internal override bool HasValue => true;

        internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
            ComparisonRightOperandJsonConverter.WriteProperty(writer, options, "value", _value);
    }

    internal sealed class AbsentComparisonRightOperand : ComparisonRightOperand
    {
        public override string Kind => "none";
        internal override bool HasValue => false;

        internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
        }
    }

    internal sealed class ComparisonRightOperandJsonConverter : JsonConverter<ComparisonRightOperand>
    {
        public override void Write(
            Utf8JsonWriter writer,
            ComparisonRightOperand value,
            JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WritePayload(writer, options);
            writer.WriteEndObject();
        }

        public override ComparisonRightOperand Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");

        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    /// <summary>Logical AND: all child conditions must be true.</summary>
    public sealed class AllCondition : ConditionGraph
    {
        private readonly IReadOnlyList<ConditionGraph> _terms;

        /// <summary>Gets the kind. Always <c>"all"</c>.</summary>
        public string Kind => "all";
        /// <summary>Gets the child conditions that must all be true.</summary>
        public IReadOnlyList<ConditionGraph> Terms => _terms;

        internal AllCondition(IReadOnlyList<ConditionGraph> terms)
        {
            _terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    /// <summary>Logical OR: at least one child condition must be true.</summary>
    public sealed class AnyCondition : ConditionGraph
    {
        private readonly IReadOnlyList<ConditionGraph> _terms;

        /// <summary>Gets the kind. Always <c>"any"</c>.</summary>
        public string Kind => "any";
        /// <summary>Gets the child conditions where at least one must be true.</summary>
        public IReadOnlyList<ConditionGraph> Terms => _terms;

        internal AnyCondition(IReadOnlyList<ConditionGraph> terms)
        {
            _terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    /// <summary>Logical NOT: inverts a single child condition.</summary>
    public sealed class NotCondition : ConditionGraph
    {
        /// <summary>Gets the kind. Always <c>"not"</c>.</summary>
        public string Kind => "not";
        /// <summary>Gets the condition to invert.</summary>
        public ConditionGraph Term { get; }

        internal NotCondition(ConditionGraph term)
        {
            Term = term ?? throw new ArgumentNullException(nameof(term));
        }
    }

    /// <summary>Prompts the user for confirmation before the reaction proceeds.</summary>
    public sealed class ConfirmCondition : ConditionGraph
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

}
