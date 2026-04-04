using System;
using System.Collections.Generic;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    internal sealed class ValidationPlanCompiler
    {
        private readonly Dictionary<RequestPlan, Type> _pendingValidators =
            new Dictionary<RequestPlan, Type>();

        internal bool HasPendingValidators => _pendingValidators.Count > 0;

        internal void TrackPendingValidator(RequestPlan request, Type validatorType)
        {
            _pendingValidators[request] = validatorType;
        }

        internal void ResolvePending(IFormValidationExtractor? extractor)
        {
            if (_pendingValidators.Count == 0)
                return;

            if (extractor == null)
            {
                throw new InvalidOperationException(
                    "One or more requests use Validate<TValidator>() but no validation extractor is registered. " +
                    "Call ReactivePlanConfig.UseFormValidationExtractor(...) at app startup.");
            }

            foreach (var kvp in _pendingValidators)
            {
                var request = kvp.Key;
                var validatorType = kvp.Value;
                var existingValidation = request.Validation;
                if (existingValidation == null)
                    continue;

                var extracted = extractor.ExtractRules(validatorType, existingValidation.FormId);
                if (extracted == null)
                {
                    throw new InvalidOperationException(
                        $"Validator '{validatorType.Name}' produced no client rules for form '{existingValidation.FormId}'. " +
                        "Ensure the validator is registered in the factory and has extractable rules.");
                }

                request.Validation = Convert(extracted);
            }

            _pendingValidators.Clear();
        }

        internal static RequestValidation Convert(FormValidation validation)
        {
            var fields = new List<RequestValidationField>();
            foreach (var field in validation.Fields)
            {
                var rules = new List<RequestValidationRule>();
                foreach (var rule in field.Rules)
                {
                    var converted = new RequestValidationRule(rule.Rule, rule.Message)
                    {
                        Constraint = rule.Constraint,
                        OtherBinding = rule.Field,
                        As = string.IsNullOrEmpty(rule.ShapeToken) ? null : ValueShapeFactory.FromToken(rule.ShapeToken),
                        When = ConvertCondition(rule.When)
                    };

                    rules.Add(converted);
                }

                fields.Add(new RequestValidationField(field.FieldName, rules));
            }

            return new RequestValidation(validation.FormId, fields);
        }

        private static PlanPredicate? ConvertCondition(ValidationCondition? condition)
        {
            if (condition == null)
                return null;

            var predicate = new ComparePredicate(new BindingValueExpr(condition.Field), condition.Op);
            if (condition.Value != null)
                predicate.Right = new LiteralValueExpr(condition.Value);

            return predicate;
        }
    }
}
