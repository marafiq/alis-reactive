namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.ResidentAdmission
{
    public class AdmitResidentModel
    {
        public string? ResidentName { get; set; }

        public string? Physician { get; set; }

        public string? CareLevel { get; set; }

        public decimal? MonthlyRate { get; set; }

        public bool MovesInToday { get; set; }

        public string? Notes { get; set; }
    }

    public sealed record AttendingPhysician(string Name, string NameWithCredentials)
    {
        public static readonly AttendingPhysician[] All =
        [
            new("Dr. Alan Smith", "Alan Smith, MD"),
            new("Dr. Ruth Jones", "Ruth Jones, MD"),
            new("Dr. Priya Patel", "Priya Patel, MD"),
            new("Dr. Wei Chen", "Wei Chen, MD")
        ];
    }

    public sealed record AdmissionCareLevel(string Code, string Name)
    {
        public static readonly AdmissionCareLevel[] All =
        [
            new("independent", "Independent Living"),
            new("assisted", "Assisted Living"),
            new("memory", "Memory Care")
        ];
    }

    public class AdmitResidentValidator : ReactiveValidator<AdmitResidentModel>
    {
        private const string PanelFull = "Wei Chen, MD";

        public AdmitResidentValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
            RuleFor(x => x.Physician).NotEmpty().WithMessage("Choose the attending physician.")
                .Must(physician => physician != PanelFull)
                .WithMessage(admission => $"{admission.Physician} is not taking new residents this month.");
            RuleFor(x => x.MonthlyRate).NotEmpty().WithMessage("Enter the monthly rate.");
        }
    }

    public sealed record ResidentAdmitted(string Message);
}
