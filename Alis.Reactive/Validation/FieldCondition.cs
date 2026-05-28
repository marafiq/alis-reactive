using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A symbolic condition tree built from client validation field paths.
    /// Resolved to <see cref="PlanModel.ConditionGraph"/> at render time when
    /// the component map is available.
    /// </summary>
    internal abstract class FieldCondition
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
            new FieldAll(CompositeTerms("all", terms));

        internal static FieldCondition Any(params FieldCondition[] terms) =>
            new FieldAny(CompositeTerms("any", terms));

        internal static FieldCondition Not(FieldCondition term) =>
            new FieldNot(term);

        internal abstract ConditionGraph ToPlanCondition(FieldConditionPlanBinding binding);

        internal abstract FieldCondition PrefixWith(FieldConditionPrefixBinding binding);

        private static IReadOnlyList<FieldCondition> CompositeTerms(
            string composition,
            IEnumerable<FieldCondition> terms)
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

            return items;
        }
    }

    /// <summary>
    /// A single field comparison: read field, apply operator, and optionally compare to a right operand.
    /// </summary>
    internal sealed class FieldCompare : FieldCondition
    {
        private readonly ValidationFieldPath _field;
        private readonly CompareOperator _op;
        private readonly FieldComparisonValue _value;

        internal FieldCompare(ValidationFieldPath field, CompareOperator op, FieldComparisonValue value)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _op = op ?? throw new ArgumentNullException(nameof(op));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal ValidationFieldPath FieldPath => _field;
        internal CompareOperator Operator => _op;
        internal FieldComparisonValue ValueOperand => _value;

        internal override ConditionGraph ToPlanCondition(FieldConditionPlanBinding binding)
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

        internal abstract ComparisonOperands BuildOperands(ValueExpression left, Shape fieldShape);

        internal static FieldComparisonValue Literal(object? value)
        {
            return value is object[] items
                ? (FieldComparisonValue)new ArrayFieldComparisonValue(items)
                : new LiteralFieldComparisonValue(value);
        }

        internal static FieldComparisonValue Text(string value) =>
            new ShapedLiteralFieldComparisonValue(value, Shape.String);

        internal static FieldComparisonValue Number(int value) =>
            new ShapedLiteralFieldComparisonValue(value, Shape.Number);

        internal static FieldComparisonValue Array(IEnumerable<object?> items) =>
            new ArrayFieldComparisonValue(items);

        internal static FieldComparisonValue CollectionItem(object? value, Shape itemShape) =>
            new CollectionItemFieldComparisonValue(value, itemShape);

        private sealed class UnaryFieldComparisonValue : FieldComparisonValue
        {
            internal override ComparisonOperands BuildOperands(ValueExpression left, Shape fieldShape) =>
                ComparisonOperands.Unary(left, fieldShape);
        }

        private sealed class LiteralFieldComparisonValue : FieldComparisonValue
        {
            private readonly object? _value;

            internal LiteralFieldComparisonValue(object? value)
            {
                _value = value;
            }

            internal override ComparisonOperands BuildOperands(ValueExpression left, Shape fieldShape) =>
                ComparisonOperands.Binary(left, ValueExpression.LiteralRaw(_value, fieldShape), fieldShape);
        }

        private sealed class ShapedLiteralFieldComparisonValue : FieldComparisonValue
        {
            private readonly object? _value;
            private readonly Shape _literalShape;

            internal ShapedLiteralFieldComparisonValue(object? value, Shape literalShape)
            {
                _value = value;
                _literalShape = literalShape ?? throw new ArgumentNullException(nameof(literalShape));
            }

            internal override ComparisonOperands BuildOperands(ValueExpression left, Shape fieldShape) =>
                ComparisonOperands.Binary(left, ValueExpression.LiteralRaw(_value, _literalShape), fieldShape);
        }

        private sealed class ArrayFieldComparisonValue : FieldComparisonValue
        {
            private readonly IReadOnlyList<object?> _items;

            internal ArrayFieldComparisonValue(IEnumerable<object?> items)
            {
                if (items == null) throw new ArgumentNullException(nameof(items));
                _items = items.ToArray();
            }

            internal override ComparisonOperands BuildOperands(ValueExpression left, Shape fieldShape)
            {
                var values = new List<ValueExpression>(_items.Count);
                foreach (var item in _items)
                    values.Add(ValueExpression.LiteralRaw(item, fieldShape));
                return ComparisonOperands.Binary(
                    left,
                    ValueExpression.Array(values, Shape.ArrayOf(fieldShape.IsNone ? Shape.Any : fieldShape)),
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

            internal override ComparisonOperands BuildOperands(ValueExpression left, Shape fieldShape) =>
                ComparisonOperands.CollectionItem(
                    left,
                    ValueExpression.LiteralRaw(_value, _itemShape),
                    fieldShape,
                    _itemShape);
        }
    }

    internal sealed class FieldAll : FieldCondition
    {
        private readonly IReadOnlyList<FieldCondition> _terms;

        internal FieldAll(IReadOnlyList<FieldCondition> terms)
        {
            _terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }

        internal override ConditionGraph ToPlanCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return ConditionGraph.All(_terms.Select(term => term.ToPlanCondition(binding)).ToArray());
        }

        internal override FieldCondition PrefixWith(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return FieldCondition.All(_terms.Select(term => term.PrefixWith(binding)).ToArray());
        }
    }

    internal sealed class FieldAny : FieldCondition
    {
        private readonly IReadOnlyList<FieldCondition> _terms;

        internal FieldAny(IReadOnlyList<FieldCondition> terms)
        {
            _terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }

        internal override ConditionGraph ToPlanCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return ConditionGraph.Any(_terms.Select(term => term.ToPlanCondition(binding)).ToArray());
        }

        internal override FieldCondition PrefixWith(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return FieldCondition.Any(_terms.Select(term => term.PrefixWith(binding)).ToArray());
        }
    }

    internal sealed class FieldNot : FieldCondition
    {
        private readonly FieldCondition _term;

        internal FieldNot(FieldCondition term)
        {
            _term = term ?? throw new ArgumentNullException(nameof(term));
        }

        internal override ConditionGraph ToPlanCondition(FieldConditionPlanBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return ConditionGraph.Not(_term.ToPlanCondition(binding));
        }

        internal override FieldCondition PrefixWith(FieldConditionPrefixBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            return FieldCondition.Not(_term.PrefixWith(binding));
        }
    }

    internal sealed class FieldConditionPrefixBinding
    {
        private readonly ValidationFieldPath _prefix;

        internal FieldConditionPrefixBinding(ValidationFieldPath prefix)
        {
            _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
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

        internal ConditionGraph Compare(
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
        private readonly ValueExpression _left;
        private readonly Shape _shape;

        private FieldComparisonTarget(ValueExpression left, Shape shape)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal static FieldComparisonTarget ForComponentValue(ValueExpression value, Shape shape) =>
            new FieldComparisonTarget(value, shape);

        internal ConditionGraph Compare(CompareOperator op, FieldComparisonValue value)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return ConditionGraph.Compare(op, value.BuildOperands(_left, _shape));
        }
    }
}
