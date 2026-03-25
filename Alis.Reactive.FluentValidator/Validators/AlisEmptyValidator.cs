using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Validators;

namespace Alis.Reactive.FluentValidator.Validators
{
    /// <summary>
    /// Marker interface for client-extractable Empty rule.
    /// FluentValidation's EmptyValidator has no public interface.
    /// </summary>
    public interface IEmptyValidator : IPropertyValidator { }

    /// <summary>
    /// Validates that a value IS empty (null, default, or empty string).
    /// Inverse of NotEmpty — used for conditional fields (e.g. "salary must be empty when not employed").
    /// </summary>
    public class EmptyValidator<T, TProperty> : PropertyValidator<T, TProperty>, IEmptyValidator
    {
        public override string Name => "EmptyValidator";

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            if (value == null) return true;
            if (value is string s) return string.IsNullOrEmpty(s);
            return EqualityComparer<TProperty>.Default.Equals(value, default!);
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
            => "'{PropertyName}' must be empty.";
    }

    public static class EmptyExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> IsEmpty<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder)
            => ruleBuilder.SetValidator(new EmptyValidator<T, TProperty>());
    }
}
