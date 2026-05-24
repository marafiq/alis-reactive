using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Internal;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    public sealed partial class FluentValidationAdapter
    {
        private sealed class ValidatorExtractionFrame
        {
            private ValidatorExtractionFrame(
                ValidationFieldPath prefix,
                ExtractedValidationContract extractedForm,
                Func<Type, IValidator?> factory,
                ClientConditionCatalog clientConditions,
                ValidationRuleCondition parentCondition)
            {
                Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
                ExtractedForm = extractedForm ?? throw new ArgumentNullException(nameof(extractedForm));
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
                ClientConditions = clientConditions ?? throw new ArgumentNullException(nameof(clientConditions));
                ParentCondition = parentCondition ?? throw new ArgumentNullException(nameof(parentCondition));
            }

            internal ValidationFieldPath Prefix { get; }
            internal ExtractedValidationContract ExtractedForm { get; }
            internal Func<Type, IValidator?> Factory { get; }
            internal ClientConditionCatalog ClientConditions { get; }
            internal ValidationRuleCondition ParentCondition { get; }

            internal static ValidatorExtractionFrame Root(
                ValidationFieldPath prefix,
                ExtractedValidationContract extractedForm,
                Func<Type, IValidator?> factory,
                ClientConditionCatalog clientConditions) =>
                new ValidatorExtractionFrame(prefix, extractedForm, factory, clientConditions, ValidationRuleCondition.Always);

            internal ValidatorExtractionFrame WithParentCondition(ValidationRuleCondition parentCondition) =>
                new ValidatorExtractionFrame(Prefix, ExtractedForm, Factory, ClientConditions, parentCondition);

            internal ValidatorExtractionFrame ForNestedValidator(IValidator nested) =>
                new ValidatorExtractionFrame(
                    Prefix,
                    ExtractedForm,
                    Factory,
                    ClientConditionCatalog.From(nested),
                    ParentCondition);

            internal ValidatorExtractionFrame ForNestedValidator(
                IValidator nested,
                ValidationFieldPath prefix,
                ValidationRuleCondition parentCondition) =>
                new ValidatorExtractionFrame(
                    prefix,
                    ExtractedForm,
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

            internal static ClientConditionMatch Found(FieldCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                return new MatchedClientCondition(condition);
            }

            internal static ClientConditionMatch Skipped(ClientRuleExtractionSkipReason reason) =>
                new SkippedClientCondition(reason);

            internal abstract RuleExtractionScope ToRuleExtractionScope(ValidatorExtractionFrame frame);

            private sealed class FluentValidationConditionWithoutClientGuard : ClientConditionMatch
            {
                internal override RuleExtractionScope ToRuleExtractionScope(ValidatorExtractionFrame frame)
                {
                    if (frame == null) throw new ArgumentNullException(nameof(frame));
                    return RuleExtractionScope.SkipClientProjection(ClientRuleExtractionSkipReason.FluentValidationConditionWithoutClientGuard);
                }
            }

            private sealed class MatchedClientCondition : ClientConditionMatch
            {
                private readonly FieldCondition _condition;

                internal MatchedClientCondition(FieldCondition condition)
                {
                    _condition = condition ?? throw new ArgumentNullException(nameof(condition));
                }

                internal override RuleExtractionScope ToRuleExtractionScope(ValidatorExtractionFrame frame)
                {
                    if (frame == null) throw new ArgumentNullException(nameof(frame));

                    var prefixBinding = new FieldConditionPrefixBinding(
                        frame.Prefix,
                        frame.ExtractedForm.EnsureField);
                    var resolved = _condition.PrefixWith(prefixBinding);
                    return RuleExtractionScope.ClientSide(resolved);
                }
            }

            private sealed class SkippedClientCondition : ClientConditionMatch
            {
                private readonly ClientRuleExtractionSkipReason _reason;

                internal SkippedClientCondition(ClientRuleExtractionSkipReason reason)
                {
                    _reason = reason;
                }

                internal override RuleExtractionScope ToRuleExtractionScope(ValidatorExtractionFrame frame)
                {
                    if (frame == null) throw new ArgumentNullException(nameof(frame));
                    return RuleExtractionScope.SkipClientProjection(_reason);
                }
            }
        }

        private abstract class RuleExtractionScope
        {
            private protected RuleExtractionScope() { }

            internal static RuleExtractionScope Unconditional { get; } =
                new ProjectedRuleExtractionScope(ValidationRuleCondition.Always);

            internal static RuleExtractionScope SkipClientProjection(ClientRuleExtractionSkipReason reason) =>
                new SkippedRuleExtractionScope(reason);

            internal static RuleExtractionScope ClientSide(FieldCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                return new ProjectedRuleExtractionScope(ValidationRuleCondition.When(condition));
            }

            internal static RuleExtractionScope For(
                IValidationRule rule,
                ValidatorExtractionFrame frame)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));
                if (frame == null) throw new ArgumentNullException(nameof(frame));
                return ResolveRuleExtractionScope(rule, frame);
            }

            internal abstract void Extract(IValidationRule rule, ValidatorExtractionFrame frame);

            private sealed class SkippedRuleExtractionScope : RuleExtractionScope
            {
                private readonly ClientRuleExtractionSkipReason _reason;

                internal SkippedRuleExtractionScope(ClientRuleExtractionSkipReason reason)
                {
                    _reason = reason;
                }

                internal override void Extract(IValidationRule rule, ValidatorExtractionFrame frame)
                {
                    if (rule == null) throw new ArgumentNullException(nameof(rule));
                    if (frame == null) throw new ArgumentNullException(nameof(frame));

                    var ruleHasNoPropertyName = string.IsNullOrEmpty(rule.PropertyName);
                    if (ruleHasNoPropertyName) return;

                    var target = ValidationRuleTarget.From(frame.Prefix, rule);
                    frame.ExtractedForm.AddSkippedClientRule(SkippedClientRuleFor(target.FullPath, rule, _reason));
                }
            }

            private sealed class ProjectedRuleExtractionScope : RuleExtractionScope
            {
                private readonly ValidationRuleCondition _condition;

                internal ProjectedRuleExtractionScope(ValidationRuleCondition condition)
                {
                    _condition = condition ?? throw new ArgumentNullException(nameof(condition));
                }

                internal override void Extract(IValidationRule rule, ValidatorExtractionFrame frame)
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
            internal static ClientRuleProjection SkipClientProjection(ClientRuleExtractionSkipReason reason) =>
                new SkippedClientRuleProjection(reason);

            internal static ClientRuleProjection Project(ExtractedRule rule)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));
                return new ProjectedClientRule(rule);
            }

            internal abstract void AddTo(
                List<ExtractedRule> rules,
                RuleComponentMapping mapping,
                ExtractedValidationContract extractedForm);

            private sealed class SkippedClientRuleProjection : ClientRuleProjection
            {
                private readonly ClientRuleExtractionSkipReason _reason;

                internal SkippedClientRuleProjection(ClientRuleExtractionSkipReason reason)
                {
                    _reason = reason;
                }

                internal override void AddTo(
                    List<ExtractedRule> rules,
                    RuleComponentMapping mapping,
                    ExtractedValidationContract extractedForm)
                {
                    if (rules == null) throw new ArgumentNullException(nameof(rules));
                    if (mapping == null) throw new ArgumentNullException(nameof(mapping));
                    if (extractedForm == null) throw new ArgumentNullException(nameof(extractedForm));

                    extractedForm.AddSkippedClientRule(SkippedClientRuleFor(
                        mapping.Target.FullPath,
                        mapping.Validator,
                        _reason));
                }
            }

            private sealed class ProjectedClientRule : ClientRuleProjection
            {
                private readonly ExtractedRule _rule;

                internal ProjectedClientRule(ExtractedRule rule)
                {
                    _rule = rule ?? throw new ArgumentNullException(nameof(rule));
                }

                internal override void AddTo(
                    List<ExtractedRule> rules,
                    RuleComponentMapping mapping,
                    ExtractedValidationContract extractedForm)
                {
                    if (rules == null) throw new ArgumentNullException(nameof(rules));
                    if (mapping == null) throw new ArgumentNullException(nameof(mapping));
                    if (extractedForm == null) throw new ArgumentNullException(nameof(extractedForm));
                    rules.Add(_rule);
                }
            }
        }

        private sealed class ExtractedRule
        {
            public ValidationRuleName Rule { get; }
            public ValidationMessage Message { get; }
            public ValidationRuleDetails Details { get; }

            public ExtractedRule(
                ValidationRuleName rule,
                ValidationMessage message,
                ValidationRuleDetails details)
            {
                Rule = rule ?? throw new ArgumentNullException(nameof(rule));
                Message = message ?? throw new ArgumentNullException(nameof(message));
                Details = details ?? throw new ArgumentNullException(nameof(details));
            }

            internal IEnumerable<ValidationFieldPath> PeerFields => Details.PeerFields;

            internal ValidationRule ToValidationRule() =>
                new ValidationRule(Rule, Message, Details);
        }

        private sealed class ExtractedValidationContract
        {
            private readonly Dictionary<string, FieldRuleSet> _fields =
                new Dictionary<string, FieldRuleSet>(StringComparer.Ordinal);
            private readonly List<SkippedClientRuleExtraction> _skippedClientRules =
                new List<SkippedClientRuleExtraction>();

            internal void EnsureField(ValidationFieldPath fieldPath)
            {
                if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
                if (_fields.ContainsKey(fieldPath.Value)) return;
                _fields[fieldPath.Value] = new FieldRuleSet(fieldPath);
            }

            internal void AddRules(ValidationFieldPath fieldPath, IEnumerable<ExtractedRule> rules)
            {
                if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
                if (rules == null) throw new ArgumentNullException(nameof(rules));

                EnsureField(fieldPath);
                _fields[fieldPath.Value].Add(rules);
            }

            internal void AddSkippedClientRule(SkippedClientRuleExtraction rule)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));
                _skippedClientRules.Add(rule);
            }

            internal void EnsurePeerFields()
            {
                var peerFields = _fields.Values
                    .SelectMany(field => field.Rules)
                    .SelectMany(rule => rule.PeerFields)
                    .Where(field => !_fields.ContainsKey(field.Value))
                    .ToList();

                foreach (var peerField in peerFields)
                    EnsureField(peerField);
            }

            internal ValidationExtractionReport ToReport(ValidationContainerId validationContainer) =>
                new ValidationExtractionReport(
                    validationContainer,
                    ToValidationFields(),
                    _skippedClientRules.ToList());

            private List<ValidationField> ToValidationFields()
            {
                var fields = new List<ValidationField>();
                foreach (var field in _fields.Values)
                {
                    var rules = new List<ValidationRule>();
                    foreach (var rule in field.Rules)
                    {
                        rules.Add(rule.ToValidationRule());
                    }

                    fields.Add(new ValidationField(field.FieldPath, rules));
                }

                return fields;
            }
        }

        private sealed class FieldRuleSet
        {
            private readonly List<ExtractedRule> _rules = new List<ExtractedRule>();

            internal FieldRuleSet(ValidationFieldPath fieldPath)
            {
                FieldPath = fieldPath ?? throw new ArgumentNullException(nameof(fieldPath));
            }

            internal ValidationFieldPath FieldPath { get; }
            internal IReadOnlyList<ExtractedRule> Rules => _rules;

            internal void Add(IEnumerable<ExtractedRule> rules)
            {
                _rules.AddRange(rules);
            }
        }
    }
}
