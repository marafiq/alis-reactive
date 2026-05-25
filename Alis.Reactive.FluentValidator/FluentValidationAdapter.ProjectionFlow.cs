using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Internal;
using Alis.Reactive.Validation;
using Shape = Alis.Reactive.PlanModel.Shape;

namespace Alis.Reactive.FluentValidator
{
    public sealed partial class FluentValidationAdapter
    {
        private sealed class ValidatorProjectionFrame
        {
            private ValidatorProjectionFrame(
                ValidationFieldPath prefix,
                FluentValidationProjectionDraft projection,
                Func<Type, IValidator?> factory,
                ClientConditionCatalog clientConditions,
                ValidationRuleCondition parentCondition)
            {
                Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
                Projection = projection ?? throw new ArgumentNullException(nameof(projection));
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
                ClientConditions = clientConditions ?? throw new ArgumentNullException(nameof(clientConditions));
                ParentCondition = parentCondition ?? throw new ArgumentNullException(nameof(parentCondition));
            }

            internal ValidationFieldPath Prefix { get; }
            internal FluentValidationProjectionDraft Projection { get; }
            internal Func<Type, IValidator?> Factory { get; }
            internal ClientConditionCatalog ClientConditions { get; }
            internal ValidationRuleCondition ParentCondition { get; }

            internal static ValidatorProjectionFrame Root(
                ValidationFieldPath prefix,
                FluentValidationProjectionDraft projection,
                Func<Type, IValidator?> factory,
                ClientConditionCatalog clientConditions) =>
                new ValidatorProjectionFrame(prefix, projection, factory, clientConditions, ValidationRuleCondition.Always);

            internal ValidatorProjectionFrame WithParentCondition(ValidationRuleCondition parentCondition) =>
                new ValidatorProjectionFrame(Prefix, Projection, Factory, ClientConditions, parentCondition);

            internal ValidatorProjectionFrame ForNestedValidator(IValidator nested) =>
                new ValidatorProjectionFrame(
                    Prefix,
                    Projection,
                    Factory,
                    ClientConditionCatalog.From(nested),
                    ParentCondition);

            internal ValidatorProjectionFrame ForNestedValidator(
                IValidator nested,
                ValidationFieldPath prefix,
                ValidationRuleCondition parentCondition) =>
                new ValidatorProjectionFrame(
                    prefix,
                    Projection,
                    Factory,
                    ClientConditionCatalog.From(nested),
                    parentCondition);
        }

        private sealed class ClientConditionCatalog
        {
            private readonly IReadOnlyDictionary<IValidationRule, ClientConditionProjection> _conditions;

            private ClientConditionCatalog(IReadOnlyDictionary<IValidationRule, ClientConditionProjection> conditions)
            {
                _conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
            }

            internal static ClientConditionCatalog Empty { get; } =
                new ClientConditionCatalog(new Dictionary<IValidationRule, ClientConditionProjection>());

            internal static ClientConditionCatalog From(IValidator validator)
            {
                if (validator == null) throw new ArgumentNullException(nameof(validator));
                var exposesClientConditions = validator is IClientConditionSource;
                if (!exposesClientConditions)
                    return Empty;

                var source = (IClientConditionSource)validator;
                return new ClientConditionCatalog(source.ClientConditions);
            }

            internal ClientConditionMatch Find(IValidationRule rule)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));

                if (!_conditions.TryGetValue(rule, out var condition))
                    return ClientConditionMatch.Missing;

