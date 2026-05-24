using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A symbolic condition tree built at extraction time using field NAMES.
    /// Resolved to <see cref="PlanModel.Condition"/> at render time when
    /// the component map is available.
    /// </summary>
    public abstract class FieldCondition
    {
        private protected FieldCondition() { }

        internal static FieldCondition Compare(ValidationFieldPath field, CompareOperator op) =>
            Compare(field, op, FieldComparisonValue.None);

        internal static FieldCondition Compare(ValidationFieldPath field, CompareOperator op, object? value) =>
            Compare(field, op, FieldComparisonValue.Literal(value));

        internal static FieldCondition Compare(
            ValidationFieldPath field,
            CompareOperator op,
            FieldComparisonValue value) =>
            new FieldCompare(field, op, value);

        internal static FieldCondition All(params FieldCondition[] terms) =>
            new FieldAll(FieldConditionTerms.From("all", terms));

        internal static FieldCondition Any(params FieldCondition[] terms) =>
            new FieldAny(FieldConditionTerms.From("any", terms));

        internal static FieldCondition Not(FieldCondition term) =>
            new FieldNot(term);

        internal abstract Condition ToPlanCondition(FieldConditionPlanBinding binding);

        internal abstract FieldCondition PrefixWith(FieldConditionPrefixBinding binding);
    }

    internal sealed class FieldConditionTerms
    {
        private readonly IReadOnlyList<FieldCondition> _items;

        private FieldConditionTerms(IReadOnlyList<FieldCondition> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        internal IReadOnlyList<FieldCondition> Items => _items;

        internal static FieldConditionTerms From(string composition, IEnumerable<FieldCondition> terms)
        {
            if (composition == null) throw new ArgumentNullException(nameof(composition));
            if (terms == null) throw new ArgumentNullException(nameof(terms));

            var items = new List<FieldCondition>();
            foreach (var term in terms)
            {
                if (term == null) throw new ArgumentException("Field condition term must not be null.", nameof(terms));
                items.Add(term);
            }

            if (items.Count == 0)
                throw new ArgumentException(
                    $"Composite field condition '{composition}' requires at least one term.",
                    nameof(terms));

            return new FieldConditionTerms(items);
        }

        internal Condition ToAllCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return Condition.All(_items.Select(term => term.ToPlanCondition(binding)).ToArray());
        }

        internal Condition ToAnyCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return Condition.Any(_items.Select(term => term.ToPlanCondition(binding)).ToArray());
        }

        internal FieldCondition PrefixAsAll(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return FieldCondition.All(_items.Select(term => term.PrefixWith(binding)).ToArray());
        }

        internal FieldCondition PrefixAsAny(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return FieldCondition.Any(_items.Select(term => term.PrefixWith(binding)).ToArray());
        }
    }

    /// <summary>
    /// A single field comparison: read field, apply operator, and optionally compare to a right operand.
    /// </summary>
    public sealed class FieldCompare : FieldCondition
    {
        private readonly ValidationFieldPath _field;
        private readonly CompareOperator _op;
        private readonly FieldComparisonValue _value;

        /// <summary>Property name to check (e.g. "IsEmployed").</summary>
        public string Field => _field.Value;

        /// <summary>Operator from <see cref="PlanModel.CompareOp"/>.</summary>
        public string Op => _op.Value;

        /// <summary>Gets the comparison operand with an explicit kind.</summary>
        public FieldComparisonOperand Operand => _value.ToOperand();

        internal FieldCompare(ValidationFieldPath field, CompareOperator op, FieldComparisonValue value)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _op = op ?? throw new ArgumentNullException(nameof(op));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal ValidationFieldPath FieldPath => _field;
        internal CompareOperator Operator => _op;
        internal FieldComparisonValue ValueOperand => _value;

        internal override Condition ToPlanCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return binding.Compare(_field, _op, _value);
        }

        internal override FieldCondition PrefixWith(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return binding.Compare(_field, _op, _value);
        }
    }

    internal abstract class FieldComparisonValue
    {
        private FieldComparisonValue() { }

        internal static FieldComparisonValue None { get; } =
            new UnaryFieldComparisonValue();

        internal abstract FieldComparisonOperand ToOperand();

        internal abstract ComparisonOperands BuildOperands(ValueProducer left, Shape fieldShape);

        internal static FieldComparisonValue Literal(object? value)
        {
            return value is object[] items
                ? (FieldComparisonValue)new ArrayFieldComparisonValue(items)
                : new LiteralFieldComparisonValue(value);
        }

        internal static FieldComparisonValue Array(IEnumerable<object?> items) =>
            new ArrayFieldComparisonValue(items);

        internal static FieldComparisonValue CollectionItem(object? value, Shape itemShape) =>
            new CollectionItemFieldComparisonValue(value, itemShape);

        private sealed class UnaryFieldComparisonValue : FieldComparisonValue
        {
            internal override FieldComparisonOperand ToOperand() =>
                FieldComparisonOperand.None;

            internal override ComparisonOperands BuildOperands(ValueProducer left, Shape fieldShape) =>
                ComparisonOperands.Unary(left, fieldShape);
        }

        private sealed class LiteralFieldComparisonValue : FieldComparisonValue
        {
            private readonly object? _value;

            internal LiteralFieldComparisonValue(object? value)
            {
                _value = value;
            }

            internal override FieldComparisonOperand ToOperand() =>
                FieldComparisonOperand.Literal(_value);

            internal override ComparisonOperands BuildOperands(ValueProducer left, Shape fieldShape) =>
                ComparisonOperands.Binary(left, ValueProducer.LiteralRaw(_value, fieldShape), fieldShape);
        }

        private sealed class ArrayFieldComparisonValue : FieldComparisonValue
        {
            private readonly IReadOnlyList<object?> _items;

            internal ArrayFieldComparisonValue(IEnumerable<object?> items)
            {
                if (items == null) throw new ArgumentNullException(nameof(items));
                _items = items.ToArray();
            }

            internal override FieldComparisonOperand ToOperand() =>
                FieldComparisonOperand.Array(_items);

            internal override ComparisonOperands BuildOperands(ValueProducer left, Shape fieldShape)
            {
                var values = new List<ValueProducer>(_items.Count);
                foreach (var item in _items)
                    values.Add(ValueProducer.LiteralRaw(item, fieldShape));
                return ComparisonOperands.Binary(
                    left,
                    ValueProducer.Array(values, FieldComparisonArrayOperandShape.FromComparedFieldShape(fieldShape)),
                    fieldShape);
            }
        }

        private sealed class CollectionItemFieldComparisonValue : FieldComparisonValue
        {
            private readonly object? _value;
            private readonly Shape _itemShape;

            internal CollectionItemFieldComparisonValue(object? value, Shape itemShape)
            {
                _value = value;
                _itemShape = itemShape ?? throw new ArgumentNullException(nameof(itemShape));
            }

            internal override FieldComparisonOperand ToOperand() =>
                FieldComparisonOperand.Literal(_value);

            internal override ComparisonOperands BuildOperands(ValueProducer left, Shape fieldShape) =>
                ComparisonOperands.CollectionItem(
                    left,
                    ValueProducer.LiteralRaw(_value, _itemShape),
                    fieldShape,
                    _itemShape);
        }
    }

    internal static class FieldComparisonArrayOperandShape
    {
        internal static Shape FromComparedFieldShape(Shape fieldShape)
        {
            if (fieldShape == null) throw new ArgumentNullException(nameof(fieldShape));
            if (fieldShape.IsNone) return Shape.ArrayOf(Shape.Any);

            return Shape.ArrayOf(fieldShape);
        }
    }

    /// <summary>
    /// Describes the right-hand operand of a field comparison without using
    /// <see langword="null"/> as the absence marker.
    /// </summary>
    public abstract class FieldComparisonOperand
    {
        private protected FieldComparisonOperand() { }

        /// <summary>Gets <c>none</c>, <c>literal</c>, or <c>array</c>.</summary>
        public abstract string Kind { get; }

        internal static FieldComparisonOperand None { get; } = new NoFieldComparisonOperand();

        internal static FieldComparisonOperand Literal(object? value) =>
            new LiteralFieldComparisonOperand(value);

        internal static FieldComparisonOperand Array(IEnumerable<object?> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return new ArrayFieldComparisonOperand(values);
        }
    }

    public sealed class NoFieldComparisonOperand : FieldComparisonOperand
    {
        internal NoFieldComparisonOperand() { }

        public override string Kind => "none";
    }

    public sealed class LiteralFieldComparisonOperand : FieldComparisonOperand
    {
        internal LiteralFieldComparisonOperand(object? value)
        {
            Value = value;
        }

        public override string Kind => "literal";
        public object? Value { get; }
        public bool IsLiteralNull => Value == null;
    }

    public sealed class ArrayFieldComparisonOperand : FieldComparisonOperand
    {
        private readonly object?[] _values;

        internal ArrayFieldComparisonOperand(IEnumerable<object?> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            _values = values.ToArray();
        }

        public override string Kind => "array";
        public IReadOnlyList<object?> Values => _values;
    }

    /// <summary>Logical AND — all terms must be true.</summary>
    public sealed class FieldAll : FieldCondition
    {
        private readonly FieldConditionTerms _terms;

        /// <summary>Gets the child conditions that must all be true.</summary>
        public IReadOnlyList<FieldCondition> Terms => _terms.Items;

        internal FieldAll(FieldConditionTerms terms)
        {
            _terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }

        internal override Condition ToPlanCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return _terms.ToAllCondition(binding);
        }

        internal override FieldCondition PrefixWith(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return _terms.PrefixAsAll(binding);
        }
    }

    /// <summary>Logical OR — any term must be true.</summary>
    public sealed class FieldAny : FieldCondition
    {
        private readonly FieldConditionTerms _terms;

        /// <summary>Gets the child conditions where at least one must be true.</summary>
        public IReadOnlyList<FieldCondition> Terms => _terms.Items;

        internal FieldAny(FieldConditionTerms terms)
        {
            _terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }

        internal override Condition ToPlanCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return _terms.ToAnyCondition(binding);
        }

        internal override FieldCondition PrefixWith(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return _terms.PrefixAsAny(binding);
        }
    }

    /// <summary>Logical NOT — inverts the inner term.</summary>
    public sealed class FieldNot : FieldCondition
    {
        /// <summary>Gets the inner condition to negate.</summary>
        public FieldCondition Term { get; }

        internal FieldNot(FieldCondition term)
        {
            Term = term ?? throw new ArgumentNullException(nameof(term));
        }

        internal override Condition ToPlanCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return Condition.Not(Term.ToPlanCondition(binding));
        }

        internal override FieldCondition PrefixWith(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return FieldCondition.Not(Term.PrefixWith(binding));
        }
    }

    internal sealed class FieldConditionPrefixBinding
    {
        private readonly ValidationFieldPath _prefix;
        private readonly Action<ValidationFieldPath> _ensureField;

        internal FieldConditionPrefixBinding(
            ValidationFieldPath prefix,
            Action<ValidationFieldPath> ensureField)
        {
            _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            _ensureField = ensureField ?? throw new ArgumentNullException(nameof(ensureField));
        }

        internal FieldCondition Compare(
            ValidationFieldPath field,
            CompareOperator op,
            FieldComparisonValue value)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var fullField = _prefix.Append(field);
            _ensureField(fullField);
            return FieldCondition.Compare(fullField, op, value);
        }
    }

    internal sealed class FieldConditionPlanBinding
    {
        private readonly Func<ValidationFieldPath, FieldComparisonTarget> _target;

        internal FieldConditionPlanBinding(Func<ValidationFieldPath, FieldComparisonTarget> target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        internal static FieldConditionPlanBinding For(ValidationFieldBindingCatalog fieldBindings)
        {
            if (fieldBindings == null) throw new ArgumentNullException(nameof(fieldBindings));
            return new FieldConditionPlanBinding(field => fieldBindings.Resolve(field).ReadConditionTarget());
        }

        internal Condition Compare(
            ValidationFieldPath fieldPath,
            CompareOperator op,
            FieldComparisonValue value)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return _target(fieldPath).Compare(op, value);
        }
    }

    internal sealed class FieldComparisonTarget
    {
        private readonly ValueProducer _left;
        private readonly Shape _shape;

        private FieldComparisonTarget(ValueProducer left, Shape shape)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal static FieldComparisonTarget ForComponentValue(ValueProducer value, Shape shape) =>
            new FieldComparisonTarget(value, shape);

        internal Condition Compare(CompareOperator op, FieldComparisonValue value)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return Condition.Compare(op, value.BuildOperands(_left, _shape));
        }
    }
}
