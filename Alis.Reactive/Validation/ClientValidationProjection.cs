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

    /// <summary>Complete client validation extraction split by projected fields.</summary>
    public sealed class ClientValidationProjection
    {
        public ClientValidationProjection(
            ValidationContainerId validationContainer,
            IReadOnlyList<ClientValidationField> fields)
        {
            ValidationContainer = validationContainer ?? throw new ArgumentNullException(nameof(validationContainer));
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            Fields = SnapshotFields(fields);
        }

        public ValidationContainerId ValidationContainer { get; }
        public IReadOnlyList<ClientValidationField> Fields { get; }

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
                fields);
        }
    }
}
