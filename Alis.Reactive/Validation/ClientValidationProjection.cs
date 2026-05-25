using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>Rendered validation boundary that owns projected browser validation output.</summary>
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
                    "A validation container id is required so projected browser rules can be attached to the rendered validation boundary.",
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

    /// <summary>Request passed from plan resolution to a client validation projection source.</summary>
    public sealed class ClientValidationProjectionRequest
    {
        private ClientValidationProjectionRequest(Type validationSourceType, ValidationContainerId validationContainer)
        {
            ValidationSourceType = validationSourceType ?? throw new ArgumentNullException(nameof(validationSourceType));
            ValidationContainer = validationContainer ?? throw new ArgumentNullException(nameof(validationContainer));
        }

        public Type ValidationSourceType { get; }
        public ValidationContainerId ValidationContainer { get; }

        public static ClientValidationProjectionRequest For(Type validationSourceType, string validationContainerId) =>
            new ClientValidationProjectionRequest(validationSourceType, ValidationContainerId.Of(validationContainerId));
    }

    /// <summary>Complete client validation projection split by projected fields and skipped browser rules.</summary>
    public sealed class ClientValidationProjection
    {
        public ClientValidationProjection(
            ValidationContainerId validationContainer,
            IReadOnlyList<ClientValidationField> fields,
            IReadOnlyList<SkippedClientRuleProjection> skippedRules)
        {
            ValidationContainer = validationContainer ?? throw new ArgumentNullException(nameof(validationContainer));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (skippedRules == null) throw new ArgumentNullException(nameof(skippedRules));

            Fields = SnapshotFields(fields);
            SkippedRules = Snapshot(skippedRules, nameof(skippedRules));
        }

        public ValidationContainerId ValidationContainer { get; }
        public IReadOnlyList<ClientValidationField> Fields { get; }
        public IReadOnlyList<SkippedClientRuleProjection> SkippedRules { get; }

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> items, string parameterName)
            where T : class
        {
            var snapshot = new List<T>(items.Count);
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentException("Client validation projection items must not be null.", parameterName);

                snapshot.Add(item);
            }

            return snapshot;
        }

        private static IReadOnlyList<ClientValidationField> SnapshotFields(IReadOnlyList<ClientValidationField> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var snapshot = new List<ClientValidationField>(fields.Count);
            foreach (var field in fields)
            {
                if (field == null)
                    throw new ArgumentException("Client validation projection items must not be null.", nameof(fields));

                snapshot.Add(field.Snapshot());
            }

            return snapshot;
        }

        public static ClientValidationProjection ForFields(
            ClientValidationProjectionRequest request,
            IReadOnlyList<ClientValidationField> fields)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new ClientValidationProjection(
                request.ValidationContainer,
                fields,
                Array.Empty<SkippedClientRuleProjection>());
        }
    }

    /// <summary>Rule intentionally omitted from the browser projection because no deterministic client rule was proven.</summary>
    public sealed class SkippedClientRuleProjection
    {
        private SkippedClientRuleProjection(
            ValidationFieldPath fieldPath,
            string validatorName,
            ClientRuleProjectionSkipReason reason)
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
        public ClientRuleProjectionSkipReason Reason { get; }

        internal ValidationFieldPath FieldPath { get; }

        public static SkippedClientRuleProjection ForField(
            string fieldName,
            string validatorName,
            ClientRuleProjectionSkipReason reason) =>
            new SkippedClientRuleProjection(ValidationFieldPath.Of(fieldName), validatorName, reason);

        internal static SkippedClientRuleProjection For(
            ValidationFieldPath fieldPath,
            string validatorName,
            ClientRuleProjectionSkipReason reason) =>
            new SkippedClientRuleProjection(fieldPath, validatorName, reason);
    }

    /// <summary>Why a validation rule was not projected into browser validation.</summary>
    public enum ClientRuleProjectionSkipReason
    {
        FluentValidationConditionWithoutClientGuard,
        RuleComponentCondition,
        MissingRangeEndpoint,
        PeerComparisonRequiresExplicitProjection,
        UnsupportedComparisonOperator,
        UnsupportedValidator,
        MissingRegexExpression
    }
}
