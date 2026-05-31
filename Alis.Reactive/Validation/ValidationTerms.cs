using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    internal sealed class ValidationFieldPath : IEquatable<ValidationFieldPath>
    {
        private ValidationFieldPath(string value, ValidationFieldPathPolicy policy)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var namedFieldRequired = policy == ValidationFieldPathPolicy.RequireNamedField;
            var fieldPathIsEmpty = string.IsNullOrWhiteSpace(value);
            var namedFieldIsMissing = namedFieldRequired && fieldPathIsEmpty;
            if (namedFieldIsMissing)
                throw new ArgumentException("Validation field path must not be empty.", nameof(value));

            Segments = ParseSegments(value, policy);
            Value = value;
        }

        internal string Value { get; }
        internal bool IsEmpty => Value.Length == 0;
        internal IReadOnlyList<string> Segments { get; }

        internal static ValidationFieldPath Empty { get; } =
            new ValidationFieldPath("", ValidationFieldPathPolicy.AllowRootField);

        internal static ValidationFieldPath Of(string value) =>
            new ValidationFieldPath(value, ValidationFieldPathPolicy.RequireNamedField);

        internal ValidationFieldPath Append(string relativePath) =>
            Append(Of(relativePath));

        internal ValidationFieldPath Append(ValidationFieldPath relativePath)
        {
            if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));
            if (relativePath.IsEmpty) return this;
            if (IsEmpty) return relativePath;
            return Of(Value + "." + relativePath.Value);
        }

        public bool Equals(ValidationFieldPath? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => Equals(obj as ValidationFieldPath);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;

        private static IReadOnlyList<string> ParseSegments(
            string value,
            ValidationFieldPathPolicy policy)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var pathTargetsRoot = value.Length == 0;
            if (pathTargetsRoot)
            {
                if (policy == ValidationFieldPathPolicy.AllowRootField)
                    return Array.Empty<string>();

                throw new ArgumentException("Validation field path must not be empty.", nameof(value));
            }

            var segments = value.Split('.');
            for (var index = 0; index < segments.Length; index++)
            {
                var segment = segments[index];
                if (string.IsNullOrWhiteSpace(segment))
                    throw new ArgumentException(
                    $"Validation field path '{value}' contains an empty segment at index {index}.",
                        nameof(value));
            }

            return segments;
        }
    }

    internal enum ValidationFieldPathPolicy
    {
        RequireNamedField,
        AllowRootField
    }

    internal sealed class ValidationMessage : IEquatable<ValidationMessage>
    {
        private ValidationMessage(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal string Value { get; }

        internal static ValidationMessage Of(string value) =>
            new ValidationMessage(value);

        public bool Equals(ValidationMessage? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => Equals(obj as ValidationMessage);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;
    }

}
