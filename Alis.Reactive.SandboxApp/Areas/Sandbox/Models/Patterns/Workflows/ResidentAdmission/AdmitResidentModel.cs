using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.ResidentAdmission
{
    /// <summary>A new resident the admissions coordinator admits to Maple Grove.</summary>
    public class AdmitResidentModel
    {
        public string? ResidentName { get; set; }

        /// <summary>The attending physician, by the name the admission notice is addressed to.</summary>
        public string? Physician { get; set; }

        /// <summary>The care level's code; none until the coordinator chooses one.</summary>
        public string? CareLevel { get; set; }

        public decimal? MonthlyRate { get; set; }

        /// <summary>The resident moves in today rather than on a later scheduled day.</summary>
        public bool MovesInToday { get; set; }

        public string? Notes { get; set; }
    }

    /// <summary>A physician who attends Maple Grove's residents.</summary>
    /// <param name="Name">The name the search lists.</param>
    /// <param name="NameWithCredentials">The name the admission notice is addressed to, and the one the search matches.</param>
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

    /// <summary>A care level a resident is admitted to.</summary>
    public sealed record AdmissionCareLevel(string Code, string Name)
    {
        public static readonly AdmissionCareLevel[] All =
        [
            new("independent", "Independent Living"),
            new("assisted", "Assisted Living"),
            new("memory", "Memory Care")
        ];
    }

    /// <summary>
    /// What an admission cannot go without. The page checks the empty fields; whether the physician takes
    /// new residents is the server's to say.
    /// </summary>
    public class AdmitResidentValidator : ReactiveValidator<AdmitResidentModel>
    {
        // Dr. Chen's panel is full this month.
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

    /// <summary>The server's confirmation that the resident is admitted.</summary>
    public sealed record ResidentAdmitted(string Message);
}
