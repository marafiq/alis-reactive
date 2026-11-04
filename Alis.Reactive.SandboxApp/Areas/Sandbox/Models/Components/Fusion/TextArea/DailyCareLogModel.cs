namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.TextArea
{
    public class DailyCareLogModel
    {
        public string NightShiftNote { get; set; } = "";

        public string? CareNote { get; set; }

        public bool Signed { get; set; }
    }

    public class DailyCareLogValidator : ReactiveValidator<DailyCareLogModel>
    {
        public DailyCareLogValidator()
        {
            RuleFor(x => x.CareNote)
                .NotEmpty().WithMessage("Write how Margaret's day went before saving.")
                .MaximumLength(1000).WithMessage("Keep the note under 1,000 characters.");
            RuleFor(x => x.Signed).Equal(true).WithMessage("Sign the note before saving it.");
        }
    }

    public sealed class CareLogDraft
    {
        public string? Note { get; set; }
    }

    public sealed class CareLogEntry
    {
        public string? CareNote { get; set; }

        public bool Signed { get; set; }
    }

    public sealed class CareLogAnswer
    {
        public string Message { get; set; } = "";
    }
}
