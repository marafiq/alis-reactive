using System;
using FluentValidation.Validators;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    public sealed partial class FluentValidationAdapter
    {
        private abstract class ComparisonRuleOperands
        {
            private protected ComparisonRuleOperands() { }

            internal abstract ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix);

            internal abstract ClientRuleProjection BuildRule(
                Comparison comparison,
                RuleComponentMapping mapping);

            internal static ComparisonRuleOperands From(
                IComparisonValidator validator,
                ValidationRuleTarget target)
            {
                if (validator == null) throw new ArgumentNullException(nameof(validator));
                if (target == null) throw new ArgumentNullException(nameof(target));

                var peerMember = validator.MemberToCompare;
                if (peerMember == null)
                    return Literal(validator.ValueToCompare);

                return ComparisonPeerField.ToOperands(peerMember, target);
            }

            private static ComparisonRuleOperands Literal(object? value) =>
                new LiteralComparisonRuleOperands(value);
        }

        private abstract class ProjectableComparisonRuleOperands : ComparisonRuleOperands
        {
            internal abstract ValidationRuleName NotEqualRule { get; }

            internal abstract ValidationRuleDetails DetailsFor(ValidationRuleCondition condition);

            internal abstract string Message(
                string displayName,
                string fieldVerb,
                string constraintVerb);

            internal abstract string NotEqualMessage(string displayName);

            internal override ClientRuleProjection BuildRule(
                Comparison comparison,
                RuleComponentMapping mapping)
            {
                if (mapping == null) throw new ArgumentNullException(nameof(mapping));

                var displayName = mapping.DisplayName;
                return comparison switch
                {
                    Comparison.Equal => ClientRule(
                        ValidationRuleName.EqualTo,
                        Message(displayName, "must match", "must equal"),
                        mapping),
                    Comparison.NotEqual => ClientRule(
                        NotEqualRule,
                        NotEqualMessage(displayName),
                        mapping),
                    Comparison.GreaterThanOrEqual => ClientRule(
                        ValidationRuleName.Min,
                        Message(displayName, "must be at least", "must be at least"),
                        mapping),
                    Comparison.LessThanOrEqual => ClientRule(
                        ValidationRuleName.Max,
                        Message(displayName, "must be at most", "must be at most"),
                        mapping),
                    Comparison.GreaterThan => ClientRule(
                        ValidationRuleName.Gt,
                        Message(displayName, "must be greater than", "must be greater than"),
                        mapping),
                    Comparison.LessThan => ClientRule(
                        ValidationRuleName.Lt,
                        Message(displayName, "must be less than", "must be less than"),
                        mapping),
                    _ => ClientRuleProjection.SkipClientProjection(ClientRuleProjectionSkipReason.UnsupportedComparisonOperator)
                };
            }

            private ClientRuleProjection ClientRule(
                ValidationRuleName ruleName,
                string defaultMessage,
                RuleComponentMapping mapping)
            {
                return ClientRuleProjection.Project(new ProjectedClientValidationRule(
                    ruleName,
                    mapping.Message.OrDefault(defaultMessage),
                    DetailsFor(mapping.RuleCondition)));
            }
        }

        private sealed class SkippedComparisonRuleOperands : ComparisonRuleOperands
        {
            private readonly ClientRuleProjectionSkipReason _reason;

            internal SkippedComparisonRuleOperands(ClientRuleProjectionSkipReason reason)
            {
                _reason = reason;
            }

            internal override ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return this;
            }

            internal override ClientRuleProjection BuildRule(
                Comparison comparison,
                RuleComponentMapping mapping)
            {
                if (mapping == null) throw new ArgumentNullException(nameof(mapping));
                return ClientRuleProjection.SkipClientProjection(_reason);
            }
        }

        private sealed class ComparisonPeerField
        {
            private ComparisonPeerField(ValidationFieldPath path, Shape shape)
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
                Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            }

            internal ValidationFieldPath Path { get; }
            internal Shape Shape { get; }

            internal static ComparisonPeerField Of(ValidationFieldPath path, Shape shape) =>
                new ComparisonPeerField(path, shape);

            internal static ComparisonRuleOperands ToOperands(
                System.Reflection.MemberInfo member,
                ValidationRuleTarget target)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));
                if (target == null) throw new ArgumentNullException(nameof(target));
                return target.ClassifyPeerMember(member).ToOperands(member);
            }

            internal ComparisonPeerField PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return new ComparisonPeerField(prefix.Append(Path), Shape);
            }
        }

        private abstract class ComparisonPeerShape
        {
            private protected ComparisonPeerShape() { }

            internal static ComparisonPeerShape From(System.Reflection.MemberInfo member)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));

                Shape shape;
                if (member is System.Reflection.PropertyInfo property)
                    shape = Shape.FromClrType(property.PropertyType);
                else if (member is System.Reflection.FieldInfo field)
                    shape = Shape.FromClrType(field.FieldType);
                else
                    return UnsupportedComparisonPeerShape.Instance;

                if (shape.IsAny)
                    return UnsupportedComparisonPeerShape.Instance;

                return new DeclaredComparisonPeerShape(shape);
            }

            internal abstract ComparisonRuleOperands ToOperands(ValidationFieldPath path);
        }

        private sealed class UnsupportedComparisonPeerShape : ComparisonPeerShape
        {
            internal static UnsupportedComparisonPeerShape Instance { get; } =
                new UnsupportedComparisonPeerShape();

            private UnsupportedComparisonPeerShape() { }

            internal override ComparisonRuleOperands ToOperands(ValidationFieldPath path)
            {
                if (path == null) throw new ArgumentNullException(nameof(path));
                return new SkippedComparisonRuleOperands(ClientRuleProjectionSkipReason.UnsupportedPeerShape);
            }
        }

        private sealed class DeclaredComparisonPeerShape : ComparisonPeerShape
        {
            private readonly Shape _shape;

            internal DeclaredComparisonPeerShape(Shape shape)
            {
                _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            }

            internal override ComparisonRuleOperands ToOperands(ValidationFieldPath path)
            {
                if (path == null) throw new ArgumentNullException(nameof(path));
                return PeerComparisonRuleOperands.For(ComparisonPeerField.Of(path, _shape));
            }
        }

        private sealed class PeerComparisonRuleOperands : ProjectableComparisonRuleOperands
        {
            private readonly ComparisonPeerField _field;

            private PeerComparisonRuleOperands(ComparisonPeerField field)
            {
                _field = field ?? throw new ArgumentNullException(nameof(field));
            }

            internal static PeerComparisonRuleOperands For(ComparisonPeerField field) =>
                new PeerComparisonRuleOperands(field);

            internal override ValidationRuleName NotEqualRule => ValidationRuleName.NotEqualTo;

            internal override ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return For(_field.PrefixedBy(prefix));
            }

            internal override ValidationRuleDetails DetailsFor(ValidationRuleCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                return ValidationRuleDetails.WithPeerField(_field.Path, condition, _field.Shape);
            }

            internal override string Message(
                string displayName,
                string fieldVerb,
                string constraintVerb)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                if (fieldVerb == null) throw new ArgumentNullException(nameof(fieldVerb));
                return $"'{displayName}' {fieldVerb} '{Humanize(_field.Path)}'.";
            }

            internal override string NotEqualMessage(string displayName)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                return $"'{displayName}' must not match '{Humanize(_field.Path)}'.";
            }
        }

        private sealed class LiteralComparisonRuleOperands : ProjectableComparisonRuleOperands
        {
            private readonly object? _constraint;
            private readonly Shape _shape;

            internal LiteralComparisonRuleOperands(object? value)
            {
                var literal = ComparisonLiteralConstraint.From(value);
                _constraint = literal.Value;
                _shape = literal.Shape;
            }

            internal override ValidationRuleName NotEqualRule => ValidationRuleName.NotEqual;

            internal override ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return this;
            }

            internal override ValidationRuleDetails DetailsFor(ValidationRuleCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                return ValidationRuleDetails.WithConstraint(_constraint, condition, _shape);
            }

            internal override string Message(
                string displayName,
                string fieldVerb,
                string constraintVerb)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                if (constraintVerb == null) throw new ArgumentNullException(nameof(constraintVerb));
                return $"'{displayName}' {constraintVerb} {_constraint}.";
            }

            internal override string NotEqualMessage(string displayName)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                return $"'{displayName}' must not equal '{_constraint}'.";
            }
        }

        private sealed class ComparisonLiteralConstraint
        {
            private ComparisonLiteralConstraint(object? value, Shape shape)
            {
                Value = value;
                Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            }

            internal object? Value { get; }
            internal Shape Shape { get; }

            internal static ComparisonLiteralConstraint From(object? value)
            {
                if (value == null)
                    return new ComparisonLiteralConstraint(null, Shape.None);

                var shape = Shape.FromClrType(value.GetType());
                var serialized = SerializeForPlan(value, shape);
                return new ComparisonLiteralConstraint(serialized, shape);
            }

            private static object SerializeForPlan(object value, Shape shape)
            {
                var shouldSerializeDateLiteral = shape == Shape.Date;
                if (shouldSerializeDateLiteral)
                    return SerializeDateConstraint(value);

                return value;
            }
        }
    }
}
