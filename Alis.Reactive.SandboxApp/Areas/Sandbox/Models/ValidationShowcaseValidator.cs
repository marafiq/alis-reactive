using System.Collections.Generic;
using FluentValidation;
using Alis.Reactive.Validation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ValidationShowcaseValidator : AbstractValidator<ValidationShowcaseModel>,
        IConditionalRuleProvider
    {
        public ValidationShowcaseValidator()
        {
            // Required + MaxLength
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

            // Required + Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            // Range (0–120)
            RuleFor(x => x.Age)
                .InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120.");

            // Regex
            RuleFor(x => x.Phone)
                .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");

            // Min + Max
            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0m).WithMessage("Salary must be at least 0.")
                .LessThanOrEqualTo(500000m).WithMessage("Salary must be at most 500,000.");

            // MinLength on Password
            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

            // Nested address
            RuleFor(x => x.Address!).SetValidator(new ValidationAddressValidator());
        }

        public IReadOnlyList<ConditionalRuleMetadata> GetConditionalRules()
        {
            return new List<ConditionalRuleMetadata>
            {
                new ConditionalRuleMetadata(
                    "JobTitle",
                    "required",
                    "Job title is required when employed.",
                    new ValidationCondition("IsEmployed", "truthy"))
            };
        }
    }

    public class ValidationAddressValidator : AbstractValidator<ValidationAddress>
    {
        public ValidationAddressValidator()
        {
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.ZipCode)
                .MinimumLength(5).WithMessage("Zip code must be at least 5 characters.");
        }
    }
}
