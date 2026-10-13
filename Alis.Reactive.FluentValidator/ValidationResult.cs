using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>One rule a value broke: the field it names and what the rule says.</summary>
    public sealed class ValidationFailure
    {
        internal ValidationFailure(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        /// <summary>The field's path in the model, as model binding names it: <c>Address.City</c>, <c>Lines[0].Qty</c>.</summary>
        public string PropertyName { get; }

        /// <summary>The rule's message.</summary>
        public string ErrorMessage { get; }

        /// <inheritdoc />
        public override string ToString() => $"{PropertyName}: {ErrorMessage}";
    }

    /// <summary>What a validator found: every rule the model broke, in the order the rules are written.</summary>
    public sealed class ValidationResult
    {
        internal ValidationResult(IReadOnlyList<ValidationFailure> errors)
        {
            Errors = errors;
        }

        /// <summary>Whether the model broke no rule.</summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>Every broken rule, in the order the rules are written.</summary>
        public IReadOnlyList<ValidationFailure> Errors { get; }

        /// <summary>
        /// The messages by field, in the order the rules are written: what a 400 response sends as
        /// <c>errors</c> for the page to show beside each field.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> ErrorsByField =>
            Errors.GroupBy(error => error.PropertyName, StringComparer.Ordinal)
                .ToDictionary(messages => messages.Key, messages => messages.Select(error => error.ErrorMessage).ToArray(), StringComparer.Ordinal);
    }
}
