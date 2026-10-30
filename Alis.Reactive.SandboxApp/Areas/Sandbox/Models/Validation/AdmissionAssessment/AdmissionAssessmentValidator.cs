using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The assessment's rules, written once. The page runs every one of them; the server runs the same
    /// ones on what the page sends, whether it checked first or not.
    /// </summary>
    public class AdmissionAssessmentValidator : ReactiveValidator<AdmissionAssessmentModel>
    {
        public static readonly DateTime FirstMoveInDay = new DateTime(2024, 1, 1);
        public static readonly DateTime FirstAssessmentDay = new DateTime(2024, 1, 1);
        public static readonly DateTime LastAssessmentDay = new DateTime(2026, 12, 31);
        public static readonly DateTime LastPlannedReviewDay = new DateTime(2027, 12, 31);

        public AdmissionAssessmentValidator()
        {
            RuleFor(x => x.ResidentName)
                .NotEmpty().WithMessage("Enter the resident's name.")
                .Length(2, 40).WithMessage("The resident's name is 2 to 40 characters.");
            RuleFor(x => x.PreferredName).MinimumLength(3).WithMessage("A preferred name is at least 3 characters.");
            RuleFor(x => x.WristbandLabel).MaximumLength(12).WithMessage("A wristband label is at most 12 characters.");
            RuleFor(x => x.RoomCode).Length(6).WithMessage("Enter the room as wing and number, like MA-214.");
            RuleFor(x => x.FamilyEmail).EmailAddress().WithMessage("Enter the family email as name@domain.");
            RuleFor(x => x.PortalLink).Url().WithMessage("The family portal link must be a web address.");
            RuleFor(x => x.ConsentWord).Equal("AGREE").WithMessage("Type AGREE to confirm the resident's consent.");
            RuleFor(x => x.SignaturesCollected).Equal(2).WithMessage("Two signatures are collected: the resident's and a witness's.");

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(55).WithMessage("Residents are 55 or older.")
                .LessThan(120).WithMessage("Check the age: 120 or more cannot be right.");
            RuleFor(x => x.WeightKg)
                .GreaterThan(20).WithMessage("Weight is recorded in kilograms, above 20.")
                .LessThanOrEqualTo(250).WithMessage("Weight is recorded in kilograms, up to 250.");
            RuleFor(x => x.PainScore).InclusiveBetween(0, 10).WithMessage("Pain is scored 0 to 10.");

            RuleFor(x => x.MoveInDate)
                .NotNull().WithMessage("Enter the move-in day.")
                .GreaterThanOrEqualTo(FirstMoveInDay).WithMessage("Move-in days start at January 1, 2024.");
            RuleFor(x => x.AssessmentDate)
                .InclusiveBetween(FirstAssessmentDay, LastAssessmentDay)
                .WithMessage("Date the assessment between January 1, 2024 and December 31, 2026.");
            RuleFor(x => x.NextReviewDate)
                .GreaterThan(x => x.AssessmentDate).WithMessage("The next review comes after the assessment day.")
                .LessThanOrEqualTo(LastPlannedReviewDay).WithMessage("Reviews are planned up to December 31, 2027.");

            RuleFor(x => x.CareLevel).NotNull().WithMessage("Choose a care level.");
            RuleFor(x => x.Residency).NotEmpty().WithMessage("Say whether the stay is permanent or respite.");
            RuleFor(x => x.MobilityAid).NotEqual(MobilityAidKind.NotAssessed).WithMessage("Assess the mobility aid.");
            RuleFor(x => x.FellInLastYear).NotNull().WithMessage("Answer whether the resident fell in the last year.");
            RuleFor(x => x.ConsentOnFile).Equal(true).WithMessage("Confirm the signed consent is on file.");

            // NotNull on a yes/no box and on a count: the server reads an unticked box as no and an
            // empty count as zero, and both are values.
            RuleFor(x => x.PhotoConsent).NotNull().WithMessage("Say whether the resident consents to photos.");
            RuleFor(x => x.FallsLastYear).NotNull().WithMessage("Record the number of falls in the last year.");
        }
    }
}
