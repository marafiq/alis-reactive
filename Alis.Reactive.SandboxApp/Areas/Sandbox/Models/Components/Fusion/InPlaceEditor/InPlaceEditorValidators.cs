using System;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentProfileValidator : AbstractValidator<ResidentProfile>
    {
        public ResidentProfileValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
            RuleFor(x => x.CareLevelId).NotEmpty();
            RuleFor(x => x.AdmissionDate).NotNull().LessThanOrEqualTo(DateTime.Today);
            RuleFor(x => x.MonthlyRate).GreaterThan(0m).LessThanOrEqualTo(25000m);
            RuleFor(x => x.Nickname).MaximumLength(50);
            RuleFor(x => x.DateOfBirth).NotNull().LessThan(DateTime.Today);
            RuleFor(x => x.Allergies).MaximumLength(200);
        }
    }

    public class DateOfBirthQuickEditValidator : AbstractValidator<DateOfBirthQuickEdit>
    {
        public DateOfBirthQuickEditValidator()
        {
            RuleFor(x => x.Value).NotNull().LessThan(DateTime.Today);
        }
    }

    public class CareLevelQuickEditValidator : AbstractValidator<CareLevelQuickEdit>
    {
        public CareLevelQuickEditValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(40);
        }
    }

    public class MonthlyRateQuickEditValidator : AbstractValidator<MonthlyRateQuickEdit>
    {
        public MonthlyRateQuickEditValidator()
        {
            RuleFor(x => x.Value).GreaterThan(0m).LessThanOrEqualTo(25000m);
        }
    }

    public class NicknameQuickEditValidator : AbstractValidator<NicknameQuickEdit>
    {
        public NicknameQuickEditValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(50);
        }
    }
}
