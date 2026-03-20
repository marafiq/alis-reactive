using System;
using FluentValidation;
using FluentValidation.Validators;

namespace Alis.Reactive.FluentValidator.Validators
{
    /// <summary>
    /// Interface for client-extractable ExclusiveBetween rule.
    /// FluentValidation's ExclusiveBetweenValidator has no distinguishing interface from InclusiveBetween.
    /// </summary>
    public interface IExclusiveBetweenValidator : IPropertyValidator
    {
        object From { get; }
        object To { get; }
    }

    /// <summary>
    /// Validates that a value is strictly between From and To (exclusive boundaries).
    /// </summary>
    public class ExclusiveBetweenValidator<T, TProperty> : PropertyValidator<T, TProperty>, IExclusiveBetweenValidator
        where TProperty : IComparable<TProperty>
    {
        public object From { get; }
        public object To { get; }

        private readonly TProperty _from;
        private readonly TProperty _to;

        public override string Name => "ExclusiveBetweenValidator";

        public ExclusiveBetweenValidator(TProperty from, TProperty to)
        {
            _from = from;
            _to = to;
            From = from!;
            To = to!;
        }

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            if (value == null) return true;
            return value.CompareTo(_from) > 0 && value.CompareTo(_to) < 0;
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
            => "'{PropertyName}' must be between {From} and {To} (exclusive).";
    }

    public static class ExclusiveBetweenExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> IsExclusiveBetween<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder, TProperty from, TProperty to)
            where TProperty : IComparable<TProperty>
            => ruleBuilder.SetValidator(new ExclusiveBetweenValidator<T, TProperty>(from, to));
    }
}
