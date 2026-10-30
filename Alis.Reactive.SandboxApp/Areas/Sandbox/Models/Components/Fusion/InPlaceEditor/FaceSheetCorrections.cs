using Alis.Reactive.Fusion.Components;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InPlaceEditor
{
    // Each face-sheet row is corrected in place and saved on its own: its own model, validator and plan.

    public class DateOfBirthCorrection
    {
        public string? ResidentId { get; set; }

        public DateTime? Value { get; set; }
    }

    public class DateOfBirthCorrectionValidator : ReactiveValidator<DateOfBirthCorrection>
    {
        public DateOfBirthCorrectionValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Enter her date of birth.")
                .LessThan(DateTime.Today).WithMessage("A date of birth must be before today.");
        }
    }

    public class CareLevelCorrection
    {
        public string? ResidentId { get; set; }

        public string? Value { get; set; }
    }

    public class CareLevelCorrectionValidator : ReactiveValidator<CareLevelCorrection>
    {
        public CareLevelCorrectionValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Choose a care level.");
        }
    }

    /// <summary>A care level the community offers, by name, with the monthly rate it carries.</summary>
    public sealed class CareLevelChoice
    {
        public string Name { get; set; } = "";

        public decimal MonthlyRate { get; set; }

        public static IReadOnlyList<CareLevelChoice> All { get; } =
        [
            new() { Name = "Independent Living", MonthlyRate = 3200m },
            new() { Name = "Assisted Living", MonthlyRate = 4500m },
            new() { Name = "Memory Care", MonthlyRate = 6200m },
            new() { Name = "Skilled Nursing", MonthlyRate = 8900m }
        ];
    }

    public class MonthlyRateCorrection
    {
        public string? ResidentId { get; set; }

        public decimal Value { get; set; }

        /// <summary>A rate other than the one her care level carries.</summary>
        public bool CustomRate { get; set; }
    }

    // The page checks the rate's range; only billing knows the band her care level allows.
    public class MonthlyRateCorrectionValidator : ReactiveValidator<MonthlyRateCorrection>
    {
        public MonthlyRateCorrectionValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0m).WithMessage("Enter a monthly rate above $0.")
                .LessThanOrEqualTo(25000m).WithMessage("A monthly rate cannot be above $25,000.");
            RuleFor(x => x.Value)
                .Must(rate => rate is >= 3800m and <= 5500m)
                .WithMessage("Assisted Living rates run from $3,800 to $5,500 a month.");
        }
    }

    /// <summary>The editor's own commit data, sent as it raised it: the field's name and the new value.</summary>
    public sealed class MonthlyRateChange
    {
        public string? ResidentId { get; set; }

        public FusionInPlaceEditorSubmission? Change { get; set; }
    }

    /// <summary>What billing answers when it accepts a rate: the rate as it keeps it.</summary>
    public sealed class MonthlyRateSaved
    {
        public string Rate { get; set; } = "";

        public string Message { get; set; } = "";
    }

    public class LastHospitalStayCorrection
    {
        public string? ResidentId { get; set; }

        public DateTime? Value { get; set; }
    }

    public class LastHospitalStayCorrectionValidator : ReactiveValidator<LastHospitalStayCorrection>
    {
        public LastHospitalStayCorrectionValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Enter when her last hospital stay began.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("A hospital stay cannot begin in the future.");
        }
    }

    public class MedicalRecordNumberCorrection
    {
        public string? ResidentId { get; set; }

        public string? Value { get; set; }
    }

    public class MedicalRecordNumberCorrectionValidator : ReactiveValidator<MedicalRecordNumberCorrection>
    {
        public MedicalRecordNumberCorrectionValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Enter her medical record number.")
                .Matches(@"^[A-Z]{3}\d{4}$").WithMessage("A medical record number is three letters and four digits.");
        }
    }

    /// <summary>What a face-sheet row answers when its correction is saved.</summary>
    public sealed class CorrectionSaved
    {
        public string Message { get; set; } = "";
    }

    /// <summary>Why the record refused a correction it could not take.</summary>
    public sealed class CorrectionRefused
    {
        public string Message { get; set; } = "";
    }
}
