using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>Rendered validation boundary that owns extracted validation output.</summary>
    public sealed class ValidationContainerId : IEquatable<ValidationContainerId>
    {
        private ValidationContainerId(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static ValidationContainerId Of(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "A validation container id is required so extracted rules can be attached to the rendered validation boundary.",
                    nameof(value));
            }

            return new ValidationContainerId(value);
        }

        public bool Equals(ValidationContainerId? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => obj is ValidationContainerId other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;
    }

    /// <summary>Request passed from plan resolution to a validation integration.</summary>
    public sealed class ValidationExtractionRequest
    {
        private ValidationExtractionRequest(Type validatorType, ValidationContainerId validationContainer)
        {
            ValidatorType = validatorType ?? throw new ArgumentNullException(nameof(validatorType));
            ValidationContainer = validationContainer ?? throw new ArgumentNullException(nameof(validationContainer));
        }

        public Type ValidatorType { get; }
        public ValidationContainerId ValidationContainer { get; }

        public static ValidationExtractionRequest For(Type validatorType, string validationContainerId) =>
            new ValidationExtractionRequest(validatorType, ValidationContainerId.Of(validationContainerId));
    }

    /// <summary>Complete extraction outcome split by projected browser fields and skipped browser projections.</summary>
    public sealed class ValidationExtractionReport
    {
        public ValidationExtractionReport(
            ValidationContainerId validationContainer,
            IReadOnlyList<ValidationField> clientFields,
            IReadOnlyList<SkippedClientRuleExtraction> skippedClientRules)
        {
            ValidationContainer = validationContainer ?? throw new ArgumentNullException(nameof(validationContainer));
            if (clientFields == null) throw new ArgumentNullException(nameof(clientFields));
            if (skippedClientRules == null) throw new ArgumentNullException(nameof(skippedClientRules));

            ClientFields = Snapshot(clientFields, nameof(clientFields));
            SkippedClientRules = Snapshot(skippedClientRules, nameof(skippedClientRules));
        }

        public ValidationContainerId ValidationContainer { get; }
        public IReadOnlyList<ValidationField> ClientFields { get; }
        public IReadOnlyList<SkippedClientRuleExtraction> SkippedClientRules { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> items, string parameterName)
            where T : class
        {
            var snapshot = new List<T>(items.Count);
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentException("Validation extraction report items must not be null.", parameterName);

                snapshot.Add(item);
            }

            return snapshot;
        }

        public static ValidationExtractionReport ForClientFields(
            ValidationExtractionRequest request,
            IReadOnlyList<ValidationField> clientFields)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new ValidationExtractionReport(
                request.ValidationContainer,
                clientFields,
                Array.Empty<SkippedClientRuleExtraction>());
        }
    }

    /// <summary>Rule intentionally omitted from the browser projection because no deterministic client rule was proven.</summary>
    public sealed class SkippedClientRuleExtraction
    {
        private SkippedClientRuleExtraction(
            ValidationFieldPath fieldPath,
            string validatorName,
            ClientRuleExtractionSkipReason reason)
        {
            if (string.IsNullOrWhiteSpace(validatorName))
            {
                throw new ArgumentException(
                    "A skipped client validation rule must name the validator that could not be projected for the browser.",
                    nameof(validatorName));
            }

            FieldPath = fieldPath ?? throw new ArgumentNullException(nameof(fieldPath));
            ValidatorName = validatorName;
            Reason = reason;
        }

        public string FieldName => FieldPath.Value;
        public string ValidatorName { get; }
        public ClientRuleExtractionSkipReason Reason { get; }

        internal ValidationFieldPath FieldPath { get; }

        public static SkippedClientRuleExtraction ForField(
            string fieldName,
            string validatorName,
            ClientRuleExtractionSkipReason reason) =>
            new SkippedClientRuleExtraction(ValidationFieldPath.Of(fieldName), validatorName, reason);

        internal static SkippedClientRuleExtraction For(
            ValidationFieldPath fieldPath,
            string validatorName,
            ClientRuleExtractionSkipReason reason) =>
            new SkippedClientRuleExtraction(fieldPath, validatorName, reason);
    }

    /// <summary>Why a validation rule was not projected into browser validation.</summary>
    public enum ClientRuleExtractionSkipReason
    {
        FluentValidationConditionWithoutClientGuard,
        RuleComponentCondition,
        MissingRangeEndpoint,
        CrossObjectPeerComparison,
        UnknownPeerFieldScope,
        UnsupportedPeerShape,
        UnsupportedComparisonOperator,
        UnsupportedValidator,
        MissingRegexExpression
    }
}