                return condition.Match(
                    ClientConditionMatch.Found,
                    ClientConditionMatch.Skipped);
            }
        }

        private abstract class ClientConditionMatch
        {
            private protected ClientConditionMatch() { }

            internal static ClientConditionMatch Missing { get; } =
                new FluentValidationConditionWithoutClientGuard();

            internal static ClientConditionMatch Found(
                FieldCondition condition,
                IReadOnlyList<ClientValidationFieldReference> fields)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                if (fields == null) throw new ArgumentNullException(nameof(fields));
                return new MatchedClientCondition(condition, fields);
            }

            internal static ClientConditionMatch Skipped(ClientRuleProjectionSkipReason reason) =>
                new SkippedClientCondition(reason);

            internal abstract RuleProjectionScope ToRuleProjectionScope(ValidatorProjectionFrame frame);

            private sealed class FluentValidationConditionWithoutClientGuard : ClientConditionMatch
            {
                internal override RuleProjectionScope ToRuleProjectionScope(ValidatorProjectionFrame frame)
                {
                    if (frame == null) throw new ArgumentNullException(nameof(frame));
                    return RuleProjectionScope.SkipClientProjection(ClientRuleProjectionSkipReason.FluentValidationConditionWithoutClientGuard);
                }
            }

            private sealed class MatchedClientCondition : ClientConditionMatch
            {
                private readonly FieldCondition _condition;
                private readonly IReadOnlyList<ClientValidationFieldReference> _fields;

                internal MatchedClientCondition(
                    FieldCondition condition,
                    IReadOnlyList<ClientValidationFieldReference> fields)
                {
                    _condition = condition ?? throw new ArgumentNullException(nameof(condition));
                    _fields = ClientValidationGuardFields.From(fields);
                }

                internal override RuleProjectionScope ToRuleProjectionScope(ValidatorProjectionFrame frame)
                {
                    if (frame == null) throw new ArgumentNullException(nameof(frame));

                    foreach (var field in _fields)
                        frame.Projection.EnsureField(field.PrefixedBy(frame.Prefix));

                    var prefixBinding = new FieldConditionPrefixBinding(
                        frame.Prefix,
                        frame.Projection.EnsureField);
                    var resolved = _condition.PrefixWith(prefixBinding);
                    return RuleProjectionScope.ClientSide(resolved);
                }
            }

            private sealed class SkippedClientCondition : ClientConditionMatch
            {
                private readonly ClientRuleProjectionSkipReason _reason;

                internal SkippedClientCondition(ClientRuleProjectionSkipReason reason)
                {
                    _reason = reason;
                }

                internal override RuleProjectionScope ToRuleProjectionScope(ValidatorProjectionFrame frame)
                {
                    if (frame == null) throw new ArgumentNullException(nameof(frame));
                    return RuleProjectionScope.SkipClientProjection(_reason);
                }
            }
        }

        private abstract class RuleProjectionScope
        {
            private protected RuleProjectionScope() { }

            internal static RuleProjectionScope Unconditional { get; } =
                new ProjectedRuleProjectionScope(ValidationRuleCondition.Always);

            internal static RuleProjectionScope SkipClientProjection(ClientRuleProjectionSkipReason reason) =>
                new SkippedRuleProjectionScope(reason);

            internal static RuleProjectionScope ClientSide(FieldCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                return new ProjectedRuleProjectionScope(ValidationRuleCondition.When(condition));
            }

            internal static RuleProjectionScope For(
                IValidationRule rule,
                ValidatorProjectionFrame frame)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));
                if (frame == null) throw new ArgumentNullException(nameof(frame));
                return ResolveRuleProjectionScope(rule, frame);
            }

            internal abstract void Project(IValidationRule rule, ValidatorProjectionFrame frame);

            private sealed class SkippedRuleProjectionScope : RuleProjectionScope
            {
                private readonly ClientRuleProjectionSkipReason _reason;

                internal SkippedRuleProjectionScope(ClientRuleProjectionSkipReason reason)
                {
                    _reason = reason;
                }

                internal override void Project(IValidationRule rule, ValidatorProjectionFrame frame)
                {
                    if (rule == null) throw new ArgumentNullException(nameof(rule));
                    if (frame == null) throw new ArgumentNullException(nameof(frame));

                    var ruleHasNoPropertyName = string.IsNullOrEmpty(rule.PropertyName);
                    if (ruleHasNoPropertyName) return;

                    var target = ValidationRuleTarget.From(frame.Prefix, rule);
                    frame.Projection.RecordSkippedRule(SkippedClientRuleFor(target.FullPath, rule, _reason));
                }
            }

            private sealed class ProjectedRuleProjectionScope : RuleProjectionScope
            {
                private readonly ValidationRuleCondition _condition;

                internal ProjectedRuleProjectionScope(ValidationRuleCondition condition)
                {
                    _condition = condition ?? throw new ArgumentNullException(nameof(condition));
                }

                internal override void Project(IValidationRule rule, ValidatorProjectionFrame frame)
                {
                    if (rule == null) throw new ArgumentNullException(nameof(rule));
                    if (frame == null) throw new ArgumentNullException(nameof(frame));

                    var ruleHasNoPropertyName = string.IsNullOrEmpty(rule.PropertyName);
                    if (ruleHasNoPropertyName)
                    {
                        ProcessIncludeRule(rule, frame.WithParentCondition(ConditionForChild(frame.ParentCondition)));
                        return;
                    }

                    var target = ValidationRuleTarget.From(frame.Prefix, rule);
                    ProcessComponents(rule, target, frame, _condition);
                }

                private ValidationRuleCondition ConditionForChild(ValidationRuleCondition parentCondition)
                {
                    if (parentCondition == null) throw new ArgumentNullException(nameof(parentCondition));
                    return parentCondition.Combine(_condition);
                }
            }
        }

        private abstract class ClientRuleProjection
        {
            internal static ClientRuleProjection SkipClientProjection(ClientRuleProjectionSkipReason reason) =>
                new SkippedClientRuleProjection(reason);

            internal static ClientRuleProjection Project(ProjectedClientValidationRule rule)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));
                return new ProjectedClientRule(rule);
            }

            internal abstract void AddTo(
                List<ProjectedClientValidationRule> rules,
                RuleComponentMapping mapping,
                FluentValidationProjectionDraft projection);

            private sealed class SkippedClientRuleProjection : ClientRuleProjection
            {
                private readonly ClientRuleProjectionSkipReason _reason;

                internal SkippedClientRuleProjection(ClientRuleProjectionSkipReason reason)
                {
                    _reason = reason;
                }

                internal override void AddTo(
                    List<ProjectedClientValidationRule> rules,
                    RuleComponentMapping mapping,
                    FluentValidationProjectionDraft projection)
                {
                    if (rules == null) throw new ArgumentNullException(nameof(rules));
                    if (mapping == null) throw new ArgumentNullException(nameof(mapping));
                    if (projection == null) throw new ArgumentNullException(nameof(projection));

                    projection.RecordSkippedRule(SkippedClientRuleFor(
                        mapping.Target.FullPath,
                        mapping.Validator,
                        _reason));
                }
            }

            private sealed class ProjectedClientRule : ClientRuleProjection
            {
                private readonly ProjectedClientValidationRule _rule;

                internal ProjectedClientRule(ProjectedClientValidationRule rule)
                {
                    _rule = rule ?? throw new ArgumentNullException(nameof(rule));
                }

                internal override void AddTo(
                    List<ProjectedClientValidationRule> rules,
                    RuleComponentMapping mapping,
                    FluentValidationProjectionDraft projection)
                {
                    if (rules == null) throw new ArgumentNullException(nameof(rules));
                    if (mapping == null) throw new ArgumentNullException(nameof(mapping));
                    if (projection == null) throw new ArgumentNullException(nameof(projection));
                    rules.Add(_rule);
                }
            }
        }

        private sealed class ProjectedClientValidationRule
        {
            public ValidationRuleName Rule { get; }
            public ValidationMessage Message { get; }
            public ValidationRuleDetails Details { get; }

            public ProjectedClientValidationRule(
                ValidationRuleName rule,
                ValidationMessage message,
                ValidationRuleDetails details)
            {
                Rule = rule ?? throw new ArgumentNullException(nameof(rule));
                Message = message ?? throw new ArgumentNullException(nameof(message));
                Details = details ?? throw new ArgumentNullException(nameof(details));
            }

            internal IEnumerable<ClientValidationFieldReference> PeerFieldReferences => Details.PeerFieldReferences;

            internal ValidationRule ToValidationRule() =>
                new ValidationRule(Rule, Message, Details);
        }

        private sealed class FluentValidationProjectionDraft
        {
            private readonly Dictionary<string, ProjectedClientValidationField> _fields =
                new Dictionary<string, ProjectedClientValidationField>(StringComparer.Ordinal);
            private readonly List<SkippedClientRuleProjection> _skippedClientRules =
                new List<SkippedClientRuleProjection>();

            internal void EnsureField(ValidationFieldPath fieldPath)
            {
                if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
                if (_fields.ContainsKey(fieldPath.Value)) return;
                _fields[fieldPath.Value] = new ProjectedClientValidationField(
                    fieldPath,
                    ProjectedFieldShapeEvidence.ModelMetadata);
            }

            internal void EnsureField(ClientValidationFieldReference field)
            {
                if (field == null) throw new ArgumentNullException(nameof(field));

                if (_fields.TryGetValue(field.Path.Value, out var existing))
                {
                    existing.RecordProjectedShape(field.Shape);
                    return;
                }

                _fields[field.Path.Value] = new ProjectedClientValidationField(
                    field.Path,
                    ProjectedFieldShapeEvidence.Projected(field.Shape));
            }

            internal void AddProjectedRules(ValidationFieldPath fieldPath, IEnumerable<ProjectedClientValidationRule> rules)
            {
                if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
                if (rules == null) throw new ArgumentNullException(nameof(rules));

                EnsureField(fieldPath);
                _fields[fieldPath.Value].Add(rules);
            }

            internal void AddProjectedRules(ClientValidationFieldReference field, IEnumerable<ProjectedClientValidationRule> rules)
            {
                if (field == null) throw new ArgumentNullException(nameof(field));
                if (rules == null) throw new ArgumentNullException(nameof(rules));

                EnsureField(field);
                _fields[field.Path.Value].Add(rules);
            }

            internal void RecordSkippedRule(SkippedClientRuleProjection rule)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));
                _skippedClientRules.Add(rule);
            }

            internal void EnsurePeerFields()
            {
                var peerFields = _fields.Values
                    .SelectMany(field => field.Rules)
                    .SelectMany(rule => rule.PeerFieldReferences)
                    .Where(field => !_fields.ContainsKey(field.Path.Value))
                    .ToList();

                foreach (var peerField in peerFields)
                    EnsureField(peerField);
            }

            internal ClientValidationProjection ToReport(ValidationContainerId validationContainer) =>
                new ClientValidationProjection(
                    validationContainer,
                    ToValidationFields(),
                    _skippedClientRules.ToList());

            private List<ClientValidationField> ToValidationFields()
            {
                var fields = new List<ClientValidationField>();
                foreach (var field in _fields.Values)
                {
                    var rules = new List<ValidationRule>();
                    foreach (var rule in field.Rules)
                    {
                        rules.Add(rule.ToValidationRule());
                    }

                    fields.Add(new ClientValidationField(
                        field.FieldPath,
                        field.ShapeEvidence.ToShapeSource(),
                        rules));
                }

                return fields;
            }
        }

        private sealed class ProjectedClientValidationField
        {
            private readonly List<ProjectedClientValidationRule> _rules = new List<ProjectedClientValidationRule>();
            private ProjectedFieldShapeEvidence _shapeEvidence;

            internal ProjectedClientValidationField(
                ValidationFieldPath fieldPath,
                ProjectedFieldShapeEvidence shapeEvidence)
            {
                FieldPath = fieldPath ?? throw new ArgumentNullException(nameof(fieldPath));
                _shapeEvidence = shapeEvidence ?? throw new ArgumentNullException(nameof(shapeEvidence));
            }

            internal ValidationFieldPath FieldPath { get; }
            internal IReadOnlyList<ProjectedClientValidationRule> Rules => _rules;
            internal ProjectedFieldShapeEvidence ShapeEvidence => _shapeEvidence;

            internal void Add(IEnumerable<ProjectedClientValidationRule> rules)
            {
                _rules.AddRange(rules);
            }

            internal void RecordProjectedShape(Shape shape)
            {
                if (shape == null) throw new ArgumentNullException(nameof(shape));
                _shapeEvidence = _shapeEvidence.Merge(shape, FieldPath);
            }
        }

        private abstract class ProjectedFieldShapeEvidence
        {
            private protected ProjectedFieldShapeEvidence() { }

            internal static ProjectedFieldShapeEvidence ModelMetadata { get; } =
                new ModelMetadataFieldShapeEvidence();

            internal static ProjectedFieldShapeEvidence Projected(Shape shape)
            {
                if (shape == null) throw new ArgumentNullException(nameof(shape));
                return new ProjectedFieldShape(shape);
            }

            internal abstract ClientValidationFieldShapeSource ToShapeSource();

            internal abstract ProjectedFieldShapeEvidence Merge(
                Shape shape,
                ValidationFieldPath fieldPath);

            private sealed class ModelMetadataFieldShapeEvidence : ProjectedFieldShapeEvidence
            {
                internal override ClientValidationFieldShapeSource ToShapeSource() =>
                    ClientValidationFieldShapeSource.ModelField;

                internal override ProjectedFieldShapeEvidence Merge(
                    Shape shape,
                    ValidationFieldPath fieldPath)
                {
                    if (shape == null) throw new ArgumentNullException(nameof(shape));
                    if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
                    return Projected(shape);
                }
            }

            private sealed class ProjectedFieldShape : ProjectedFieldShapeEvidence
            {
                private readonly Shape _shape;

                internal ProjectedFieldShape(Shape shape)
                {
                    _shape = shape ?? throw new ArgumentNullException(nameof(shape));
                }

                internal override ClientValidationFieldShapeSource ToShapeSource() =>
                    ClientValidationFieldShapeSource.Projected(_shape);

                internal override ProjectedFieldShapeEvidence Merge(
                    Shape shape,
                    ValidationFieldPath fieldPath)
                {
                    if (shape == null) throw new ArgumentNullException(nameof(shape));
                    if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));

                    if (!_shape.Equals(shape))
                    {
                        throw new InvalidOperationException(
                            $"Client validation field '{fieldPath.Value}' was projected with conflicting shapes: " +
                            $"'{_shape.Kind}' and '{shape.Kind}'.");
                    }

                    return this;
                }
            }
        }
    }
}
